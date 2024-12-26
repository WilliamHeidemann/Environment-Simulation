using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Look for other sheep", story: "[Self] look for other [sheep]", category: "Action", id: "ad87b5cc2899b53f26a5d13b016f51b4")]
public partial class LookForOtherSheepAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    [SerializeReference] public BlackboardVariable<List<GameObject>> Sheep;
    private float _lastSeenSheep;

    protected override Status OnStart()
    {
        if (Sheep.Value == null)
        {
            return Status.Failure;
        }
        
        if (Sheep.Value.Count == 0)
        {
            return Status.Failure;
        }
        
        if (_lastSeenSheep < Time.time - 20f)
        {
            return Status.Failure;
        }

        _lastSeenSheep = Time.time;
        
        Vector3 agentPosition = Self.Value.transform.position;
        
        Vector3 position = Sheep.Value
            .Select(sheep => sheep.transform.position)
            .OrderBy(position => Vector3.SqrMagnitude(position - agentPosition))
            .First();
        
        float proximity = Vector3.SqrMagnitude(position - agentPosition);

        if (proximity < 5f)
        {
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

