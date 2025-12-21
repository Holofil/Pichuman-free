using UnityEngine;

public class UpgradePopupManager : MonoBehaviour
{
    public GameObject popupPanel;
    private const string paidGameUrl = "https://play.google.com/store/apps/details?id=com.HOLOFIL.HolofilPachuman3DX";

    public void ShowPopup() => popupPanel.SetActive(true);
    public void ClosePopup() => popupPanel.SetActive(false);
    public void OpenPaidVersion() => Application.OpenURL(paidGameUrl);
}
