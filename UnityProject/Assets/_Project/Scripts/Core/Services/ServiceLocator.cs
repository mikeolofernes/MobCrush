using System;
using System.Collections.Generic;

namespace MobCrush.Core.Services
{
    /// <summary>
    /// Minimal type-keyed service registry.
    /// why: a reflection-free alternative to a DI container — zero startup cost on mobile,
    /// trivially debuggable, and consumers still depend on interfaces so tests can inject fakes.
    /// Rule (Loop 3 §4): only composition roots (bootstrap / scene installers) may call this;
    /// gameplay classes receive their dependencies as references.
    /// </summary>
    public static class ServiceLocator
    {
        private static readonly Dictionary<Type, object> Services = new();

        /// <summary>Registers a service instance for interface type T. Last registration wins (supports test overrides).</summary>
        public static void Register<T>(T service) where T : class
        {
            Services[typeof(T)] = service ?? throw new ArgumentNullException(nameof(service));
        }

        /// <summary>Resolves a service; throws with a clear message if bootstrap order is wrong.</summary>
        public static T Get<T>() where T : class
        {
            if (Services.TryGetValue(typeof(T), out var service))
                return (T)service;
            throw new InvalidOperationException($"Service {typeof(T).Name} not registered. Check GameBootstrap order.");
        }

        public static bool TryGet<T>(out T service) where T : class
        {
            if (Services.TryGetValue(typeof(T), out var obj))
            {
                service = (T)obj;
                return true;
            }
            service = null;
            return false;
        }

        /// <summary>Clears all registrations. why: needed between play-mode test runs (domain reload disabled).</summary>
        public static void Reset() => Services.Clear();
    }
}
