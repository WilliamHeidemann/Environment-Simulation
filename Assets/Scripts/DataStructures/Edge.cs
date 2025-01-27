using System;
using Unity.Mathematics;
using UnityEngine;

namespace DataStructures
{
    public struct Edge
    {
        public float SquareDistance;
        public float3 SeparationVector;
        public float3 EndPosition;
        public float3 EndForward;
    }
}