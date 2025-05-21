using System;

namespace MiningTycoon.GraphEntities
{
    [Serializable]
    public struct Edge : IEquatable<Edge>
    {
        public int ID;
        public int NodeA;
        public int NodeB;
        public float Length;
        
        public bool Equals(Edge other)
        {
            return ID == other.ID;
        }

        public override bool Equals(object obj)
        {
            return obj is Edge other && Equals(other);
        }

        public override int GetHashCode()
        {
            return ID;
        }
    }
}