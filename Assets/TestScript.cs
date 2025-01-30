using Unity.Cinemachine;
using UnityEngine;

public class TestScript : MonoBehaviour
{
    CinemachineFollow _cinemachineFollow;
    [SerializeField] Transform _target;
    private float _zOffset;
    [SerializeField] private float _upwardSpeed;
    [SerializeField] private float _backwardSpeed;

    private void Start()
    {
        _cinemachineFollow = FindFirstObjectByType<CinemachineFollow>();
        _zOffset = -_target.transform.position.z;
    }

    private void Update()
    {
        float z = _target.position.z + _zOffset;
        _cinemachineFollow.FollowOffset = new Vector3(0, z * _upwardSpeed * _upwardSpeed, -z * _backwardSpeed * _backwardSpeed);
    }
}
