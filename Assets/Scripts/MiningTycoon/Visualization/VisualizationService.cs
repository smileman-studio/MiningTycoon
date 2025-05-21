using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using MiningTycoon.CoreServices;
using MiningTycoon.GraphEntities;
using MiningTycoon.RuntimeData;
using MiningTycoon.Settings;
using MiningTycoon.Transport;
using MiningTycoon.Utilities;
using UnityEngine;

namespace MiningTycoon.Visualization
{
    public class VisualizationService : IInitializable
    {
        private MapRuntimeData mapRuntimeData;
        private TrainsRuntimeData trainsRuntimeData;
        private TransportService transportService;
        private GraphSettings graphSettings;
        private Transform mapContainer;
        private Transform trainsContainer;
        private Dictionary<int, NodeView> nodes;
        private Dictionary<int, EdgeView> edges;
        
        public UniTask Initialize()
        {
            mapRuntimeData = Services.Resolve<MapRuntimeData>();
            trainsRuntimeData = Services.Resolve<TrainsRuntimeData>();
            transportService = Services.Resolve<TransportService>();
            graphSettings = Services.Resolve<GraphSettings>();
            mapRuntimeData.OnMapLoaded += MapLoadedHandler;
            trainsRuntimeData.OnSetTrains += SetTrainsHandler;
            mapContainer = new GameObject("Map").transform;
            trainsContainer = new GameObject("Trains").transform;
            new GameObject($"ResourcesGUI").AddComponent<ResourcesGUI>();
            return UniTask.CompletedTask;
        }

        private void MapLoadedHandler()
        {
            mapContainer.DestroyChildren();
            nodes = new();
            foreach (var node in mapRuntimeData.Nodes)
            {
                CreateNodeView(node);
            }
            foreach (var edge in mapRuntimeData.Edges)
            {
                CreateEdgeView(edge);
            }

            foreach (var station in mapRuntimeData.Stations)
            {
                CreateStationView(station);
            }

            foreach (var mine in mapRuntimeData.Mines)
            {
                CreateMineView(mine);
            }
        }
        
        private void CreateNodeView(Node node)
        {
            var nodeView = Object.Instantiate(graphSettings.NodePrefab, mapContainer);
            nodeView.name = $"Node {node.ID}";
            nodeView.transform.position = new Vector3(node.Position.x, 0, node.Position.y);
            nodeView.SetNode(node);
            nodes.Add(node.ID, nodeView);
        }
        
        private void CreateEdgeView(Edge edge)
        {
            if(!nodes.TryGetValue(edge.NodeA, out NodeView startNode) ||
               !nodes.TryGetValue(edge.NodeB, out NodeView endNode))
                return;
            var edgeView = Object.Instantiate(graphSettings.EdgePrefab, mapContainer);
            edgeView.name = $"Edge {edge.ID} [{startNode.Node.ID}, {endNode.Node.ID}]";
            edgeView.SetEdge(edge, nodes);
            edgeView.OnChanged += EdgeViewChangedHandler;
        }
        
        private void CreateStationView(Station station)
        {
            if(!nodes.TryGetValue(station.NodeId, out var nodeView))
                return;
            var stationView = Object.Instantiate(graphSettings.StationPrefab, nodeView.transform, true);
            stationView.transform.localPosition = Vector3.zero;
            stationView.SetStation(station);
            stationView.OnChanged += StationViewChangedHandler;
        }
        
        private void CreateMineView(Mine mine)
        {
            if(!nodes.TryGetValue(mine.NodeId, out var nodeView))
                return;
            var mineView = Object.Instantiate(graphSettings.MinePrefab, nodeView.transform, true);
            mineView.transform.localPosition = Vector3.zero;
            mineView.SetMine(mine);
            mineView.OnChanged += MineViewChangedHandler;
        }

        private void SetTrainsHandler()
        {
            trainsContainer.DestroyChildren();
            foreach (var train in trainsRuntimeData.Trains)
            {
                CreateTrainView(train);
            }
        }

        private void CreateTrainView(Train train)
        {
            var trainView = Object.Instantiate(graphSettings.TrainPrefab, trainsContainer);
            trainView.SetTrain(train);
            trainView.OnChanged += TrainViewChangedHandler;
        }
        
        private void MineViewChangedHandler(MineView mineView)
        {
            mapRuntimeData.ChangeMine(mineView.Mine);
        }
        
        private void StationViewChangedHandler(StationView stationView)
        {
            mapRuntimeData.ChangeStation(stationView.Station);
        }
        
        private void EdgeViewChangedHandler(EdgeView edgeView)
        {
            mapRuntimeData.ChangeEdge(edgeView.Edge);
        }
        
        private void TrainViewChangedHandler(TrainView trainView)
        {
            transportService.ChangeTrain(trainView.Train, trainView.ChangedTrainData);
        }
    }
}