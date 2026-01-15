using UnityEngine;
using TMPro;

public class UIInteractionManager : MonoBehaviour
{
    public static UIInteractionManager Instance { get; private set; }

    [Header("Referencias UI")]
    public GameObject panelInteraccion;
    public TextMeshProUGUI textoInteraccion;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        OcultarInteraccion();
    }

    public void MostrarInteraccion(string mensaje)
    {
        if (panelInteraccion != null)
            panelInteraccion.SetActive(true);
        
        if (textoInteraccion != null)
            textoInteraccion.text = mensaje;
    }

    public void OcultarInteraccion()
    {
        if (panelInteraccion != null)
            panelInteraccion.SetActive(false);
    }
}
