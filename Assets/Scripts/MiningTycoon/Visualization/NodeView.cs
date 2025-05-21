using System;
using MiningTycoon.GraphEntities;
using MiningTycoon.Utilities;
using UnityEngine;

namespace MiningTycoon.Visualization
{
    [ExecuteInEditMode]
    public class NodeView : MonoBehaviour
    {
        private int nodeId;
        private const float PositionAccuracy = 0.001f;

        private Node node;
        public Node Node => node;
        public event Action<NodeView> OnChanged; 
        public event Action<NodeView> OnDestroyed; 

        public void SetNode(Node node)
        {
            this.node = node;
        }

        private void Update()
        {
            var pos = transform.position;
            if (Math.Abs(pos.x - Node.Position.x) > PositionAccuracy ||
                Math.Abs(pos.z - Node.Position.y) > PositionAccuracy)
            {
                node.Position = new Vector2(pos.x, pos.z);
                OnChanged?.Invoke(this);
            }
        }

        private void OnDestroy()
        {
            OnDestroyed?.Invoke(this);
        }

#if UNITY_EDITOR
        public event Action OnCreateEdge;
        public event Action<NodeView> OnAddStation;
        public event Action<NodeView> OnAddMine;
        
        [Button]
        private void CreateEdge()
        {
            OnCreateEdge?.Invoke();
        }

        [Button]
        private void AddStation()
        {
            OnAddStation?.Invoke(this);
        }
        
        [Button]
        private void AddMine()
        {
            OnAddMine?.Invoke(this);
        }
#endif
    }
}