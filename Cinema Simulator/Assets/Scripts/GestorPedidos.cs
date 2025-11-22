using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Linq;

public class GestorPedidos : MonoBehaviour
{
    [System.Serializable]
    public class ItemRequerido
    {
        public ItemData.TipoDeItem tipo;
        public int nivel;

        public ItemRequerido(ItemData.TipoDeItem t, int n)
        {
            tipo = t;
            nivel = n;
        }
    }

    [System.Serializable]
    public class PedidoRuntime
    {
        public List<ItemRequerido> itemsPendientes = new List<ItemRequerido>();
        public string textoDescripcion;
    }

    [Header("--- Referencias UI ---")]
    public GameObject contenedorBocadillo;
    public TextMeshProUGUI textoBocadillo;

    [Header("--- Referencias Monitor (Solo Lista) ---")]
    public GameObject panelMonitor;
    public Transform contenedorItemsMonitor;
    public GameObject prefabItemLista;

    private PedidoRuntime pedidoActual;

    private Coroutine rutinaNPC;
    private Coroutine rutinaFlujoPrincipal;

    void Start()
    {
        if (contenedorBocadillo != null) contenedorBocadillo.SetActive(false);
        if (panelMonitor != null) panelMonitor.SetActive(false);
    }

    public void GenerarNuevoPedido()
    {
        DetenerTodo();
        pedidoActual = new PedidoRuntime();

        pedidoActual.itemsPendientes.Add(new ItemRequerido(ItemData.TipoDeItem.Ticket, 1));

        int opcionAleatoria = Random.Range(0, 5);
        ItemRequerido extra = null;

        switch (opcionAleatoria)
        {
            case 0: extra = new ItemRequerido(ItemData.TipoDeItem.Bebida, 1); break;
            case 1: extra = new ItemRequerido(ItemData.TipoDeItem.Perrito, 1); break;
            case 2: extra = new ItemRequerido(ItemData.TipoDeItem.Palomitas, 1); break;
            case 3: extra = new ItemRequerido(ItemData.TipoDeItem.Palomitas, 2); break;
            case 4: extra = new ItemRequerido(ItemData.TipoDeItem.Palomitas, 3); break;
        }

        pedidoActual.itemsPendientes.Add(extra);

        string nombreExtra = FormatearNombre(extra);
        pedidoActual.textoDescripcion = "Hola, quiero una entrada y " + nombreExtra + ".";

        contenedorBocadillo.SetActive(true);
        panelMonitor.SetActive(true);

        ActualizarListaMonitorVisual();

        rutinaNPC = StartCoroutine(EscribirEnTexto(textoBocadillo, pedidoActual.textoDescripcion, 0.02f));
    }

    public bool RecibirItem(ItemData itemDelJugador)
    {
        if (pedidoActual == null || pedidoActual.itemsPendientes.Count == 0) return false;

        DetenerTodo();

        if (itemDelJugador == null)
        {
            rutinaFlujoPrincipal = StartCoroutine(GestionarMensajes("¡Eh! No traes nada.", 1.5f, false));
            return false;
        }

        ItemRequerido coincidencia = pedidoActual.itemsPendientes
            .FirstOrDefault(x => x.tipo == itemDelJugador.tipoDeItem && x.nivel == itemDelJugador.nivel);

        if (coincidencia != null)
        {
            // Primero marcamos visualmente
            MarcarItemComoEntregado(coincidencia);

            pedidoActual.itemsPendientes.Remove(coincidencia);

            if (pedidoActual.itemsPendientes.Count == 0)
            {
                rutinaFlujoPrincipal = StartCoroutine(GestionarMensajes("¡Perfecto, gracias!", 2f, true));
                return true;
            }
            else
            {
                string faltan = "";
                foreach (var item in pedidoActual.itemsPendientes) faltan += FormatearNombre(item) + " ";

                rutinaFlujoPrincipal = StartCoroutine(GestionarMensajes("Gracias. Aún me falta: " + faltan, 2f, false));
                return true;
            }
        }
        else
        {
            string errorMsg = "Eso no es lo que pedí... " + pedidoActual.textoDescripcion;
            rutinaFlujoPrincipal = StartCoroutine(GestionarMensajes(errorMsg, 2.5f, false));
            return false;
        }
    }

