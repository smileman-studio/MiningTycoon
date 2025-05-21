using System;

namespace MiningTycoon.GraphEntities
{
    [Serializable]
    public struct Station : IEquatable<Station>
    {
        public int NodeId;
        public int Multiplier;
        
        public bool Equals(Station other)
        {
            return NodeId == other.NodeId;
        }

        public override bool Equals(object obj)
        {
            return obj is Station other && Equals(other);
        }

        public override int GetHashCode()
        {
            return NodeId;
        }
    }
}