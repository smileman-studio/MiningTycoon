using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using MiningTycoon.CoreServices;
using MiningTycoon.GraphEntities;
using MiningTycoon.Settings;
using MiningTycoon.Transport;
using Random = UnityEngine.Random;

namespace MiningTycoon.RuntimeData
{
    public class MapRuntimeData : IInitializable
    {
        private PathfindingService pathfindingService;
        
        private Dictionary<int, Node> nodes;
        private Dictionary<int, Edge> edges;
        private Dictionary<int, Dictionary<int, int>> nodeEdges;
        private Dictionary<int, Station> stations;
        private Dictionary<int, Mine> mines;
        private Dictionary<int, Dictionary<int, PathData>> pathsDictionary;
        
        public event Action OnChanged;
        public event Action OnMapLoaded;
        
        public IEnumerable<Node> Nodes => nodes.Values;
        public IEnumerable<Edge> Edges => edges.Values;
        public IEnumerable<Station> Stations => stations.Values;
        public IEnumerable<Mine> Mines => mines.Values;
        
        public UniTask Initialize()
        {
            pathfindingService = Services.Resolve<PathfindingService>();
            return UniTask.CompletedTask;
        }
        
        public Node GetNode(int id) => nodes.TryGetValue(id, out Node node) ? node : throw new NullReferenceException();
        public Edge GetEdge(int id) => edges.TryGetValue(id, out Edge edge) ? edge : throw new NullReferenceException();
        public Station GetStation(int id) => stations.TryGetValue(id, out Station station) ? station : throw new NullReferenceException();
        public Mine GetMine(int id) => mines.TryGetValue(id, out Mine mine) ? mine : throw new NullReferenceException();
        
        public bool TryGetEdge(int nodeA, int nodeB, out Edge edge)
        {
            edge = default;
            if(!nodeEdges.TryGetValue(nodeA, out Dictionary<int, int> edgeIDs))
                return false;
            if(!edgeIDs.TryGetValue(nodeB, out int edgeID))
                return false;
            return edges.TryGetValue(edgeID, out edge);
        }
        public bool TryGetPath(int stationId, int mineId, out PathData pathData)
        {
            pathData = default;
            if (!pathsDictionary.TryGetValue(stationId, out Dictionary<int, PathData> paths))
                return false;
            if (!paths.TryGetValue(mineId, out pathData))
                return false;
            return true;
        }
        public bool TryGetRandomEdge(out Edge edge)
        {
            edge = default;
            if (edges == null || edges.Count == 0)
                return false;
            edge = edges.ElementAt(Random.Range(0, edges.Count)).Value;
            return true;
        }

        public void SetMap(MapSettings mapSettings)
        {
            Clear();
            foreach (var node in mapSettings.Graph.Nodes)
            {
                nodes.Add(node.ID, node);
            }
            foreach (var edge in mapSettings.Graph.Edges)
            {
                edges.Add(edge.ID, edge);
                nodeEdges.TryAdd(edge.NodeA, new Dictionary<int, int>());
                nodeEdges.TryAdd(edge.NodeB, new Dictionary<int, int>());
                nodeEdges[edge.NodeA][edge.NodeB] = edge.ID;
                nodeEdges[edge.NodeB][edge.NodeA] = edge.ID;
            }
            foreach (var station in mapSettings.Stations)
            {
                stations.Add(station.NodeId, station);
            }
            foreach (var mine in mapSettings.Mines)
            {
                mines.Add(mine.NodeId, mine);
            }

            FindPaths();
            OnMapLoaded?.Invoke();
        }

        private void Clear()
        {
            nodes = new();
            edges = new();
            nodeEdges = new();
            stations = new();
            mines = new();
            pathsDictionary = new();
        }

        private void FindPaths()
        {
            pathfindingService.SetEdges(edges, nodeEdges);
            pathsDictionary = pathfindingService.FindPaths(stations.Keys.ToList(), mines.Keys.ToList());
        }

        public void ChangeMine(Mine mine)
        {
            if (!mines.ContainsKey(mine.NodeId))
            {
                throw new NullReferenceException();
            }
            mines[mine.NodeId] = mine;
            OnChanged?.Invoke();
        }
        
        public void ChangeStation(Station station)
        {
            if (!stations.ContainsKey(station.NodeId))
            {
                throw new NullReferenceException();
            }
            stations[station.NodeId] = station;
            OnChanged?.Invoke();
        }
        
        public void ChangeEdge(Edge edge)
        {
            if (!edges.ContainsKey(edge.ID))
            {
                throw new NullReferenceException();
            }
            edges[edge.ID] = edge;
            FindPaths();
            OnChanged?.Invoke();
        }
    }
}