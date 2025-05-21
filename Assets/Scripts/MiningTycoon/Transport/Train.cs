using System;
using System.Linq;
using MiningTycoon.CoreServices;
using MiningTycoon.GraphEntities;
using MiningTycoon.RuntimeData;
using MiningTycoon.Settings;
using UnityEngine;

namespace MiningTycoon.Transport
{
    public class Train
    {
        private readonly PathfindingService pathfindingService;
        private readonly MapRuntimeData mapRuntimeData;
        private TrainEdgePosition edgePosition;
        public int StationID => Route.Path[0];
        public int MineID => Route.Path[^1];
        
        public PathData PathToRoute { get; private set; }
        public bool IsFull { get; private set; }
        public TrainState State { get; private set; }
        public PathData Route { get; private set; }
        public TrainEdgePosition EdgePosition => edgePosition;
        public TrainBaseData BaseData { get; private set; }
        public float MineProgress { get; private set; }
        public event Action<Train> OnUnloaded;

        public Train(TrainBaseData baseData, Edge edge, float position)
        {
            pathfindingService = Services.Resolve<PathfindingService>();
            mapRuntimeData = Services.Resolve<MapRuntimeData>();
            BaseData = baseData;
            edgePosition = new TrainEdgePosition()
            {
                EdgeID = edge.ID,
                FromNodeID = edge.NodeA,
                ToNodeID = edge.NodeB,
                Position = position
            };
        }

        public void SetRoute(PathData path)
        {
            if (Route.Path != null && Route.Path.Count == path.Path.Count)
            {
                bool changed = false;
                for (int i = 0; i < Route.Path.Count; i++)
                {
                    if (Route.Path[i] != path.Path[i])
                    {
                        changed = true;
                        break;
                    }
                }

                if (!changed)
                    return;
            }
            Route = path;
            if(!TryGetPathToRoute())
                return;
            State = TrainState.MoveToRoute;
        }

        public void Update(float deltaTime)
        {
            switch (State)
            {
                case TrainState.None:
                    if(!TryGetPathToRoute())
                        return;
                    State = TrainState.MoveToRoute;
                    break;
                case TrainState.MoveToRoute:
                    if (Move(deltaTime, PathToRoute))
                    {
                        State = IsFull ? TrainState.Unloading : TrainState.Mine;
                        PathToRoute = default;
                    }
                    break;
                case TrainState.MoveToMine:
                    if (Move(deltaTime, Route))
                    {
                        State = TrainState.Mine;
                    }
                    break;
                case TrainState.MoveToStation:
                    if (Move(deltaTime, Route, true))
                    {
                        State = TrainState.Unloading;
                    }
                    break;
                case TrainState.Mine:
                    var mine = mapRuntimeData.GetMine(MineID);
                    MineProgress += deltaTime / BaseData.MiningDuration / mine.Multiplier;
                    if (MineProgress >= 1)
                    {
                        MineProgress = 0;
                        IsFull = true;
                        State = TrainState.MoveToStation;
                        edgePosition.FromNodeID = MineID;
                        edgePosition.ToNodeID = Route.Path[^2];
                        edgePosition.Position = 0;
                    }
                    break;
                case TrainState.Unloading:
                    IsFull = false;
                    State = TrainState.MoveToMine;
                    edgePosition.FromNodeID = StationID;
                    edgePosition.ToNodeID = Route.Path[1];
                    edgePosition.Position = 0;
                    OnUnloaded?.Invoke(this);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private bool TryGetPathToRoute()
        {
            try
            {
                int targetNode = IsFull ? StationID : MineID;
                PathData fromStartPath = pathfindingService.FindPath(EdgePosition.FromNodeID, targetNode);
                PathData fromEndPath = pathfindingService.FindPath(EdgePosition.ToNodeID, targetNode);
                PathData bestPath;
                Edge edge = mapRuntimeData.GetEdge(EdgePosition.EdgeID);
                float edgeLength = edge.Length;
                if (fromStartPath.Distance + EdgePosition.Position * edgeLength >
                    fromEndPath.Distance + (1 - EdgePosition.Position) * edgeLength)
                {
                    var path = fromEndPath.Path.ToList();
                    path.Insert(0, EdgePosition.FromNodeID);
                    bestPath = new PathData(path, fromEndPath.Distance + edgeLength);
                }
                else
                {
                    var path = fromStartPath.Path.ToList();
                    path.Insert(0, EdgePosition.ToNodeID);
                    bestPath = new PathData(path, fromStartPath.Distance + edgeLength); 
                }

                PathToRoute = bestPath;
                edgePosition = new TrainEdgePosition()
                {
                    EdgeID = edge.ID,
                    FromNodeID = PathToRoute.Path[0],
                    ToNodeID = PathToRoute.Path[1],
                    Position = edgePosition.FromNodeID == PathToRoute.Path[0] ? EdgePosition.Position : 1 - EdgePosition.Position,
                };
                return true;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Train cant find path to {(IsFull ? "station" : "mine")} e:{e.Message}");
                return false;
            }
        }

        private bool Move(float deltaTime, PathData pathData, bool reverse = false)
        {
            float traveledDistance = BaseData.MovementSpeed * deltaTime;
            while (traveledDistance > 0)
            {
                float edgeLength = mapRuntimeData.GetEdge(EdgePosition.EdgeID).Length;
                float edgeRemainingDistance = edgeLength * (1 - EdgePosition.Position);
                if (traveledDistance < edgeRemainingDistance)
                {
                    edgePosition.Position += traveledDistance / edgeLength;
                    traveledDistance = 0;
                }
                else
                {
                    traveledDistance -= edgeRemainingDistance;
                    var path = pathData.Path;
                    
                    for (int i = 0; i < path.Count - 1; i++)
                    {
                        int index = reverse ? path.Count - 1 - i : i;
                        int index1 = reverse ? index - 1 : index + 1;
                        int index2 = reverse ? index - 2 : index + 2;
                        if (EdgePosition.FromNodeID == path[index] && EdgePosition.ToNodeID == path[index1])
                        {
                            if (index == (reverse ? 1 : path.Count - 2))
                            {
                                edgePosition.Position = 1;
                                return true;
                            }
                            int nextStart = path[index1];
                            int nextEnd = path[index2];
                            if(!mapRuntimeData.TryGetEdge(nextStart, nextEnd, out Edge edge))
                            {
                                Debug.LogError($"Edge {nextStart}:{nextEnd} not found");
                                return false;
                            }

                            edgePosition = new TrainEdgePosition()
                            {
                                EdgeID = edge.ID,
                                FromNodeID = nextStart,
                                ToNodeID = nextEnd,
                                Position = 0
                            };
                            break;
                        }
                    }
                }
            }

            return false;
        }

        public void ChangeBaseData(TrainBaseData baseData)
        {
            BaseData = baseData;
        }
    }
}