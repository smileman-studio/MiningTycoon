using System;
using System.Collections.Generic;
using UnityEngine;

namespace MiningTycoon.GraphEntities
{
    [Serializable]
    public class Graph
    {
        [SerializeField] private List<Node> nodes;
        [SerializeField] private List<Edge> edges;
        
        public IReadOnlyList<Node> Nodes => nodes.AsReadOnly();
        public IReadOnlyList<Edge> Edges => edges.AsReadOnly();
        
        public void AddNode(Node node) => nodes.Add(node);
        public void RemoveNode(Node node) => nodes.Remove(node);

        public void UpdateNode(Node node)
        {
            try
            {
                nodes[nodes.IndexOf(node)] = node;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
        
        public void AddEdge(Edge edge) => edges.Add(edge);
        public void RemoveEdge(Edge edge) => edges.Remove(edge);

        public void UpdateEdge(Edge edge)
        {
            try
            {
                edges[edges.IndexOf(edge)] = edge;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
    }
}