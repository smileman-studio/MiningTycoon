using System;
using UnityEngine;

namespace MiningTycoon.GraphEntities
{
    [Serializable]
    public struct Node : IEquatable<Node>
    {
        public Vector2 Position;
        public int ID;

        public bool Equals(Node other)
        {
            return ID == other.ID;
        }

        public override bool Equals(object obj)
        {
            return obj is Node other && Equals(other);
        }

        public override int GetHashCode()
        {
            return ID;
        }
    }
}