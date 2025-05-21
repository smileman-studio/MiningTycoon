using System;
using System.Collections.Generic;
using MiningTycoon.GraphEntities;
using UnityEngine;

namespace MiningTycoon.Settings
{
    [CreateAssetMenu(fileName = "MapSettings", menuName = "MiningTycoon/Map Settings")]
    public class MapSettings : ScriptableObject
    {
        [field: SerializeField] public Graph Graph { get; private set; }
        [SerializeField] private List<Station> stations;
        [SerializeField] private List<Mine> mines;

        public IReadOnlyList<Station> Stations => stations.AsReadOnly();
        public IReadOnlyList<Mine> Mines => mines.AsReadOnly();

        public void AddStation(Station station) => stations.Add(station);
        public void RemoveStation(Station station) => stations.Remove(station);

        public void UpdateStation(Station station)
        {
            try
            {
                stations[stations.IndexOf(station)] = station;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        public void AddMine(Mine mine) => mines.Add(mine);
        public void RemoveMine(Mine mine) => mines.Remove(mine);

        public void UpdateMine(Mine mine)
        {
            try
            {
                mines[mines.IndexOf(mine)] = mine;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
    }
}