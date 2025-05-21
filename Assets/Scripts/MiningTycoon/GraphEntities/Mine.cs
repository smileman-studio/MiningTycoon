using System;

namespace MiningTycoon.GraphEntities
{
    [Serializable]
    public struct Mine : IEquatable<Mine>
    {
        public int NodeId;
        public float Multiplier;
        
        public bool Equals(Mine other)
        {
            return NodeId == other.NodeId;
        }

        public override bool Equals(object obj)
        {
            return obj is Mine other && Equals(other);
        }

        public override int GetHashCode()
        {
            return NodeId;
        }
    }
}