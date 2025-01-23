using UnityEngine;

namespace DataStructures
{
    public struct AgentTransform
    {
        public Vector3 Position;
        public Quaternion Rotation;
    }
    
    public struct AgentMotion
    {
        public float Speed;
        public Vector3 Velocity;
    }
}