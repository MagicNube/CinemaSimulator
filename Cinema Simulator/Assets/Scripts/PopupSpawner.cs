using UnityEngine;

public class PopupSpawner : MonoBehaviour
{
    [Header("Prefab del popup")]
    public GameObject popupPrefab;

    [Header("AnchorUI")]
    public Transform uiAnchor;

    private GameObject popupInstancia;

    void OnMouseDown()
    {
        AbrirPopup();
    }

    public void AbrirPopup()
    {
        if (popupInstancia != null) return;

        popupInstancia = Instantiate(popupPrefab);

        if (uiAnchor != null)
        {
            popupInstancia.transform.SetParent(uiAnchor, false);
            popupInstancia.transform.localPosition = Vector3.zero;
        }
        else
        {
            Canvas canvas = FindObjectOfType<Canvas>();
            popupInstancia.transform.SetParent(canvas.transform, false);
            popupInstancia.transform.localPosition = Vector3.zero;
        }
    }
}
