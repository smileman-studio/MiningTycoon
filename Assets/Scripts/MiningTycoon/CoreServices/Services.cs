using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using MiningTycoon.Visualization;
using UnityEngine;

namespace MiningTycoon.CoreServices
{
    public class Services : MonoBehaviour
    {
        private static Services instance;
        private Dictionary<Type, object> services;

        public static event Action OnInitializationComplete;

        private async void Awake()
        {
            instance = this;
            await Initialize();
            Debug.Log($"Services initialization complete.");
        }

        private async Task Initialize()
        {
            instance.services = new Dictionary<Type, object>();
            BindInterfaces(new AddressablesAssetsLoading());
            await new SettingsLoader().Initialize();
            
            Bind(new MapService());
            Bind(new PathfindingService());
            Bind(new TransportService());
            Bind(new ProductionService());
            Bind(new VisualizationService());
            
            List<UniTask> tasks = new();
            foreach (var service in services.Values)
            {
                if (service is IInitializable implementation)
                {
                    tasks.Add(implementation.Initialize());
                }
            }

            await UniTask.WhenAll(tasks);
            OnInitializationComplete?.Invoke();
        }

        public static T Bind<T>(T service)
        {
            if (instance == null || service == null)
                throw new NullReferenceException();
            var services = instance.services;
            Type key = service.GetType();
            services[key] = service;
            return service;
        }
        
        public static void BindInterfaces(object service)
        {
            if (instance == null || service == null)
                throw new NullReferenceException();
            var services = instance.services;
            var interfaces = GetInterfaces(service.GetType());
            foreach (var type in interfaces)
            {
                if(type == typeof(IInitializable))
                    continue;
                services[type] = service;
            }
        }
        
        private static Type[] GetInterfaces(Type type)
        {
#if UNITY_WSA && ENABLE_DOTNET && !UNITY_EDITOR
                return type.GetTypeInfo().ImplementedInterfaces.ToArray();
#else
            return type.GetInterfaces();
#endif
        }

        public static T Resolve<T>()
        {
            if (!instance.services.TryGetValue(typeof(T), out object service))
                throw new NullReferenceException($"Service {typeof(T)} not found");
            return (T) service;
        }
        
        public static object Resolve(Type serviceType)
        {
            if (!instance.services.TryGetValue(serviceType, out object service))
                throw new NullReferenceException($"Service {serviceType} not found");
            return service;
        }

        public static bool Contains<T>()
        {
            return instance != null && instance.services != null && instance.services.ContainsKey(typeof(T));
        }
    }
}