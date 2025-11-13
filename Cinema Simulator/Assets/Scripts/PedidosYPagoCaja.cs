using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class RegisterUI : MonoBehaviour
{
    [Header("Entradas")]
    public Toggle entradaNormalToggle;
    public Toggle entradaReducidaToggle;

    [Header("Botones Snacks")]
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

    private List<string> comanda = new List<string>();
    private float total = 0f;

    void Start()
    {
        // Entradas
        entradaNormalToggle.onValueChanged.AddListener(delegate { SeleccionarEntrada(); });
        entradaReducidaToggle.onValueChanged.AddListener(delegate { SeleccionarEntrada(); });

        // Snacks
        palomitaSButton.onClick.AddListener(() => AgregarItem("Palomitas S", 3f));
        palomitaMButton.onClick.AddListener(() => AgregarItem("Palomitas M", 4f));
        palomitaLButton.onClick.AddListener(() => AgregarItem("Palomitas L", 5f));

        bebidaSButton.onClick.AddListener(() => AgregarItem("Bebida S", 2f));
        bebidaMButton.onClick.AddListener(() => AgregarItem("Bebida M", 3f));
        bebidaLButton.onClick.AddListener(() => AgregarItem("Bebida L", 4f));

        perritoButton.onClick.AddListener(() => AgregarItem("Perrito Caliente", 3.5f));

        // Botón Pagar
        pagarButton.onClick.AddListener(Pagar);

        ActualizarUI();
    }

    void SeleccionarEntrada()
    {
        if (entradaNormalToggle.isOn)
        {
            entradaReducidaToggle.isOn = false;
            AgregarItemUnico("Entrada Normal", 10f);
        }
        else if (entradaReducidaToggle.isOn)
        {
            entradaNormalToggle.isOn = false;
            AgregarItemUnico("Entrada Reducida", 7f);
        }
        else
        {
            EliminarItem("Entrada Normal");
            EliminarItem("Entrada Reducida");
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
        if (entradaNormalToggle.isOn) nuevoTotal += 10f;
        if (entradaReducidaToggle.isOn) nuevoTotal += 7f;

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
        comandaText.text = "🧾 COMANDA:\n";
        foreach (string item in comanda)
            comandaText.text += "- " + item + "\n";

        totalText.text = "TOTAL: $" + total.ToString("F2");
    }

    void Pagar()
    {
        Debug.Log("Pago realizado por $" + total);
        comanda.Clear();
        DesmarcarEntradas();
        ActualizarUI();
    }

    void DesmarcarEntradas()
    {
        entradaNormalToggle.isOn = false;
        entradaReducidaToggle.isOn = false;
    }
}
