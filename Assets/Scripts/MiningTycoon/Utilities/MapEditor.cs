#if UNITY_EDITOR
using System.Collections.Generic;
using MiningTycoon.GraphEntities;
using MiningTycoon.Settings;
using MiningTycoon.Visualization;
using UnityEditor;
using UnityEngine;

namespace MiningTycoon.Utilities
{
    [ExecuteInEditMode]
    public class MapEditor : MonoBehaviour
    {
        [SerializeField] private GraphSettings graphSettings;
        [SerializeField] private MapSettings mapSettings;
        
        private Dictionary<int, NodeView> nodes;
        private Dictionary<int, EdgeView> edges;
        private Dictionary<int, Dictionary<int, EdgeView>> nodeEdges;
        private Dictionary<int, StationView> stations;
        private Dictionary<int, MineView> mines;

        private void Awake()
        {
#if !UNITY_EDITOR
            Clear();
            Destroy(gameObject);
            return;
#endif
            if (Application.isPlaying)
            {
                Clear();
                Destroy(gameObject);
            }
        }

        [Button]
        private void LoadSettings()
        {
            Clear();
            transform.DestroyChildren();

            if(graphSettings == null || mapSettings == null)
                return;

            foreach (var node in mapSettings.Graph.Nodes)
            {
                CreateNodeView(node);
            }
            foreach (var edge in mapSettings.Graph.Edges)
            {
                CreateEdgeView(edge);
            }

            foreach (var station in mapSettings.Stations)
            {
                CreateStationView(station);
            }

            foreach (var mine in mapSettings.Mines)
            {
                CreateMineView(mine);
            }
        }

        private void Clear()
        {
            nodes = new();
            edges = new();
            nodeEdges = new();
            stations = new();
            mines = new();
        }

        private void CreateNodeView(Node node)
        {
            var nodeView = Instantiate(graphSettings.NodePrefab, transform);
            nodeView.name = $"Node {node.ID}";
            nodeView.transform.position = new Vector3(node.Position.x, 0, node.Position.y);
            nodeView.SetNode(node);
            nodeView.OnChanged += NodeViewChangedHandler;
            nodeView.OnCreateEdge += CreateNewEdge;
            nodeView.OnAddStation += NodeViewAddStationHandler;
            nodeView.OnAddMine += NodeViewAddMineHandler;
            nodeView.OnDestroyed += NodeViewDestroyedHandler;
            nodes.Add(node.ID, nodeView);
        }
        
        private void NodeViewChangedHandler(NodeView nodeView)
        {
            mapSettings.Graph.UpdateNode(nodeView.Node);
            SaveSettings();
        }

        private void NodeViewAddStationHandler(NodeView nodeView)
        {
            if(stations.ContainsKey(nodeView.Node.ID))
                return;
            nodeView.transform.DestroyChildren();
            var station = new Station()
            {
                NodeId = nodeView.Node.ID,
                Multiplier = 1,
            };
            mapSettings.AddStation(station);
            SaveSettings();
            CreateStationView(station);
        }

        private void CreateStationView(Station station)
        {
            if(!nodes.TryGetValue(station.NodeId, out var nodeView))
                return;
            var stationView = Instantiate(graphSettings.StationPrefab, nodeView.transform, true);
            stationView.transform.localPosition = Vector3.zero;
            stationView.SetStation(station);
            stationView.OnChanged += StationViewChangedHandler;
            stationView.OnDestroyed += StationViewDestroyedHandler;
            stations.Add(station.NodeId, stationView);
        }
        
        private void StationViewChangedHandler(StationView stationView)
        {
            mapSettings.UpdateStation(stationView.Station);
            SaveSettings();
        }
        
        private void StationViewDestroyedHandler(StationView stationView)
        {
            if(!stations.ContainsKey(stationView.Station.NodeId))
                return;
            stations.Remove(stationView.Station.NodeId);
            mapSettings.RemoveStation(stationView.Station);
            SaveSettings();
        }

        private void NodeViewAddMineHandler(NodeView nodeView)
        {
            if(mines.ContainsKey(nodeView.Node.ID))
                return;
            nodeView.transform.DestroyChildren();
            var mine = new Mine()
            {
                NodeId = nodeView.Node.ID,
                Multiplier = 1,
            };
            mapSettings.AddMine(mine);
            SaveSettings();
            CreateMineView(mine);
        }

        private void CreateMineView(Mine mine)
        {
            if(!nodes.TryGetValue(mine.NodeId, out var nodeView))
                return;
            var mineView = Instantiate(graphSettings.MinePrefab, nodeView.transform, true);
            mineView.transform.localPosition = Vector3.zero;
            mineView.SetMine(mine);
            mineView.OnChanged += MineViewChangedHandler;
            mineView.OnDestroyed += MineViewDestroyedHandler;
            mines.Add(mine.NodeId, mineView);
        }

