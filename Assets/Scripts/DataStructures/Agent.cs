using UnityEngine;
using UtilityToolkit.Runtime;

public class Agent
{
    public Vector3 Position;
    public Quaternion Rotation;
    public Vector3 Scale;
    public Vector3 TargetPosition;
    public float Speed;
    public Behavior Behavior;
    public DebugCohesion DebugCohesion;
    public int Index;
}

public enum Behavior
{
    Wandering,
    Flocking
}

public enum DebugCohesion
{
    NoCohesion,
    LowCohesion,
    MiddleCohesion,
    HighCohesion
}