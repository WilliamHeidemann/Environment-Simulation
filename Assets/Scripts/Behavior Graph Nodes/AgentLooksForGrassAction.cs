using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Agent looks for grass", story: "[Agent] looks for [Grass]", category: "Action", id: "1849ed8fb485508d736ae54e92319671")]
public partial class AgentLooksForGrassAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    [SerializeReference] public BlackboardVariable<GameObject> Grass;
    private IEnumerable<Vector3> _grass;
    
    protected override Status OnStart()
    {
        var allGrass = GameObject.FindGameObjectsWithTag("Grass");
        
        if (allGrass.Length == 0)
        {
            return Status.Failure;
        }
        
        Vector3 agentPosition = Agent.Value.transform.position;

        (GameObject grass, Vector3 position) = allGrass
            .Select(grass => (grass, grass.transform.position))
            .OrderBy(pair => Vector3.SqrMagnitude(pair.position - agentPosition))
            .First();
        
        float proximity = Vector3.SqrMagnitude(position - agentPosition);

        if (proximity < 5f)
        {
            Grass.Value = grass;
            return Status.Success;
        }
        
        return Status.Failure;
    }

    protected override Status OnUpdate()
    {
        return Status.Success;
    }

    protected override void OnEnd()
    {
    }
}

