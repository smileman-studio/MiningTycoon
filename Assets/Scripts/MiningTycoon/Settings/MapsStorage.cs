using System.Collections.Generic;
using UnityEngine;

namespace MiningTycoon.Settings
{
    [CreateAssetMenu(fileName = "MapsStorage", menuName = "MiningTycoon/Maps Storage")]
    public class MapsStorage : ScriptableObject
    {
        [SerializeField] private List<MapSettings> maps;
        
        public MapSettings DefaultMap => maps.Count > 0 ? maps[0] : null;
    }
}