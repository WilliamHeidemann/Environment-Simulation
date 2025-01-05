using UnityEngine;

public class ShepherdControls : MonoBehaviour
{
    private void Update()
    {
        float rotation = Input.GetAxis("Horizontal") * Time.deltaTime * 100.0f;
        transform.Rotate(0, rotation, 0);
        
        float translation = Input.GetAxis("Vertical") * Time.deltaTime * 15.0f;
        transform.Translate(0, 0, translation);
    }
}
