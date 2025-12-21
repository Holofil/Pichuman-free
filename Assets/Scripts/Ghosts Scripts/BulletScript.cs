using UnityEngine;

public class BulletScript : MonoBehaviour
{
    [SerializeField] float BulletTimeToDestroy = 1f;

    private void Start()
    {
        Destroy(gameObject, BulletTimeToDestroy);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            Destroy(gameObject);
        }
        if (other.gameObject.layer == LayerMask.NameToLayer("Pacman"))
        {
            FindObjectOfType<GameManager>().PacmanEaten();
            Destroy(gameObject);
        }
    }
}