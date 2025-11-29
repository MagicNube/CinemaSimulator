using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class PopupRegisterUI : MonoBehaviour
{
    [Header("Panel principal (dentro del prefab)")]
    public GameObject popupPanel;

    [Header("Entradas")]
    public Button entradaNormalButton;
    public Button entradaReducidaButton;

    [Header("Snacks")]
    public Button palomitaSButton;
    public Button palomitaMButton;
    public Button palomitaLButton;
    public Button bebidaSButton;
    public Button bebidaMButton;
    public Button bebidaLButton;
    public Button perritoButton;

    [Header("Comanda UI")]
    public TextMeshProUGUI comandaText;
    public TextMeshProUGUI totalText;
    public Button pagarButton;
    public Button cerrarButton;

    private List<string> comanda = new List<string>();
    private float total = 0f;
    private string entradaSeleccionada = "";

    void Start()
    {
        // activar el panel
        popupPanel.SetActive(true);

        // Entradas
        entradaNormalButton.onClick.AddListener(() => SeleccionarEntrada("Entrada Normal", 10f));
        entradaReducidaButton.onClick.AddListener(() => SeleccionarEntrada("Entrada Reducida", 7f));

        // Snacks
        palomitaSButton.onClick.AddListener(() => AgregarItem("Palomitas S", 3f));
        palomitaMButton.onClick.AddListener(() => AgregarItem("Palomitas M", 4f));
        palomitaLButton.onClick.AddListener(() => AgregarItem("Palomitas L", 5f));
        bebidaSButton.onClick.AddListener(() => AgregarItem("Bebida S", 2f));
        bebidaMButton.onClick.AddListener(() => AgregarItem("Bebida M", 3f));
        bebidaLButton.onClick.AddListener(() => AgregarItem("Bebida L", 4f));
        perritoButton.onClick.AddListener(() => AgregarItem("Perrito Caliente", 3.5f));

        pagarButton.onClick.AddListener(Pagar);
        cerrarButton.onClick.AddListener(CerrarPopup);

        ActualizarUI();
    }

    public void CerrarPopup()
    {
        Destroy(gameObject); // mata el prefab completo
    }

    void SeleccionarEntrada(string nombre, float precio)
    {
        if (entradaSeleccionada == nombre)
        {
            EliminarItem(nombre);
            entradaSeleccionada = "";
        }
        else
        {
            if (!string.IsNullOrEmpty(entradaSeleccionada))
                EliminarItem(entradaSeleccionada);

            AgregarItemUnico(nombre, precio);
            entradaSeleccionada = nombre;
        }
        ActualizarUI();
    }

    void AgregarItem(string nombre, float precio)
    {
        comanda.Add(nombre);
        total += precio;
        ActualizarUI();
    }

    void AgregarItemUnico(string nombre, float precio)
    {
        EliminarItem("Entrada Normal");
        EliminarItem("Entrada Reducida");
        comanda.Add(nombre);
        total = CalcularTotal();
    }

    void EliminarItem(string nombre)
    {
        if (comanda.Contains(nombre))
            comanda.Remove(nombre);
        total = CalcularTotal();
    }

    float CalcularTotal()
    {
        float nuevoTotal = 0f;

        if (entradaSeleccionada == "Entrada Normal") nuevoTotal += 10f;
        else if (entradaSeleccionada == "Entrada Reducida") nuevoTotal += 7f;

        foreach (string item in comanda)
        {
            if (item.Contains("Palomitas S")) nuevoTotal += 3f;
            else if (item.Contains("Palomitas M")) nuevoTotal += 4f;
            else if (item.Contains("Palomitas L")) nuevoTotal += 5f;
            else if (item.Contains("Bebida S")) nuevoTotal += 2f;
            else if (item.Contains("Bebida M")) nuevoTotal += 3f;
            else if (item.Contains("Bebida L")) nuevoTotal += 4f;
            else if (item.Contains("Perrito Caliente")) nuevoTotal += 3.5f;
        }

        return nuevoTotal;
    }

    void ActualizarUI()
    {
        comandaText.text = "COMANDA:\n";
        foreach (string item in comanda)
            comandaText.text += "- " + item + "\n";

        totalText.text = "TOTAL: $" + total.ToString("F2");
    }

    void Pagar()
    {
        Debug.Log("Pago realizado por $" + total);
        comanda.Clear();
        entradaSeleccionada = "";
        ActualizarUI();
        CerrarPopup();
    }
}
