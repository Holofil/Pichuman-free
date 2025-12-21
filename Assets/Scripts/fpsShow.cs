using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class fpsShow : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI fpsText;

    private void Update()
    {
        fpsText.text = Mathf.RoundToInt(1 / Time.deltaTime).ToString();
    }

}
