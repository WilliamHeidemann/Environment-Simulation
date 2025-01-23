using Unity.Collections;
using UnityEngine;

namespace DataStructures
{
    [CreateAssetMenu(menuName = "Create AgentsData", fileName = "AgentsData", order = 0)]
    public class AgentsData : ScriptableObject
    {
        public NativeArray<AgentTransform> Transforms;
        public NativeArray<AgentMotion> Motions;
    }
}