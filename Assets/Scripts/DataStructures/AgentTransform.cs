using System;
using UnityEngine;

namespace DataStructures
{
    public struct AgentTransform : IEquatable<AgentTransform>
    {
        public Vector3 Position;
        public Quaternion Rotation;
        public bool Equals(AgentTransform other)
        {
            return Position == other.Position;
        }
    }
    
    public struct AgentMotion
    {
        public float Speed;
        public Vector3 Velocity;
    }
}