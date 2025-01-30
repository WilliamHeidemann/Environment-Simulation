using System;
using Unity.Cinemachine;
using UnityEngine;

public class ViewSection : MonoBehaviour
{
    public CinemachineCamera Cinemachine;

    private void Start()
    {
        Cinemachine = FindFirstObjectByType<CinemachineCamera>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<ShepherdControls>(out _))
        {
            Cinemachine.Target.TrackingTarget = transform;
        }
    }
}
