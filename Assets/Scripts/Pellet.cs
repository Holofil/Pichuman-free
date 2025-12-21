using UnityEngine;

public class Pellet : MonoBehaviour
{
    public int points = 10; // Score for each pellet

    protected virtual void OnTriggerEnter(Collider other)
    {
        Debug.Log("Trigger entered by object: " + other.gameObject.name + ", Tag: " + other.tag + ", Is Trigger: " + other.isTrigger);
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player detected! Eating pellet: " + gameObject.name);
            Eat();
        }
        else
        {
            Debug.Log("Not the Player, ignoring. Expected tag: Player, Got: " + other.tag);
        }
    }

    protected virtual void Eat()
    {
        Debug.Log("Eating pellet: " + gameObject.name + ", points: " + points);
        GameManager gameManager = FindObjectOfType<GameManager>();
        if (gameManager != null)
        {
            Debug.Log("GameManager found, calling PelletEaten...");
            gameManager.PelletEaten(this);
        }
        else
        {
            Debug.LogError("GameManager NOT found in scene!");
        }
        Destroy(gameObject);
    }
}
