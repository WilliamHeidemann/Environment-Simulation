using UnityEngine;

public class GrowUp : MonoBehaviour
{
    private async void Start()
    {
        await Awaitable.WaitForSecondsAsync(10f);
        Destroy(this);
    }
}
