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
    private Outline customerOutline;

    void Awake()
    {
        customerOutline = GetComponent<Outline>();
        if (customerOutline != null) customerOutline.enabled = false;
        // 1. UI INTERNA (Bocadillo)
        if (contenedorBocadillo == null)
        {
            Transform bocadilloTrans = transform.Find("BocadilloCanvas");
            if (bocadilloTrans != null)
            {
                contenedorBocadillo = bocadilloTrans.gameObject;
                textoBocadillo = contenedorBocadillo.GetComponentInChildren<TextMeshProUGUI>();
            }
        }
        if (contenedorBocadillo != null) contenedorBocadillo.SetActive(false);

        // 2. UI EXTERNA (Monitor)
        QueueManager qm = FindObjectOfType<QueueManager>();

        if (qm != null)
        {
            panelMonitor = qm.REF_PanelMonitor;
            contenedorItemsMonitor = qm.REF_ContenedorItems;
        }
        else
        {
            Debug.LogError("ERROR: No encuentro el script QueueManager en la escena.");
        }
    }

    public void GenerarNuevoPedido()
    {
        // Verificar turno
        QueueManager qm = FindObjectOfType<QueueManager>();

        // COMENTA ESTA LÍNEA SI TE DABA PROBLEMAS CON LA CAMPANA
        // if (qm != null && qm.GetCustomerAtOrderPoint() != this.gameObject) return;

        DetenerTodo();
        pedidoActual = new PedidoRuntime();

        if (contenedorBocadillo != null) contenedorBocadillo.SetActive(true);
        if (panelMonitor != null) panelMonitor.SetActive(true);
        if (customerOutline != null) customerOutline.enabled = true;

        // ---------------------------------------------------------
        // 1. SIEMPRE AÑADIR LA ENTRADA (OBLIGATORIO)
        // ---------------------------------------------------------
        pedidoActual.itemsPendientes.Add(new ItemRequerido(ItemData.TipoDeItem.Ticket, 1));

        // ---------------------------------------------------------
        // 2. AÑADIR EXTRAS ALEATORIOS (SIN REPETIDOS)
        // ---------------------------------------------------------
        int cantidadExtras = Random.Range(1, 4); // Generará 1, 2 o 3 items extra
        int extrasAnadidos = 0;
        int intentosSeguridad = 0; // Para evitar bucles infinitos (por si acaso)

        while (extrasAnadidos < cantidadExtras && intentosSeguridad < 50)
        {
            intentosSeguridad++;
            ItemRequerido candidato = GenerarItemAleatorio();

            // LÓGICA DE UNICIDAD:
            // Buscamos en la lista si ya existe algún item con el MISMO tipo y el MISMO nivel
            bool yaExiste = pedidoActual.itemsPendientes.Any(x => x.tipo == candidato.tipo && x.nivel == candidato.nivel);

            if (!yaExiste)
            {
                pedidoActual.itemsPendientes.Add(candidato);
                extrasAnadidos++;
            }
            // Si ya existe, no hacemos nada y el bucle while vuelve a intentarlo
        }

        // ---------------------------------------------------------
        // 3. CONSTRUIR TEXTO DE DESCRIPCIÓN
        // ---------------------------------------------------------
        string descripcion = "Hola, ponme: ";
        for (int i = 0; i < pedidoActual.itemsPendientes.Count; i++)
        {
            descripcion += FormatearNombre(pedidoActual.itemsPendientes[i]);

            // Si no es el último, añade coma, si es el último pone punto.
            if (i < pedidoActual.itemsPendientes.Count - 1) descripcion += ", ";
            else descripcion += ".";
        }
        pedidoActual.textoDescripcion = descripcion;

        Debug.Log($"[PEDIDO GENERADO] Cliente: {gameObject.name} | Items: {pedidoActual.itemsPendientes.Count}");

        // ---------------------------------------------------------
        // 4. MOSTRAR UI
        // ---------------------------------------------------------
        contenedorBocadillo.SetActive(true);
        if (panelMonitor != null) panelMonitor.SetActive(true);

        ActualizarListaMonitorVisual();

        rutinaNPC = StartCoroutine(EscribirEnTexto(textoBocadillo, pedidoActual.textoDescripcion, 0.02f));
    }

    // Método auxiliar para elegir un item al azar
    private ItemRequerido GenerarItemAleatorio()
    {
        int opcion = Random.Range(0, 5);
        switch (opcion)
        {
            case 0: return new ItemRequerido(ItemData.TipoDeItem.Bebida, 1);
            case 1: return new ItemRequerido(ItemData.TipoDeItem.Perrito, 1);
            case 2: return new ItemRequerido(ItemData.TipoDeItem.Palomitas, 1); // Pequeñas
            case 3: return new ItemRequerido(ItemData.TipoDeItem.Palomitas, 2); // Medianas
            case 4: return new ItemRequerido(ItemData.TipoDeItem.Palomitas, 3); // Grandes
            default: return new ItemRequerido(ItemData.TipoDeItem.Bebida, 1);
        }
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
            Debug.Log("ME ESTAN DANDO UN ITEM");
            // Primero marcamos visualmente
            MarcarItemComoEntregado(coincidencia);

            if (EconomyManager.Instance != null)
            {
                EconomyManager.Instance.SumarDinero(itemDelJugador.precio);
            }

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

    // Función pública para llamar desde fuera cuando se acaba el tiempo
    public void ForzarCierrePedido()
    {
        DetenerTodo(); // Parar de escribir texto

        // 1. APAGAR UI Y EFECTOS
        if (contenedorBocadillo != null) contenedorBocadillo.SetActive(false);
        if (panelMonitor != null) panelMonitor.SetActive(false);
        if (customerOutline != null) customerOutline.enabled = false;

        // 2. LIMPIAR DATOS
        pedidoActual = null;

        // 3. LIMPIAR MONITOR
        if (contenedorItemsMonitor != null)
        {
            for (int i = contenedorItemsMonitor.childCount - 1; i >= 0; i--)
            {
                Destroy(contenedorItemsMonitor.GetChild(i).gameObject);
            }
        }

        Debug.Log("Pedido cancelado por tiempo o error. UI limpiada.");
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
            if (customerOutline != null) customerOutline.enabled = false;
            pedidoActual = null;
            for (int i = contenedorItemsMonitor.childCount - 1; i >= 0; i--) { Destroy(contenedorItemsMonitor.GetChild(i).gameObject); }

            PedidoCliente pc = GetComponent<PedidoCliente>();
            if (pc != null) {
                pc.OrderFinished(true);
                Debug.Log("Order Finished");
            }
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

    public void DetenerTodo()
    {
        if (rutinaNPC != null) StopCoroutine(rutinaNPC);
        if (rutinaFlujoPrincipal != null) StopCoroutine(rutinaFlujoPrincipal);
    }
}
