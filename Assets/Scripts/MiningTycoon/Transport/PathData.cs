using System.Collections.Generic;

namespace MiningTycoon.Transport
{
    public readonly struct PathData
    {
        private readonly List<int> path;
        public readonly float Distance;

        public IReadOnlyList<int> Path => path?.AsReadOnly();
        
        public PathData(List<int> path, float distance)
        {
            this.path = path;
            Distance = distance;
        }
    }
}