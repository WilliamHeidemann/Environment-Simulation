using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Create AgentsData", fileName = "AgentsData", order = 0)]
public class AgentsData : ScriptableObject
{
    public List<Agent> Agents;
    public ProximityGraph ProximityGraph;
}

public class ProximityGraph
{
    public readonly Dictionary<Agent, HashSet<Edge>> Close = new();
    public readonly Dictionary<Agent, HashSet<Edge>> TooClose = new();
}

public struct Edge : IEquatable<Edge>
{
    public float SquareDistance;
    public Vector3 SeparationVector;
    public Vector3 EndPosition;
    public Vector3 EndForward;
    public int Index;

    public bool Equals(Edge other)
    {
        return Index == other.Index;
    }

    public override bool Equals(object obj)
    {
        return obj is Edge other && Equals(other);
    }

    public override int GetHashCode()
    {
        return Index;
    }

    public static bool operator ==(Edge left, Edge right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Edge left, Edge right)
    {
        return !left.Equals(right);
    }
}