using System;
using System.Collections.Generic;
using MiningTycoon.Transport;

namespace MiningTycoon.RuntimeData
{
    public class TrainsRuntimeData
    {
        private List<Train> trains;

        public event Action OnSetTrains;
        
        public IReadOnlyList<Train> Trains => trains?.AsReadOnly();

        public void SetTrains(List<Train> trains)
        {
            this.trains = trains;
            OnSetTrains?.Invoke();
        }
    }
}