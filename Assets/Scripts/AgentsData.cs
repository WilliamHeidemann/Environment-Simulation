using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Create AgentsData", fileName = "AgentsData", order = 0)]
public class AgentsData : ScriptableObject
{
    public List<Agent> Agents;
}