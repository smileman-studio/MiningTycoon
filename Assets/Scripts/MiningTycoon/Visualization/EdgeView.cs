using System;
using System.Collections.Generic;
using MiningTycoon.GraphEntities;
using UnityEngine;

namespace MiningTycoon.Visualization
{
    [ExecuteInEditMode]
    public class EdgeView : MonoBehaviour
    {
        [SerializeField] private float length;
        
        private NodeView startNode;
        private NodeView endNode;
        private Vector3 center;

        private Edge edge;
        public Edge Edge => edge;
        public event Action<EdgeView> OnChanged;
        public event Action<EdgeView> OnDestroyed;
        
        public void SetEdge(Edge edge, Dictionary<int, NodeView> nodes)
        {
            this.edge = edge;
            startNode = nodes[edge.NodeA];
            endNode = nodes[edge.NodeB];
            startNode.OnChanged += UpdateEdge;
            endNode.OnChanged += UpdateEdge;
            length = edge.Length;

            UpdateEdge(null);
        }

        private void UpdateEdge(NodeView _)
        {
            transform.position = startNode.transform.position;
            var direction = endNode.transform.position - startNode.transform.position;
            transform.localScale = new Vector3(1, 1, direction.magnitude);
            float angle = GetAngle(direction.z, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, angle, 0);
            center = startNode.transform.position + direction * 0.5f;
        }
        
        private static float GetAngle(float x, float y)
        {
            float atan = Mathf.Atan(y / x);
            if (x > 0 & y >= 0)
            {
                return atan;
            }

            if (x > 0 & y < 0)
            {
                return atan + Mathf.PI * 2f;
            }

            if (x < 0)
                return atan + Mathf.PI;
            if (x == 0)
            {
                if (y == 0)
                    return 0;
                return y > 0 ? Mathf.PI * 0.5f : Mathf.PI * 1.5f;
            }

            return 0;
        }

        private void Update()
        {
            if (!Mathf.Approximately(length, edge.Length))
            {
                edge.Length = length;
                OnChanged?.Invoke(this);
            }
        }

        private void OnGUI()
        {
            var mainCamera = Camera.main;
            if(mainCamera == null)
                return;
            float size = 100;
            var viewportPoint = mainCamera.WorldToViewportPoint(center);
            var rect = new Rect(Screen.width * viewportPoint.x - size * 0.5f,
                Screen.height * (1 - viewportPoint.y) - size * 0.5f, size, size);
            var style = new GUIStyle
            {
                fontSize = 30,
                alignment = TextAnchor.MiddleCenter
            };
            GUI.Label(rect, $"{edge.Length}", style);
        }

        private void OnDestroy()
        {
            OnDestroyed?.Invoke(this);
            if(startNode != null)
                startNode.OnChanged -= UpdateEdge;
            if(endNode != null)
                endNode.OnChanged -= UpdateEdge;
        }
    }
}