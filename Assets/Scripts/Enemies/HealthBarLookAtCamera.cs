using UnityEngine;

public class HealthBarLookAtCamera : MonoBehaviour
{
    private Camera cam;

    private void Start()
    {
        cam = Camera.main;
    }

    private void LateUpdate()
    {
        if (cam == null)
            return;

        // Pasek pozostaje dzieckiem przeciwnika,
        // ale sam obraca się w stronę kamery.
        transform.rotation = cam.transform.rotation;
    }
}