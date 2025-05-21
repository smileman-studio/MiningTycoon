using System;

namespace MiningTycoon.RuntimeData
{
    public class ResourcesRuntimeData
    {
        public event Action OnChanged;
        
        public int ResourceCount { get; private set; }

        public void AddResource(int amount)
        {
            ResourceCount += amount;
            OnChanged?.Invoke();
        }
    }
}