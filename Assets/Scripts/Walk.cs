using UnityEngine;
using UtilityToolkit.Runtime;

public class Walk : MonoBehaviour
{
    [SerializeField] private float RotationSpeed;
    [SerializeField] private float TranslationSpeed;
    public Option<Transform> Target { get; set; }

    public bool ShouldWalk { get; set; }
    
    
    private void Update()
    {
        if (!ShouldWalk)
        {
            return;
        }
        
        if (!Target.IsSome(out Transform target))
        {
            return;
        }

        Rotate(target);
        Translate(target);
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