using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Spawn Grass Patch", story: "Spawn [Grass] Patch", category: "Action", id: "ef0a5fd02b37f5b85eab8d65635b9e21")]
public partial class SpawnGrassPatchAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Grass;
    private Vector3 _lastSpawnPosition = Vector3.zero;
    
    protected override Status OnStart()
    {
        if (Grass.Value == null)
        {
            return Status.Failure;
        }

        _lastSpawnPosition += Utility.RandomOnCircle(radius: 5f);
        Object.Instantiate(Grass.Value, _lastSpawnPosition, Quaternion.identity);
        
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

