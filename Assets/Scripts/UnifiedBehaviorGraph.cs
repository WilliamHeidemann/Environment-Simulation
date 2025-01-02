using UnityEngine;
using UtilityToolkit.Runtime;

public class UnifiedBehaviorGraph : MonoBehaviour
{
    [SerializeField] private AgentsData AgentsData;

    public void Update()
    {
        for (int i = 0; i < AgentsData.Agents.Count; i++)
        {
            Agent agent = AgentsData.Agents[i];
            if (agent.TargetPosition.IsSome(out Vector3 targetPosition))
            {
                if (Vector3.SqrMagnitude(AgentsData.Agents[i].Position - targetPosition) < 0.1f)
                {
                    agent.TargetPosition = Option<Vector3>.None;
                    agent.StartIdleTime = Time.time;
                }
            }
            else if (Time.time - agent.StartIdleTime > 0f)
            {
                Vector3 position = agent.Position + Utility.RandomOnCircle(5f);
                agent.TargetPosition = Option<Vector3>.Some(position);
            }
            AgentsData.Agents[i] = agent;
        }
    }
}
