using System;
using UnityEngine;
using UtilityToolkit.Editor;

public class ViewSectionController : MonoBehaviour
{
    [SerializeField] private Transform _target;
    [SerializeField] private Vector3 _boundingBoxSize;
    [SerializeField] private BoxCollider _viewSectionPrefab;

    [Button]
    public void SetViewSections()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }
        
        for (int i = 0; i < 10; i++)
        {
            for (int j = 0; j < 10; j++)
            {
                float x = _boundingBoxSize.x * j + j;
                float z = _boundingBoxSize.z * i + i;
                BoxCollider viewSection = Instantiate(_viewSectionPrefab, new Vector3(x, 0, z), Quaternion.identity, transform);
                viewSection.size = _boundingBoxSize;
                viewSection.isTrigger = true;
            }
        }
    }
}
