using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;

public class WebHyperLink : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] TextMeshProUGUI LinkText;

    void Start()
    {
        string LinkProperties = "<color=#00B4FF><u>www.holofil.com</u></color>";
        LinkText.text = "<u><link=https://www.holofil.com/holofil-cardboard>" + LinkProperties + "</link></u>";
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            int index = TMP_TextUtilities.FindIntersectingLink(LinkText, Input.mousePosition, null);

            if (index > -1)
            {
                Application.OpenURL(LinkText.textInfo.linkInfo[index].GetLinkID());
            }
        }
    }
}
