using Cysharp.Threading.Tasks;
using MiningTycoon.RuntimeData;
using MiningTycoon.Settings;

namespace MiningTycoon.CoreServices
{
    public class MapService : IInitializable
    {
        private MapRuntimeData mapRuntimeData;
        private MapsStorage mapsStorage;
        
        public UniTask Initialize()
        {
            mapsStorage = Services.Resolve<MapsStorage>();
            mapRuntimeData = Services.Resolve<MapRuntimeData>();
            Services.OnInitializationComplete += ServicesInitializationCompleteHandler;
            return UniTask.CompletedTask;
        }

        private void ServicesInitializationCompleteHandler()
        {
            Services.OnInitializationComplete -= ServicesInitializationCompleteHandler;
            LoadDefaultMap();
        }
        
        private void LoadDefaultMap()
        {
            var mapSettings = mapsStorage.DefaultMap;
            mapRuntimeData.SetMap(mapSettings);
        }
    }
}