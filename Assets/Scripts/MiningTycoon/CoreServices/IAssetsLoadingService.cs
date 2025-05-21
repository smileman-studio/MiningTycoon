using Cysharp.Threading.Tasks;
using Object = UnityEngine.Object;

namespace MiningTycoon.CoreServices
{
    public interface IAssetsLoadingService
    {
        UniTask<T> LoadAsset<T>() where T : Object;

        UniTask<T> LoadAsset<T>(string key) where T : Object;
    }
}