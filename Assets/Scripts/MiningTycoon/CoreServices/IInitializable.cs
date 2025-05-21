using Cysharp.Threading.Tasks;

namespace MiningTycoon.CoreServices
{
    public interface IInitializable
    {
        UniTask Initialize();
    }
}