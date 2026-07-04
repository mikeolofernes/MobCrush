# Loop 20 — Save System (hardening)

**Status:** Complete — this closes Alpha (M2) feature scope.

## What Loop 4 already guaranteed (recap)
Versioned JSON schema, ordered `ISaveMigration` chain, atomic writes (tmp → backup rotate → move), backup fallback, corrupt-file → fresh-model resilience, cloud-ready single blob with `LastSavedUtcTicks`.

## This loop's additions
- **`AesPayloadTransform`** — AES-256-CBC through the `IPayloadTransform` seam designed in Loop 4 (zero changes to SaveService logic). Random IV per write, `base64(IV+cipher)` format, key = SHA-256(appSalt + deviceId) — no key storage, saves don't trivially copy between devices. **Honesty note in-code:** this deters casual editing; it is not DRM. Bonus: legacy plaintext saves pass through Decode once and re-write encrypted — free in-place migration.
- **Multiple saves** — bootstrap now creates `slot_<n>.sav` (0–2); each slot is an independent SaveService (file + backup + encryption). Slot switching = re-bootstrap with a different index; a slot-picker UI is a menu concern for later.
- **`AutosaveController`** — run end, `OnApplicationPause(true)` (often the last code to run before the OS kills a mobile process) and quit. Mutation-site saves (wallet/equipment/talents) already cover the transactional moments.
- **Versioning** — unchanged mechanism, now verified end-to-end with encryption in the path (migrations operate on decrypted JObject).

## Deliverables
✅ Encryption, multiple saves, autosave, versioning, cloud-save-ready.

## Folder Structure (added)
```
Scripts/Core/Save/{AesPayloadTransform,AutosaveController}.cs
```
(+ GameBootstrap wiring: slot file, AES transform, autosave component.)

## Scripts
2 new, 1 touched.

## Assets Needed
None.

## Risks
- Device-bound key means an OS-level device transfer breaks local saves — acceptable offline trade-off; real transfer arrives with cloud save (the blob is ready; only the transport/auth is future work).
- `deviceUniqueIdentifier` can change on reinstall on some Android configs — the plaintext-passthrough fallback prevents data loss from key mismatch only for legacy saves; cloud backup remains the durable answer (Loop 23+).

## Testing Checklist
- [x] Round-trip: encrypt → decrypt → identical model; unique IV per write.
- [x] Legacy plaintext save loads once and re-saves encrypted.
- [x] Kill-app-mid-write leaves a loadable file (tmp/backup sequence).
- [x] Slots are fully isolated (separate files, backups, models).

## What Comes Next
Loop 21 — mobile optimization pass (profiler checklist, allocation audit, batching/Addressables verification).
