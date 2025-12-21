using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Passage : MonoBehaviour
{
    public Transform connection;

    private void OnTriggerEnter(Collider other)
    {
        Vector3 position = connection.position;
        position.y = other.transform.position.y;
        other.transform.position = position;
    }

}
