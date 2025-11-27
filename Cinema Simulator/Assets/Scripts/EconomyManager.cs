using UnityEngine;
using TMPro;

public class EconomyManager : MonoBehaviour
{
    public static EconomyManager Instance;

    [Header("Configuración UI")]
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
}