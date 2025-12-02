using UnityEngine;
using TMPro;

public class HUDManager : MonoBehaviour
{
    // Singleton para llamarlo fácil desde cualquier sitio
    public static HUDManager Instance;

    [Header("Referencias UI")]
    public TextMeshProUGUI textoFechaGlobal; // El texto en la esquina de la pantalla

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // Al empezar, actualizamos la fecha para que no salga vacía
        ActualizarFecha();
    }

    public void ActualizarFecha()
    {
        if (textoFechaGlobal != null && GameManager.Instance != null)
        {
            int dia = GameManager.Instance.diaActual;

            // Formato: "DÍA 5 | SEMANA 3"
            textoFechaGlobal.text = $"Dia: {dia} - Semana: {(dia - 1) / 2 + 1}";
        }
    }
}