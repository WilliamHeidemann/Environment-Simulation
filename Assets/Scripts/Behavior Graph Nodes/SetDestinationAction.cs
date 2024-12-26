using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using Random = UnityEngine.Random;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Set Destination", story: "Sets the [destination] based on [self] and [collection]", category: "Action", id: "213ebe4a68548c0bb3070e02d354f57f")]
public partial class SetDestinationAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Destination;
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    [SerializeReference] public BlackboardVariable<List<GameObject>> Collection;

    private const float NearbyDistance = 20f;
    private const float SeparationDistance = 4f;
    
    private const float CohesionStrength = 1f;
    private const float AlignmentStrength = 1f;
    private const float SeparationStrength = 1f;
    
    private bool _hasSetDestination = false;
    
    protected override Status OnStart()
    {
        if (Destination.Value == null)
        {
            return Status.Failure;
        }
        
        if (Self.Value == null)
        {
            return Status.Failure;
        }

        if (Collection.Value == null)
        {
            return Status.Failure;
        }
        
        if (!_hasSetDestination)
        {
            Destination.Value.transform.position = Self.Value.transform.position + Utility.UnitCirclePosition();
            _hasSetDestination = true;
        }
        
        return Status.Running;
    }



    protected override Status OnUpdate()
    {
        Destination.Value.transform.position += Utility.UnitCirclePosition();

        // Vector3 selfPosition = Self.Value.transform.position;
        // var nearbyAgents = Collection.Value
        //     // .Where(agent =>
        //     // {
        //     //     float distance = Vector3.Distance(selfPosition, agent.transform.position);
        //     //     return distance < NearbyDistance;
        //     // })
        //     .Select(agent => agent.transform);
        //
        // Vector3 centerOfNearbyAgents =
        //     nearbyAgents.Select(agent => agent.position)
        //         .Aggregate(Vector3.zero, (acc, position) => acc + position);
        // centerOfNearbyAgents /= nearbyAgents.Count();
        //
        // Debug.Log($"Center of nearby agents {centerOfNearbyAgents}");
        // Vector3 cohesion = (centerOfNearbyAgents - selfPosition)
        //     .normalized * CohesionStrength;
        //
        // Vector3 alignment = nearbyAgents
        //     .Aggregate(Vector3.zero, (acc, agent) => acc + agent.forward)
        //     .normalized * AlignmentStrength;
        //
        // Vector3 separation = nearbyAgents
        //     .Where(agent =>
        //     {
        //         float distance = Vector3.Distance(selfPosition, agent.transform.position);
        //         return distance < SeparationDistance;
        //     })
        //     .Select(agent => agent.transform.position)
        //     .Aggregate(Vector3.zero, (acc, position) => acc + position - selfPosition);
        //     // .normalized * SeparationStrength;
        //
        // Debug.Log($"cohesion: {cohesion.magnitude} | lookDirection: {alignment.magnitude} | separation: {separation.magnitude}");
        // Debug.DrawLine(selfPosition, selfPosition + cohesion, Color.blue);
        // Debug.DrawLine(selfPosition, selfPosition + alignment, Color.yellow);
        // Debug.DrawLine(selfPosition, selfPosition + separation, Color.red);
        //
        // Destination.Value.transform.position += cohesion + alignment + separation;
        //
        return Status.Success;
    }

    protected override void OnEnd()
    {
    }
}

