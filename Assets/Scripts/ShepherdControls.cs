using UnityEngine;
using UnityEngine.Serialization;

public class ShepherdControls : MonoBehaviour
{
    [SerializeField] private float _translationSpeed;
    private void Update()
    {
        float rotation = Input.GetAxis("Horizontal") * Time.deltaTime * 100.0f;
        transform.Rotate(0, rotation, 0);
        
        float translation = Input.GetAxis("Vertical") * Time.deltaTime * _translationSpeed;
        transform.Translate(0, 0, translation);
    }
}
