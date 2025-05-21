using Cysharp.Threading.Tasks;
using MiningTycoon.RuntimeData;

namespace MiningTycoon.CoreServices
{
    public class ProductionService : IInitializable
    {
        private ResourcesRuntimeData resourcesData;
        private TransportService transportService;
        
        public UniTask Initialize()
        {
            resourcesData = Services.Resolve<ResourcesRuntimeData>();
            transportService = Services.Resolve<TransportService>();
            transportService.OnResourceDelivered += ResourceDeliveredHandler;
            return UniTask.CompletedTask;
        }

        private void ResourceDeliveredHandler(int amount)
        {
            resourcesData.AddResource(amount);
        }
    }
}