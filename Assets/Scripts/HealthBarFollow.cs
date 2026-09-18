using UnityEngine;

public class HealthBarFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0, 2.5f, 0);

    void LateUpdate()
    {
        if (target != null)
        {
            transform.position = target.position + offset;

            // pasek zawsze patrzy do kamery
            transform.rotation = Camera.main.transform.rotation;
        }
    }
}