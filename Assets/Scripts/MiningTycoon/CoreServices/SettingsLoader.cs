using Cysharp.Threading.Tasks;
using MiningTycoon.RuntimeData;
using MiningTycoon.Settings;
using UnityEngine;

namespace MiningTycoon.CoreServices
{
    public class SettingsLoader
    {
        private IAssetsLoadingService assetsLoadingService;
        
        public async UniTask Initialize()
        {
            assetsLoadingService = Services.Resolve<IAssetsLoadingService>();
            await LoadAsset<GraphSettings>();
            await LoadAsset<TrainsSettings>();
            await LoadAsset<MapsStorage>();
            
            InstantiateData<MapRuntimeData>();
            InstantiateData<TrainsRuntimeData>();
            InstantiateData<ResourcesRuntimeData>();
        }

        private async UniTask LoadAsset<T>() where T : Object
        {
            var asset = await assetsLoadingService.LoadAsset<T>();
            Services.Bind(asset);
        }
        
        private void InstantiateData<T>() where T : new()
        {
            var data = new T();
            Services.Bind(data);
        }
    }
}