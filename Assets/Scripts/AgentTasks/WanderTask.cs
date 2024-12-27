using UnityEngine;

public class WanderTask : AgentTask
{
    private readonly Transform _transform;
    private readonly Vector3 _targetPosition;
    
    private const float RotationSpeed = 200f;
    private const float TranslationSpeed = 2f;
    
    public WanderTask(Transform transform, Vector3 targetPosition)
    {
        _transform = transform;
        _targetPosition = targetPosition;
    }

    public override void Update()
    {
        if (IsCompleted)
        {
            return;
        }
        
        Rotate(_targetPosition);
        Translate(_targetPosition);
        
        if (Vector3.SqrMagnitude(_transform.position - _targetPosition) < 0.1f)
        {
            IsCompleted = true;
        }
    }

    private void Rotate(Vector3 target)
    {
        float angle = Quaternion.Angle(
            _transform.rotation,
            Quaternion.LookRotation(_transform.position - target));

        if (angle > 0.1f)
        {
            _transform.rotation = Quaternion.RotateTowards(
                _transform.rotation,
                Quaternion.LookRotation(_transform.position - target),
                RotationSpeed * Time.deltaTime);
        }
    }

    private void Translate(Vector3 target)
    {
        Vector3 direction = target - _transform.position;
        float squareDistance = direction.sqrMagnitude;

        if (squareDistance > 0.1f)
        {
            Vector3 distanceToMove = direction.normalized * (TranslationSpeed * Time.deltaTime);
            _transform.position += distanceToMove;
        }
    }
}