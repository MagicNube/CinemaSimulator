// MaquinaDeBebidas.cs
using UnityEngine;

using UnityEngine.UI;

// Asegúrate de que tiene los componentes necesarios
[RequireComponent(typeof(Outline))] // O como se llame tu script de outline
[RequireComponent(typeof(Collider))]
public class MaquinaDeBebidas : MonoBehaviour
{
    public ItemData.TipoDeItem tipoDeCajaRequerida;

    [SerializeField] private int capacidadMaxima = 10;
    [SerializeField] private int _capacidadActual = 0;

    [SerializeField] private Image barraRellenoImage;

    public int CapacidadActual
    {
        get { return _capacidadActual; }
        private set
        {
            _capacidadActual = Mathf.Clamp(value, 0, capacidadMaxima);
            ActualizarBarraVisual();
        }
    }

    private void Start()
    {
        ActualizarBarraVisual();
    }

    public void Interactuar(ControladorInteraccion jugador)
    {
        GameObject itemSujetado = jugador.itemActual;

        if (itemSujetado == null)
        {
            Debug.Log($"Estado: {CapacidadActual}/{capacidadMaxima}");
            return;
        }

        ItemData data = itemSujetado.GetComponent<ItemData>();

        if (data == null) return;

        if(data.tipoDeItem == tipoDeCajaRequerida)
        {
            if(Input.GetMouseButton(1))
            {
                CapacidadActual = capacidadMaxima;
                jugador.AsignarItem(null);
            }
            else
            {
                if (CapacidadActual < capacidadMaxima) CapacidadActual++;
            }
            return;
        }

        if (data.tipoDeItem == ItemData.TipoDeItem.VasoVacio)
        {
            if (CapacidadActual > 0)
            {
                if (data.prefabItemLleno != null)
                {
                    jugador.AsignarItem(data.prefabItemLleno);
                    CapacidadActual--;
                }
            }
            return;
        }
    }

    private void ActualizarBarraVisual()
    {
        if (barraRellenoImage != null)
        {
            float porcentaje = (float)CapacidadActual / (float)capacidadMaxima;
            barraRellenoImage.fillAmount = porcentaje;
        }
    }
}