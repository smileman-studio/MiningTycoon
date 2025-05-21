using System.Collections.Generic;
using MiningTycoon.GraphEntities;
using MiningTycoon.Transport;

namespace MiningTycoon.CoreServices
{
    public class PathfindingService
    {
        private Dictionary<int, Edge> edges; 
        private Dictionary<int, Dictionary<int, int>> nodeEdges;
        
        public class Vertex 
        {
            public readonly int ID;
            public readonly float Cost;
            public readonly Vertex Parent;

            public Vertex(int id, float cost = 0, Vertex parent = null)
            {
                ID = id;
                Cost = cost;
                Parent = parent;
            }

            private bool Equals(Vertex other)
            {
                return ID == other.ID;
            }

            public override bool Equals(object obj)
            {
                if (obj is null) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != GetType()) return false;
                return Equals((Vertex)obj);
            }

            public override int GetHashCode()
            {
                return ID;
            }
        }

        public void SetEdges(Dictionary<int, Edge> edges, Dictionary<int, Dictionary<int, int>> nodeEdges)
        {
            this.edges = edges;
            this.nodeEdges = nodeEdges;
        }

        public Dictionary<int, Dictionary<int, PathData>> FindPaths(List<int> startNodes, List<int> endNodes)
        {
            Dictionary<int, Dictionary<int, PathData>> paths = new();
            foreach (var startNode in startNodes)
            {
                paths.Add(startNode, new Dictionary<int, PathData>());
                foreach (var endNode in endNodes)
                {
                    var pathData = FindPath(startNode, endNode);
                    if(pathData.Path != null)
                        paths[startNode].Add(endNode, pathData); 
                }
            }

            return paths;
        }

        public PathData FindPath(int startNodeID, int endNodeID)
        {
            List<Vertex> openSet = new();
            Dictionary<int, Vertex> closedSet = new();
            openSet.Add(new Vertex(startNodeID));
            while (openSet.Count > 0)
            {
                Vertex currentVertex = openSet[0];
                openSet.Remove(currentVertex);
                closedSet.Add(currentVertex.ID, currentVertex);

                if (currentVertex.ID == endNodeID)
                {
                    return RetracePath(currentVertex);
                }

                if (nodeEdges.TryGetValue(currentVertex.ID, out var neighbourNodes))
                {
                    foreach (var keyValue in neighbourNodes)
                    {
                        if (closedSet.ContainsKey(keyValue.Key))
                            continue;
                        int edgeID = keyValue.Value;
                        edges.TryGetValue(edgeID, out Edge edge);
                        var vertex = new Vertex(keyValue.Key, currentVertex.Cost + edge.Length, currentVertex);
                        var exist = openSet.Find(v => v.ID == keyValue.Key);
                        if (exist != null)
                        {
                            if(exist.Cost < vertex.Cost)
                                continue;
                            openSet.Remove(exist);
                        }
                        int index = openSet.Count;
                        for (; index > 0; index--)
                        {
                            if (openSet[index - 1].Cost <= vertex.Cost)
                                break;
                        }
                        if(index == openSet.Count)
                            openSet.Add(vertex);
                        else
                            openSet.Insert(index, vertex);
                    }
                }
            }

            return default;
        }

        private PathData RetracePath(Vertex endVertex)
        {
            List<int> path = new();
            Vertex currentVertex = endVertex;

            while (currentVertex.Parent != null)
            {
                path.Insert(0, currentVertex.ID);
                currentVertex = currentVertex.Parent;
            }
            path.Insert(0, currentVertex.ID);
            return new PathData(path, endVertex.Cost);
        }
    }
}