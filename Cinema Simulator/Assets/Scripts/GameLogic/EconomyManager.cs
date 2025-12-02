using UnityEngine;
using TMPro;

public class EconomyManager : MonoBehaviour
{
    public static EconomyManager Instance;

    [Header("Configuraci�n UI")]
    public TextMeshProUGUI textoDineroTotal;
    public TextMeshProUGUI textoDeuda;

    [Header("Datos de Juego")]
    public int dineroActual = 0;
    public int deuda = 3000;

    [Header("Datos del Día (Reporte)")]
    public int ingresosHoy = 0;
    public int gastosHoy = 0;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        ActualizarTextoDinero();
        ActualizarTextoDeuda();
    }

    public void ResetearDatosDiarios()
    {
        ingresosHoy = 0;
        gastosHoy = 0;
    }

    public void SumarDinero(int cantidad)
    {
        dineroActual += cantidad;
        ingresosHoy += cantidad;
        ActualizarTextoDinero();
    }

    private void ActualizarTextoDinero()
    {
        if (textoDineroTotal != null)
        {
            // Muestra el dinero, ej: "$ 150"
            textoDineroTotal.text = "Balance:" + "$ " + dineroActual.ToString();
        }
    }

    private void ActualizarTextoDeuda()
    {
        if (textoDeuda != null)
        {
            // Muestra el dinero, ej: "$ 150"
            textoDeuda.text = "Deuda:" + "$ " + deuda;
        }
    }

    public bool GastarDinero(int cantidad)
    {
        // 1. Comprobamos si nos alcanza
        if (dineroActual >= cantidad)
        {
            dineroActual -= cantidad;
            gastosHoy += cantidad;
            ActualizarTextoDinero();
            return true; // ¡Compra exitosa!
        }
        else
        {
            Debug.Log("No tienes suficiente dinero.");
            return false; // ¡Compra fallida!
        }
    }

    public bool PagarDeuda(int cantidad)
    {
        if (dineroActual >= cantidad)
        {
            deuda -= cantidad;
            dineroActual -= cantidad;
            gastosHoy += cantidad;
            ActualizarTextoDinero();
            ActualizarTextoDeuda();
            return true;
        }
        else
        {
            Debug.Log("No tienes suficiente dinero.");
            return false;
        }
    }
}