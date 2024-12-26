using UnityEngine;
using UtilityToolkit.Runtime;

public class Walk : MonoBehaviour
{
    [SerializeField] private float RotationSpeed;
    [SerializeField] private float TranslationSpeed;
    private Option<Transform> _target;

    public bool ShouldWalk { get; set; }
    
    
    private void Update()
    {
        if (!ShouldWalk)
        {
            return;
        }
        
        if (!_target.IsSome(out Transform target))
        {
            return;
        }

        Rotate(target);
        Translate(target);
    }
    
    public void SetTarget(Transform target)
    {
        _target = Option<Transform>.Some(target);
    }

    private void Rotate(Transform target)
    {
        float angle = Quaternion.Angle(
            transform.rotation,
            Quaternion.LookRotation(target.transform.position - transform.position));

        if (angle > 0.1f)
        {
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                Quaternion.LookRotation(target.transform.position - transform.position),
                RotationSpeed * Time.deltaTime);
        }
    }

    private void Translate(Transform target)
    {
        Vector3 direction = (target.transform.position - transform.position).normalized;
        float squareDistance = direction.sqrMagnitude;

        if (squareDistance > 0.1f)
        {
            Vector3 distanceToMove = direction.normalized * (TranslationSpeed * Time.deltaTime);
            transform.position += distanceToMove;
        }
    }
}