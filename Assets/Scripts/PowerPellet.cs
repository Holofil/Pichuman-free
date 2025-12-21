using UnityEngine;

public class PowerPellet : Pellet
{
    public float duration = 8f;
    [SerializeField] ParticleSystem PowerPelletEffect;

    protected override void Eat()
    {
        GameManager gameManager = FindObjectOfType<GameManager>();
        if (gameManager.isGraphicsActive)
        {
            var effect = Instantiate(PowerPelletEffect, transform.position, transform.rotation);
            Destroy(effect, 3f);
        }
        gameManager.PowerPelletEaten(this);
        Debug.Log($"pacman eating power pellet!!");
    }
}
