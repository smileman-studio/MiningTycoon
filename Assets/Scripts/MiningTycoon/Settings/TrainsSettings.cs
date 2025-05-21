using System.Collections.Generic;
using UnityEngine;

namespace MiningTycoon.Settings
{
    [CreateAssetMenu(fileName = "TrainsSettings", menuName = "MiningTycoon/TrainsSettings")]
    public class TrainsSettings : ScriptableObject
    {
        [field: SerializeField] public int DefaultCapacity { get; private set; }
        [field: SerializeField] public Vector2 MinMaxMovementSpeed { get; private set; }
        [field: SerializeField] public Vector2 MinMaxMiningDuration { get; private set; }
        [SerializeField] private List<TrainBaseData> defaultTrains;
        
        public IReadOnlyList<TrainBaseData> DefaultTrains => defaultTrains.AsReadOnly();
    }
}