using System;
using System.Collections.Generic;
using System.Linq;
using MiningTycoon.CoreServices;
using MiningTycoon.GraphEntities;
using MiningTycoon.RuntimeData;
using MiningTycoon.Settings;
using MiningTycoon.Transport;
using UnityEngine;

namespace MiningTycoon.Visualization
{
    public class TrainView : MonoBehaviour
    {
        [SerializeField] private float movementSpeed;
        [SerializeField] private float mineDuration;
        
        private MapRuntimeData mapRuntimeData;
        
        public Train Train { get; private set; }
        public TrainBaseData ChangedTrainData { get; private set; }
        public event Action<TrainView> OnChanged;

        public void SetTrain(Train train)
        {
            mapRuntimeData ??= Services.Resolve<MapRuntimeData>();
            Train = train;
            movementSpeed = Train.BaseData.MovementSpeed;
            mineDuration = Train.BaseData.MiningDuration;
            SetPosition();
        }

        private void SetPosition()
        {
            Node startNode = mapRuntimeData.GetNode(Train.EdgePosition.FromNodeID);
            Node endNode = mapRuntimeData.GetNode(Train.EdgePosition.ToNodeID);
            Vector2 pos = startNode.Position + (endNode.Position - startNode.Position) * Train.EdgePosition.Position;
            transform.position = new Vector3(pos.x, 0, pos.y);
        }

        private void Update()
        {
            SetPosition();
            if (!Mathf.Approximately(movementSpeed, Train.BaseData.MovementSpeed) || 
                !Mathf.Approximately(mineDuration, Train.BaseData.MiningDuration))
            {
                ChangedTrainData = new TrainBaseData()
                {
                    Capacity = ChangedTrainData.Capacity,
                    MovementSpeed = movementSpeed,
                    MiningDuration = mineDuration,
                };
                OnChanged?.Invoke(this);
            }
        }

        private void OnDrawGizmosSelected()
        {
            if(Train == null)
                return;
            IReadOnlyList<int> path;
            switch (Train.State)
            {
                case TrainState.MoveToRoute:
                    path = Train.PathToRoute.Path;
                    break;
                case TrainState.MoveToMine:
                    path = Train.Route.Path;
                    break;
                case TrainState.MoveToStation:
                    path = Train.Route.Path.Reverse().ToList();
                    break;
                default:
                    return;
            }
           
            Gizmos.color = Color.green;
            for (int i = path.Count - 1; i > 0; i--)
            {
                var node = mapRuntimeData.GetNode(path[i]);
                var prevNode = mapRuntimeData.GetNode(path[i - 1]);
                if (Train.EdgePosition.FromNodeID == prevNode.ID && Train.EdgePosition.ToNodeID == node.ID)
                {
                    Gizmos.DrawLine(new Vector3(node.Position.x, 0, node.Position.y), transform.position);
                    break;
                }
                Gizmos.DrawLine(new Vector3(node.Position.x, 0, node.Position.y),
                    new Vector3(prevNode.Position.x, 0, prevNode.Position.y));
            }
        }
    }
}