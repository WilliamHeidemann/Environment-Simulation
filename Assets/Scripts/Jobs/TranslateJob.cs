using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Jobs
{
    public struct TranslateJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<Vector3> TargetPositions;
        [ReadOnly] public NativeArray<float> AgentSpeeds;
        [ReadOnly] public float DeltaTime;
        public NativeArray<Vector3> AgentPositions;
        public NativeArray<Quaternion> AgentRotations;

        private const float RotationSpeed = 100f;
        private const float TranslationSpeed = 0.2f;

        public void Execute(int index)
        {
            if (AgentSpeeds[index] < 0.1f)
            {
                return;
            }
        
            Vector3 direction = TargetPositions[index] - AgentPositions[index];
            if (direction.sqrMagnitude < 0.1f)
            {
                return;
            }

            Rotate(index, direction);
            Translate(index, direction);
        }

        private void Rotate(int index, Vector3 direction)
        {
            Quaternion destinationRotation = Quaternion.LookRotation(direction);
            float angle = Quaternion.Angle(AgentRotations[index], destinationRotation);

            if (angle > 1f)
            {
                AgentRotations[index] =
                    Quaternion.RotateTowards(AgentRotations[index], destinationRotation, RotationSpeed * DeltaTime);
            }
        }

        private void Translate(int index, Vector3 direction)
        {
            Vector3 distanceToMove = direction.normalized * (AgentSpeeds[index] * TranslationSpeed * DeltaTime);
            AgentPositions[index] += distanceToMove;
        }
    }
}