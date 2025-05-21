using MiningTycoon.Visualization;
using UnityEngine;

namespace MiningTycoon.Settings
{
    [CreateAssetMenu(fileName = "GraphSettings", menuName = "MiningTycoon/Graph Settings")]
    public class GraphSettings : ScriptableObject
    {
        [field: SerializeField] public NodeView NodePrefab { get; private set; }
        [field: SerializeField] public EdgeView EdgePrefab { get; private set; }
        [field: SerializeField] public StationView StationPrefab { get; private set; }
        [field: SerializeField] public MineView MinePrefab { get; private set; }
        [field: SerializeField] public TrainView TrainPrefab { get; private set; }
    }
}