    private void ActualizarListaMonitorVisual()
    {
        for (int i = contenedorItemsMonitor.childCount - 1; i >= 0; i--)
            Destroy(contenedorItemsMonitor.GetChild(i).gameObject);

        foreach (ItemRequerido item in pedidoActual.itemsPendientes)
        {
            GameObject nuevoObj = Instantiate(prefabItemLista, contenedorItemsMonitor);
            nuevoObj.name = "Row_" + item.tipo.ToString() + "_" + item.nivel;

            TextMeshProUGUI txt = nuevoObj.GetComponentInChildren<TextMeshProUGUI>();
            if (txt)
            {
                txt.text = "<size=150%><color=red>X</color></size> " + FormatearNombre(item);
            }

            Toggle tgl = nuevoObj.GetComponentInChildren<Toggle>();
            if (tgl) tgl.isOn = false;
        }
    }

    private void MarcarItemComoEntregado(ItemRequerido item)
    {
        string nombreBuscado = "Row_" + item.tipo.ToString() + "_" + item.nivel;
        Transform fila = contenedorItemsMonitor.Find(nombreBuscado);

        if (fila != null)
        {
            Toggle tgl = fila.GetComponentInChildren<Toggle>();
            if (tgl) tgl.isOn = true;

            TextMeshProUGUI txt = fila.GetComponentInChildren<TextMeshProUGUI>();
            if (txt)
            {
                txt.text = "<size=150%><color=green><b>V</b></color></size> " + FormatearNombre(item);
            }
        }
    }

    private string FormatearNombre(ItemRequerido item)
    {
        if (item.tipo == ItemData.TipoDeItem.Ticket) return "Entrada";
        if (item.tipo == ItemData.TipoDeItem.Bebida) return "Bebida";
        if (item.tipo == ItemData.TipoDeItem.Perrito) return "Perrito";

        if (item.tipo == ItemData.TipoDeItem.Palomitas)
        {
            if (item.nivel == 1) return "Palomitas (S)";
            if (item.nivel == 2) return "Palomitas (M)";
            if (item.nivel == 3) return "Palomitas (L)";
        }

        return item.tipo.ToString();
    }

    IEnumerator EscribirEnTexto(TextMeshProUGUI targetText, string frase, float velocidad)
    {
        if (targetText == null) yield break;
        targetText.text = "";
        foreach (char letra in frase.ToCharArray())
        {
            targetText.text += letra;
            yield return new WaitForSeconds(velocidad);
        }
    }

    IEnumerator GestionarMensajes(string msgNPC, float duracion, bool pedidoFinalizado)
    {
        if (textoBocadillo != null) textoBocadillo.text = msgNPC;

        yield return new WaitForSeconds(duracion);

        if (pedidoFinalizado)
        {
            contenedorBocadillo.SetActive(false);
            panelMonitor.SetActive(false);
            pedidoActual = null;
            for (int i = contenedorItemsMonitor.childCount - 1; i >= 0; i--)
                Destroy(contenedorItemsMonitor.GetChild(i).gameObject);
        }
        else
        {
            if (pedidoActual != null)
            {
                string recordatorio = "Me falta: ";
                foreach (var item in pedidoActual.itemsPendientes) recordatorio += FormatearNombre(item) + " ";
                rutinaNPC = StartCoroutine(EscribirEnTexto(textoBocadillo, recordatorio, 0.02f));
            }
        }
    }

    private void DetenerTodo()
    {
        if (rutinaNPC != null) StopCoroutine(rutinaNPC);
        if (rutinaFlujoPrincipal != null) StopCoroutine(rutinaFlujoPrincipal);
    }
}