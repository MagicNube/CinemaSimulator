using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class TabletManager : MonoBehaviour
{
    [Header("Control General")]
    public GameObject panelTabletGeneral; // El padre de todo
    public GameObject pantallaHome;
    public GameObject pantallaTienda;
    public GameObject pantallaBanco;
    public GameObject pantallaCine;


    // Variables internas
    private ControladorInteraccion jugador;
    private bool interfazAbierta = false;

    // ==========================================
    // SECCIÓN 1: TIENDA (Lo que ya tenías)
    // ==========================================
    [System.Serializable]
    public struct ProductoTienda
    {
        public string nombre;
        public int precio;
        public GameObject prefabCaja;
        public Sprite icono;
    }
    [Header("APP TIENDA")]
    public List<ProductoTienda> catalogo;
    public Transform contenedorBotonesTienda;
    public Transform contenedorListaCarrito;
    public TextMeshProUGUI textoTotalPrecio;
    public Transform zonaDeEntrega;
    public GameObject botonProductoPrefab;
    public GameObject lineaCarritoPrefab;
    public int maxItemsCarrito = 10;

    private List<ProductoTienda> carrito = new List<ProductoTienda>();
    private int precioTotalCarrito = 0;

    // ==========================================
    // SECCIÓN 2: BANCO (Nueva)
    // ==========================================
    [Header("APP BANCO")]
    public int deudaTotal = 5000; // La deuda inicial
    public TextMeshProUGUI textoDeudaUI;
    public TextMeshProUGUI textoDineroDisponibleBanco; // Para ver cuánto tenemos al pagar

    // ==========================================
    // SECCIÓN 3: CINE (Nueva)
    // ==========================================
    [Header("APP CINE")]
    //public MeshRenderer pantallaDelCine; // La pantalla gigante 3D del cine
    public Material[] peliculasDisponibles; // Materiales con los posters/películas
    public Transform contenedorBotonesPeliculas;
    public GameObject botonPeliculaPrefab; // Un botón simple con imagen

    // ----------------------------------------------------------------------
    // FUNCIONES PRINCIPALES (Abrir/Cerrar/Navegar)
    // ----------------------------------------------------------------------

    void Start()
    {
        // Inicializar todo
        panelTabletGeneral.SetActive(false);
        GenerarTienda(); // Preparamos la tienda
        GenerarSelectorPeliculas(); // Preparamos el cine
        ActualizarUIBanco();
    }

    void Update()
    {
        if (interfazAbierta && Input.GetKeyDown(KeyCode.Escape)) CerrarTablet();
    }

    public void AbrirTablet(ControladorInteraccion _jugador)
    {
        jugador = _jugador;
        interfazAbierta = true;
        panelTabletGeneral.SetActive(true);

        // Al abrir, vamos siempre al HOME
        Debug.Log("AbrirTablet");
        IrAlHome();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        if (jugador != null) jugador.AlternarControlJugador(false);
    }

    public void CerrarTablet()
    {
        interfazAbierta = false;
        panelTabletGeneral.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        if (jugador != null) jugador.AlternarControlJugador(true);
    }

    // --- NAVEGACIÓN ---

    public void IrAlHome()
    {
        Debug.Log("Home");
        pantallaHome.SetActive(true);
        pantallaTienda.SetActive(false);
        pantallaBanco.SetActive(false);
        pantallaCine.SetActive(false);
    }

    public void AbrirAppTienda()
    {
        pantallaHome.SetActive(false);
        pantallaTienda.SetActive(true);
    }

    public void AbrirAppBanco()
    {
        pantallaHome.SetActive(false);
        pantallaBanco.SetActive(true);
        ActualizarUIBanco(); // Refrescamos los datos al entrar
    }

    public void AbrirAppCine()
    {
        pantallaHome.SetActive(false);
        pantallaCine.SetActive(true);
    }

    // ----------------------------------------------------------------------
    // LÓGICA DE LA TIENDA (Simplificada de tu anterior script)
    // ----------------------------------------------------------------------
    void GenerarTienda()
    {
        foreach (Transform child in contenedorBotonesTienda) Destroy(child.gameObject);

        for (int i = 0; i < catalogo.Count; i++)
        {
            int idx = i;
            GameObject btn = Instantiate(botonProductoPrefab, contenedorBotonesTienda);

            // Texto
            TextMeshProUGUI txt = btn.GetComponentInChildren<TextMeshProUGUI>();
            if (txt) txt.text = $"{catalogo[i].nombre}\n${catalogo[i].precio}";

            // Icono
            Transform icono = btn.transform.Find("Icono");
            if (icono && catalogo[i].icono) icono.GetComponent<Image>().sprite = catalogo[i].icono;

            // Click
            btn.GetComponent<Button>().onClick.AddListener(() => AgregarAlCarrito(idx));
        }
    }

    void AgregarAlCarrito(int index)
    {
        if (carrito.Count >= maxItemsCarrito) return;

        carrito.Add(catalogo[index]);
        precioTotalCarrito += catalogo[index].precio;
        ActualizarCarritoVisual();
    }

    void ActualizarCarritoVisual()
    {
        foreach (Transform child in contenedorListaCarrito) Destroy(child.gameObject);
        foreach (var item in carrito)
        {
            GameObject linea = Instantiate(lineaCarritoPrefab, contenedorListaCarrito);
            linea.transform.localScale = Vector3.one; // Bug fix escala
            linea.GetComponent<TextMeshProUGUI>().text = item.nombre;
        }
        textoTotalPrecio.text = $"Total: ${precioTotalCarrito}";
    }

    public void ComprarCarrito()
    {
        if (carrito.Count == 0) return;

        if (EconomyManager.Instance.GastarDinero(precioTotalCarrito))
        {
            float altura = 0;
            foreach (var item in carrito)
            {
                Instantiate(item.prefabCaja, zonaDeEntrega.position + Vector3.up * altura, Quaternion.identity);
                altura += 0.5f;
            }
            CancelarCarrito();
            // Opcional: CerrarTablet();
        }
    }

    public void CancelarCarrito()
    {
        carrito.Clear();
        precioTotalCarrito = 0;
        ActualizarCarritoVisual();
    }

    // ----------------------------------------------------------------------
    // LÓGICA DEL BANCO (Pagar Deuda)
    // ----------------------------------------------------------------------
    void ActualizarUIBanco()
    {
        if (textoDeudaUI) textoDeudaUI.text = $"Deuda Restante:\n<color=red>${deudaTotal}</color>";
        if (textoDineroDisponibleBanco) textoDineroDisponibleBanco.text = $"Tu Dinero: ${EconomyManager.Instance.dineroActual}";
    }

    public void PagarDeuda(int cantidad)
    {
        if (deudaTotal <= 0)
        {
            Debug.Log("¡Ya eres libre de deudas!");
            return;
        }

        // Verificamos si tenemos dinero para pagar esa cantidad
        if (EconomyManager.Instance.GastarDinero(cantidad))
        {
            deudaTotal -= cantidad;
            if (deudaTotal < 0) deudaTotal = 0; // No tener deuda negativa

            ActualizarUIBanco();
            Debug.Log($"Has pagado ${cantidad}. Deuda restante: {deudaTotal}");
        }
    }

    // ----------------------------------------------------------------------
    // LÓGICA DEL CINE (Cambiar Película)
    // ----------------------------------------------------------------------
    void GenerarSelectorPeliculas()
    {
        foreach (Transform child in contenedorBotonesPeliculas) Destroy(child.gameObject);

        for (int i = 0; i < peliculasDisponibles.Length; i++)
        {
            int idx = i;
            GameObject btn = Instantiate(botonPeliculaPrefab, contenedorBotonesPeliculas);

            // Asignamos la imagen del botón (asumiendo que el material tiene una textura principal)
            if (peliculasDisponibles[i].mainTexture != null)
            {
                // Convertir textura a sprite al vuelo (truco rápido)
                Texture2D tex = (Texture2D)peliculasDisponibles[i].mainTexture;
                Sprite cartel = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
                btn.GetComponent<Image>().sprite = cartel;
            }

            // Al hacer click, cambiamos el material de la pantalla gigante
            //btn.GetComponent<Button>().onClick.AddListener(() => CambiarPelicula(idx));
        }
    }

    // void CambiarPelicula(int index)
    // {
    //     if (pantallaDelCine != null && peliculasDisponibles.Length > index)
    //     {
    //         pantallaDelCine.material = peliculasDisponibles[index];
    //         Debug.Log("Película cambiada a: " + peliculasDisponibles[index].name);
    //     }
    // }
}