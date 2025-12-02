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
        public ItemRequerido(ItemData.TipoDeItem t, int n) { tipo = t; nivel = n; }
    }

    [System.Serializable]
    public class PedidoRuntime
    {
        public List<ItemRequerido> itemsPendientes = new List<ItemRequerido>();
        public string textoDescripcion;
    }

    [Header("UI Referencias")]
    public GameObject contenedorBocadillo;
    public TextMeshProUGUI textoBocadillo;
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
        // Doble verificación: solo pido si el QueueManager dice que es mi turno
        QueueManager qm = FindObjectOfType<QueueManager>();
        if (qm != null && qm.GetCustomerAtOrderPoint() != this.gameObject) return;

        DetenerTodo();
        pedidoActual = new PedidoRuntime();

        // Lógica del pedido
        pedidoActual.itemsPendientes.Add(new ItemRequerido(ItemData.TipoDeItem.Ticket, 1));

        int r = Random.Range(0, 5);
        ItemRequerido extra = null;
        switch (r) {
            case 0: extra = new ItemRequerido(ItemData.TipoDeItem.Bebida, 1); break;
            case 1: extra = new ItemRequerido(ItemData.TipoDeItem.Perrito, 1); break;
            case 2: extra = new ItemRequerido(ItemData.TipoDeItem.Palomitas, 1); break;
            case 3: extra = new ItemRequerido(ItemData.TipoDeItem.Palomitas, 2); break;
            case 4: extra = new ItemRequerido(ItemData.TipoDeItem.Palomitas, 3); break;
        }
        pedidoActual.itemsPendientes.Add(extra);

        string nombreExtra = FormatearNombre(extra);
        pedidoActual.textoDescripcion = "Hola, quiero una entrada y " + nombreExtra + ".";

        if (contenedorBocadillo) contenedorBocadillo.SetActive(true);
        if (panelMonitor) panelMonitor.SetActive(true);

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

        var coincidencia = pedidoActual.itemsPendientes
            .FirstOrDefault(x => x.tipo == itemDelJugador.tipoDeItem && x.nivel == itemDelJugador.nivel);

        if (coincidencia != null)
        {
            MarcarItemComoEntregado(coincidencia);

            // ASUMO QUE TIENES ECONOMYMANAGER
            if (EconomyManager.Instance != null)
            {
                EconomyManager.Instance.SumarDinero(itemDelJugador.precio);
            }

            pedidoActual.itemsPendientes.Remove(coincidencia);

            if (pedidoActual.itemsPendientes.Count == 0)
            {
                // PEDIDO COMPLETADO -> ÉXITO
                rutinaFlujoPrincipal = StartCoroutine(GestionarMensajes("¡Perfecto, gracias!", 2f, true));
                return true;
            }
            else
            {
                string faltan = "";
                foreach (var it in pedidoActual.itemsPendientes) faltan += FormatearNombre(it) + " ";
                rutinaFlujoPrincipal = StartCoroutine(GestionarMensajes("Gracias. Me falta: " + faltan, 2f, false));
                return true;
            }
        }
        else
        {
            rutinaFlujoPrincipal = StartCoroutine(GestionarMensajes("Eso no es lo que pedí...", 2.5f, false));
            return false;
        }
    }

    public void DetenerTodo()
    {
        if (rutinaNPC != null) StopCoroutine(rutinaNPC);
        if (rutinaFlujoPrincipal != null) StopCoroutine(rutinaFlujoPrincipal);
    }

    IEnumerator GestionarMensajes(string msg, float duracion, bool pedidoFinalizado)
    {
        if (textoBocadillo != null) textoBocadillo.text = msg;
        yield return new WaitForSeconds(duracion);

        if (pedidoFinalizado)
        {
            // Limpieza visual
            if (contenedorBocadillo) contenedorBocadillo.SetActive(false);
            if (panelMonitor) panelMonitor.SetActive(false);
            pedidoActual = null;
            if (contenedorItemsMonitor) {
                foreach (Transform child in contenedorItemsMonitor) Destroy(child.gameObject);
            }

            // COMUNICAR AL CLIENTE QUE SE VAYA (EXITO)
            PedidoCliente pc = GetComponent<PedidoCliente>();
            if (pc != null) pc.OrderFinished(true);
        }
        else
        {
            // Recordatorio de lo que falta
            if (pedidoActual != null) {
                string recordatorio = "Me falta: ";
                foreach (var it in pedidoActual.itemsPendientes) recordatorio += FormatearNombre(it) + " ";
                rutinaNPC = StartCoroutine(EscribirEnTexto(textoBocadillo, recordatorio, 0.02f));
            }
        }
    }

    // --- MÉTODOS VISUALES AUXILIARES ---

    private void ActualizarListaMonitorVisual()
    {
        if (!contenedorItemsMonitor || !prefabItemLista) return;
        foreach (Transform child in contenedorItemsMonitor) Destroy(child.gameObject);

        foreach (var item in pedidoActual.itemsPendientes)
        {
            GameObject obj = Instantiate(prefabItemLista, contenedorItemsMonitor);
            obj.name = "Row_" + item.tipo + "_" + item.nivel;
            TextMeshProUGUI txt = obj.GetComponentInChildren<TextMeshProUGUI>();
            if (txt) txt.text = "<color=red>X</color> " + FormatearNombre(item);
            Toggle tgl = obj.GetComponentInChildren<Toggle>();
            if (tgl) tgl.isOn = false;
        }
    }

    private void MarcarItemComoEntregado(ItemRequerido item)
    {
        if (!contenedorItemsMonitor) return;
        Transform fila = contenedorItemsMonitor.Find("Row_" + item.tipo + "_" + item.nivel);
        if (fila)
        {
            Toggle tgl = fila.GetComponentInChildren<Toggle>();
            if (tgl) tgl.isOn = true;
            TextMeshProUGUI txt = fila.GetComponentInChildren<TextMeshProUGUI>();
            if (txt) txt.text = "<color=green>V</color> " + FormatearNombre(item);
        }
    }

    private string FormatearNombre(ItemRequerido item)
    {
        // Ajusta los nombres según tus necesidades
        if (item.tipo.ToString().Contains("Caja")) return item.tipo.ToString().Replace("Caja", "Caja ");
        return item.tipo.ToString();
    }

    IEnumerator EscribirEnTexto(TextMeshProUGUI target, string frase, float vel)
    {
        if (!target) yield break;
        target.text = "";
        foreach (char c in frase)
        {
            target.text += c;
            yield return new WaitForSeconds(vel);
        }
    }
}
