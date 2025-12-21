using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PromotionScript : MonoBehaviour
{
    [SerializeField] string URL = "";

    AudioManager audioManager;

    private void Start()
    {
        audioManager = FindObjectOfType<AudioManager>();
    }

    public void ClosePromotionPanel()
    {
        audioManager.Play("back");
        this.gameObject.SetActive(false);
    }

    public void OpenPromoURL()
    {
        audioManager.Play("select");
        ClosePromotionPanel();
        Application.OpenURL(URL);
    }

}
