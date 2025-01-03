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

public class Edge : IEquatable<Edge>
{
    public Agent Other;
    public float SquareDistance;
    public Vector3 SeparationVector;

    public bool Equals(Edge other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Equals(Other, other.Other);
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((Edge)obj);
    }

    public override int GetHashCode()
    {
        return (Other != null ? Other.GetHashCode() : 0);
    }

    public static bool operator ==(Edge left, Edge right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(Edge left, Edge right)
    {
        return !Equals(left, right);
    }
}