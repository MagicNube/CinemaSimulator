using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class TabletPedidos : MonoBehaviour
{
    [System.Serializable]
    public struct ProductoTienda
    {
        public string nombreProducto;
        public int precio;
        public GameObject prefabCaja;
        public Sprite icono;
    }

    [Header("Configuración Tienda")]
    public int maxItemsCarrito = 10; // LÍMITE DE ITEMS
    public List<ProductoTienda> catalogo;
    public Transform zonaDeEntrega;

    [Header("Interfaz UI")]
    public GameObject panelTabletUI;
    public Transform contenedorBotonesProductos;
    public Transform contenedorListaCarrito;
    public TextMeshProUGUI textoTotalPrecio;

    [Header("Prefabs UI")]
    public GameObject botonProductoPrefab;
    public GameObject lineaCarritoPrefab;

    // Estado interno
    private List<ProductoTienda> carrito = new List<ProductoTienda>();
    private int precioTotal = 0;
    private bool interfazAbierta = false;
    private ControladorInteraccion jugador;

    void Start()
    {
        GenerarBotonesTienda();
        if (panelTabletUI != null) panelTabletUI.SetActive(false);
    }

    void Update()
    {
        if (interfazAbierta && Input.GetKeyDown(KeyCode.Escape)) CerrarTablet();
    }

    // ... (AbrirTablet y CerrarTablet se quedan igual) ...
    public void AbrirTablet(ControladorInteraccion _jugador)
    {
        jugador = _jugador;
        interfazAbierta = true;
        panelTabletUI.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        if (jugador != null) jugador.AlternarControlJugador(false);
    }

    public void CerrarTablet()
    {
        interfazAbierta = false;
        panelTabletUI.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        if (jugador != null) jugador.AlternarControlJugador(true);
    }

    void GenerarBotonesTienda()
    {
        // Borrar botones antiguos
        foreach (Transform child in contenedorBotonesProductos) Destroy(child.gameObject);

        for (int i = 0; i < catalogo.Count; i++)
        {
            int indice = i;
            GameObject btn = Instantiate(botonProductoPrefab, contenedorBotonesProductos);

            // 1. Poner Texto
            TextMeshProUGUI textoBtn = btn.GetComponentInChildren<TextMeshProUGUI>();
            if (textoBtn != null) textoBtn.text = $"{catalogo[i].nombreProducto}\n${catalogo[i].precio}";

            // --- 2. NUEVO: PONER EL ICONO ---
            // Buscamos un objeto hijo llamado "Icono" dentro del botón
            Transform hijoIcono = btn.transform.Find("Icono");

            if (hijoIcono != null && catalogo[i].icono != null)
            {
                // Si existe el hueco y tenemos icono, se lo asignamos
                hijoIcono.GetComponent<Image>().sprite = catalogo[i].icono;
            }
            // --------------------------------

            // 3. Añadir evento Click
            btn.GetComponent<Button>().onClick.AddListener(() => AgregarAlCarrito(indice));
        }
    }

    // --- AQUÍ ESTÁ EL LÍMITE DE 10 ITEMS ---
    void AgregarAlCarrito(int indice)
    {
        // 1. Comprobamos si ya hemos llegado al límite
        if (carrito.Count >= maxItemsCarrito)
        {
            Debug.Log("¡El carrito está lleno! No caben más cajas.");
            // Opcional: Hacer parpadear el texto de total en rojo
            return; // Salimos de la función sin añadir nada
        }

        ProductoTienda producto = catalogo[indice];
        carrito.Add(producto);
        precioTotal += producto.precio;

        ActualizarVistaCarrito();
    }

    // --- FUNCIÓN PARA BORRAR VISUALMENTE ---
    void ActualizarVistaCarrito()
    {
        // 1. ELIMINAR LOS TEXTOS VIEJOS (Limpieza visual)
        // Recorremos todos los hijos del contenedor y los destruimos
        foreach (Transform child in contenedorListaCarrito)
        {
            Destroy(child.gameObject);
        }

        // 2. CREAR LOS TEXTOS NUEVOS
        foreach (var item in carrito)
        {
            GameObject linea = Instantiate(lineaCarritoPrefab, contenedorListaCarrito);
            // Aseguramos que la escala sea 1,1,1 por si el layout hace cosas raras
            linea.transform.localScale = Vector3.one;
            linea.GetComponent<TextMeshProUGUI>().text = $"- {item.nombreProducto} (${item.precio})";
        }

        // 3. ACTUALIZAR TEXTO TOTAL (Añadimos el contador de items)
        textoTotalPrecio.text = $"TOTAL: ${precioTotal}  ({carrito.Count}/{maxItemsCarrito})";
    }

    public void RealizarPedido()
    {
        if (carrito.Count == 0) return;

        // --- AQUÍ CONECTAMOS CON TU ECONOMY MANAGER ---

        // Intentamos gastar el dinero. Si devuelve TRUE, procedemos.
        if (EconomyManager.Instance.GastarDinero(precioTotal))
        {
            // SI EL PAGO FUE ACEPTADO:
            Debug.Log($"Pedido realizado. Coste: -${precioTotal}");

            float offsetAltura = 0;
            foreach (var item in carrito)
            {
                Vector3 pos = zonaDeEntrega.position + Vector3.up * offsetAltura;
                Instantiate(item.prefabCaja, pos, zonaDeEntrega.rotation);
                offsetAltura += 0.5f;
            }

            // Vaciamos el carrito y cerramos
            CancelarPedido();
            CerrarTablet();
        }
        else
        {
            // SI NO TIENE DINERO (EconomyManager devolvió false):
            // Aquí podrías hacer que el texto del precio parpadee en rojo
            StartCoroutine(AnimacionSinDinero());
        }
    }

    // Un pequeño feedback visual opcional para cuando no tienes dinero
    System.Collections.IEnumerator AnimacionSinDinero()
    {
        Color colorOriginal = textoTotalPrecio.color;
        textoTotalPrecio.color = Color.red; // Se pone rojo
        textoTotalPrecio.text = "¡SIN FONDOS!";

        yield return new WaitForSeconds(1f);

        textoTotalPrecio.color = colorOriginal; // Vuelve al color normal
        ActualizarVistaCarrito(); // Restaura el texto del precio
    }

    // --- CORRECCIÓN DEL BOTÓN CANCELAR ---
    public void CancelarPedido()
    {
        // 1. Borrar datos lógicos
        carrito.Clear();
        precioTotal = 0;

        // 2. IMPORTANTE: Borrar lo visual llamando a la función de actualizar
        // Como la lista 'carrito' ahora está vacía (Count = 0), esta función borrará todo el texto
        ActualizarVistaCarrito();
    }
}