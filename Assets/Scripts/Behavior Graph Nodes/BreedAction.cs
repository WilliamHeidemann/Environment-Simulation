using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Breed", story: "Breed [Lamb] from [self]", category: "Action", id: "95992f71026df633736c1f56d747cc88")]
public partial class BreedAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Lamb;
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    protected override Status OnStart()
    {
        if (Lamb.Value == null)
        {
            return Status.Failure;
        }

        if (Self.Value == null)
        {
            return Status.Failure;
        }
        
        Vector3 spawnPosition = Self.Value.transform.position + Utility.RandomOnCircle(radius: 5f);
        Vector3 lookDirection = spawnPosition - Self.Value.transform.position;
        Quaternion spawnRotation = Quaternion.LookRotation(lookDirection, Vector3.up);
        
        Object.Instantiate(Lamb.Value, spawnPosition, spawnRotation);
        
        return Status.Success;
    }

    protected override Status OnUpdate()
    {
        return Status.Success;
    }

    protected override void OnEnd()
    {
    }
}

