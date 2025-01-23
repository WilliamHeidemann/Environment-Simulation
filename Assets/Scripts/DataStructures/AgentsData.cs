using System.Collections.Generic;
using DataStructures;
using UnityEngine;

[CreateAssetMenu(menuName = "Create AgentsData", fileName = "AgentsData", order = 0)]
public class AgentsData : ScriptableObject
{
    public List<AgentTransform> Transforms;
    public List<AgentMotion> Motions;
}