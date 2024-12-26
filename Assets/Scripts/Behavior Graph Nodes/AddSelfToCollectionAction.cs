using System;
using System.Collections.Generic;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Add Self to Collection", story: "Add [Self] to [Collection]", category: "Action", id: "2ba6ac7cf14dcdad69cbfbc16fdae8eb")]
public partial class AddSelfToCollectionAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    [SerializeReference] public BlackboardVariable<List<GameObject>> Collection;
    protected override Status OnStart()
    {
        Collection.Value.Add(Self.Value);
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