        private void MineViewChangedHandler(MineView mineView)
        {
            mapSettings.UpdateMine(mineView.Mine);
            SaveSettings();
        }
        
        private void MineViewDestroyedHandler(MineView mineView)
        {
            if(!mines.ContainsKey(mineView.Mine.NodeId))
                return;
            mines.Remove(mineView.Mine.NodeId);
            mapSettings.RemoveMine(mineView.Mine);
            SaveSettings();
        }

        private void NodeViewDestroyedHandler(NodeView nodeView)
        {
            if(!nodes.ContainsKey(nodeView.Node.ID))
                return;
            nodes.Remove(nodeView.Node.ID);
            mapSettings.Graph.RemoveNode(nodeView.Node);
            if (nodeEdges.TryGetValue(nodeView.Node.ID, out var nodeNeighbours))
            {
                foreach (var edgeView in nodeNeighbours.Values)
                {
                    DestroyImmediate(edgeView.gameObject);
                }
            }
            SaveSettings();
        }

        private void CreateEdgeView(Edge edge)
        {
            if(!nodes.TryGetValue(edge.NodeA, out NodeView startNode) ||
               !nodes.TryGetValue(edge.NodeB, out NodeView endNode))
                return;
            var edgeView = Instantiate(graphSettings.EdgePrefab, transform);
            edgeView.name = $"Edge {edge.ID} [{startNode.Node.ID}, {endNode.Node.ID}]";
            edgeView.SetEdge(edge, nodes);
            edgeView.OnChanged += EdgeViewChangedHandler;
            edgeView.OnDestroyed += EdgeViewDestroyedHandler;
            edges.Add(edge.ID, edgeView);
            nodeEdges.TryAdd(edge.NodeA, new Dictionary<int, EdgeView>());
            nodeEdges.TryAdd(edge.NodeB, new Dictionary<int, EdgeView>());
            nodeEdges[edge.NodeA][edge.NodeB] = edgeView;
            nodeEdges[edge.NodeB][edge.NodeA] = edgeView;
        }

        private void EdgeViewChangedHandler(EdgeView edgeView)
        {
            mapSettings.Graph.UpdateEdge(edgeView.Edge);
            SaveSettings();
        }

        private void CreateNewEdge()
        {
            if(Selection.objects.Length != 2)
                return;
            if(Selection.objects[0] is not GameObject go1 || Selection.objects[1] is not GameObject go2)
                return;
            if(!go1.TryGetComponent(out NodeView nodeView1) || !go2.TryGetComponent(out NodeView nodeView2))
                return;
            
            if(nodeEdges.TryGetValue(nodeView1.Node.ID, out var nodeNeighbours) && nodeNeighbours.ContainsKey(nodeView2.Node.ID))
                return;
            int id = 1;
            for (; id <= edges.Count; id++)
            {
                if(!edges.ContainsKey(id))
                    break;
            }
            var edge = new Edge()
            {
                ID = id,
                NodeA = nodeView1.Node.ID,
                NodeB = nodeView2.Node.ID,
                Length = 1,
            };
            mapSettings.Graph.AddEdge(edge);
            SaveSettings();
            CreateEdgeView(edge);

        }

        private void EdgeViewDestroyedHandler(EdgeView edgeView)
        {
            if(!edges.ContainsKey(edgeView.Edge.ID))
                return;
            nodeEdges[edgeView.Edge.NodeA].Remove(edgeView.Edge.NodeB);
            nodeEdges[edgeView.Edge.NodeB].Remove(edgeView.Edge.NodeA);
            edges.Remove(edgeView.Edge.ID);
            mapSettings.Graph.RemoveEdge(edgeView.Edge);
            SaveSettings();
        }

        private void Update()
        {
            if (mapSettings != null && nodes == null)
            {
                Debug.Log($"ReloadSettings");
                LoadSettings();
            }
        }
        
        [Button]
        private void AddNode()
        {
            if(mapSettings == null)
                return;
            int id = 1;
            for (; id <= nodes.Count; id++)
            {
                if(!nodes.ContainsKey(id))
                    break;
            }
            Node node = new Node()
            {
                ID = id,
            };
            mapSettings.Graph.AddNode(node);
            SaveSettings();
            CreateNodeView(node);
        }

        private void SaveSettings()
        {
            EditorUtility.SetDirty(mapSettings);
            AssetDatabase.SaveAssets();
        }

        private void OnDestroy()
        {
            Clear();
        }
    }
    
}
#endif