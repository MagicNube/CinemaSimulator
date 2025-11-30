using UnityEngine;
using TMPro;

public class EconomyManager : MonoBehaviour
{
    public static EconomyManager Instance;

    [Header("Configuraci�n UI")]
    public TextMeshProUGUI textoDineroTotal;

    [Header("Datos de Juego")]
    public int dineroActual = 0;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        ActualizarTextoDinero();
    }

    public void SumarDinero(int cantidad)
    {
        dineroActual += cantidad;
        ActualizarTextoDinero();
    }

    private void ActualizarTextoDinero()
    {
        if (textoDineroTotal != null)
        {
            // Muestra el dinero, ej: "$ 150"
            textoDineroTotal.text = "$ " + dineroActual.ToString();
        }
    }

    public bool GastarDinero(int cantidad)
    {
        // 1. Comprobamos si nos alcanza
        if (dineroActual >= cantidad)
        {
            dineroActual -= cantidad;
            ActualizarTextoDinero();
            return true; // ¡Compra exitosa!
        }
        else
        {
            Debug.Log("No tienes suficiente dinero.");
            return false; // ¡Compra fallida!
        }
    }
}