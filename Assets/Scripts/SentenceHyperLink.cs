using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;

public class SentenceHyperLink : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] TextMeshProUGUI LinkText;

    void Start()
    {
        string LinkProperties = "<color=#00B4FF><u>www.holofil.com</u></color>";
        string logo = "<u><link=https://www.holofil.com/holofil-cardboard>" + LinkProperties + "</link></u>";
        LinkText.text = "You can play the game using a normal touch interface where you just use up / down / left / right touch and slide controls on the screen. You can as well play remotely with the Bluetooth controller supplied by us or the exact same controller if you buy your own. You can play in the holographic mode by placing your mobile in HOLOFIL-cardboard device and choosing Holographic mode to play with the Bluetooth controller. You should have bought HOLOFIL-cardboard device from us for this mode. For more details about the Holographic mode please visit " + logo;
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
