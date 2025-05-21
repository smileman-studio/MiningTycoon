using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Object = UnityEngine.Object;

namespace MiningTycoon.CoreServices
{
    public class AddressablesAssetsLoading : IAssetsLoadingService
    {
        public async UniTask<T> LoadAsset<T>() where T : Object
        {
            if (typeof(T).IsSubclassOf(typeof(Component)))
            {
                var result = await LoadAsset<GameObject>(typeof(T).Name);
                if (result.TryGetComponent(out T component))
                    return component;
                Debug.LogError($"Asset {result.name} has no component {typeof(T).Name}");
                return null;
            }
            return await LoadAsset<T>(typeof(T).Name);
        }

        public async UniTask<T> LoadAsset<T>(string key) where T : Object
        {
            var handle = Addressables.LoadAssetAsync<T>(key);
            await UniTask.WaitUntil(()=>handle.IsDone);

            if (handle.Status != AsyncOperationStatus.Succeeded)
                throw new ArgumentException($"Cant load asset {key}");
            return handle.Result;
        }
    }
}