using System;
using UnityEngine;
using UtilityToolkit.Runtime;

public class Walk : MonoBehaviour
{
    [SerializeField] private float RotationSpeed;
    [SerializeField] private float TranslationSpeed;
    public Option<Vector3> Target { get; set; }
    public event Action OnTargetReached;
    
    private void Update()
    {
        if (!Target.IsSome(out Vector3 target))
        {
            return;
        }

        Rotate(target);
        Translate(target);
        
        if (Vector3.SqrMagnitude(transform.position - target) < 0.1f)
        {
            Target = Option<Vector3>.None;
            OnTargetReached?.Invoke();
        }
    }

    private void Rotate(Vector3 target)
    {
        float angle = Quaternion.Angle(
            transform.rotation,
            Quaternion.LookRotation(transform.position - target));

        if (angle > 0.1f)
        {
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                Quaternion.LookRotation(transform.position - target),
                RotationSpeed * Time.deltaTime);
        }
    }

    private void Translate(Vector3 target)
    {
        Vector3 direction = (target - transform.position).normalized;
        float squareDistance = direction.sqrMagnitude;

        if (squareDistance > 0.1f)
        {
            Vector3 distanceToMove = direction.normalized * (TranslationSpeed * Time.deltaTime);
            transform.position += distanceToMove;
        }
    }
}