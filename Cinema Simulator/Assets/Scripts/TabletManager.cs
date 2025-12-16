using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;


public class TabletManager : MonoBehaviour
{
    [Header("Control General")]
    public GameObject panelTabletGeneral;
    public GameObject pantallaHome;
    public GameObject pantallaTienda;
    public GameObject pantallaBanco;
    public GameObject pantallaCine;

    // Variables internas
    private ControladorInteraccion jugador;
    public bool interfazAbierta = false;

    // ==========================================
    // SECCIÓN 1: TIENDA
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
    // SECCIÓN 2: BANCO
    // ==========================================
    [Header("APP BANCO")]
    public TextMeshProUGUI textoDeudaUI;
    public TextMeshProUGUI textoDineroDisponibleBanco;
    public TMP_InputField inputCantidadPago;

    // ==========================================
    // SECCIÓN 3: CINE (SISTEMA COMPLETO)
    // ==========================================
    [Header("APP CINE - CONFIGURACIÓN")]
    public List<NewsScenario> tablaDeNoticias; // Arrastra aquí tus noticias (Noticia + 3 Géneros)
    public List<MovieAsset> peliculasDisponibles; // Arrastra aquí tus pelis (Titulo + Poster + Material + Género)

    [Header("APP CINE - UI")]
    public TextMeshProUGUI textoNoticiaDia; // El texto grande de la noticia
    public Button[] botonesEleccion; // Tienen que ser 3 botones fijos en la UI
    public Image[] postersEleccion; // Las 3 imagenes (Image) dentro de esos botones

    public Image[] postersDecoracion;

    // Estado del Cine
    [HideInInspector] public float multiplicadorClientes = 1.0f; // 1.5 = Bueno, 0.6 = Malo
    private NewsScenario noticiaActual;
    private MovieAsset[] peliculasOpcion = new MovieAsset[3];
    private CinemaGenre[] generosOpcion = new CinemaGenre[3];

    [Header("UI HOME")]
    public TextMeshProUGUI textoAvisoHome;

    // ----------------------------------------------------------------------
    // FUNCIONES PRINCIPALES
    // ----------------------------------------------------------------------

    void Start()
    {
        // Inicializar todo
        panelTabletGeneral.SetActive(false);
        if (postersDecoracion != null)
        {
            foreach (Image poster in postersDecoracion) poster.gameObject.SetActive(false);
        }
        GenerarTienda();
        ActualizarUIBanco();

        // Iniciamos el día 1 del cine
        NuevoDiaCine();
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
        pantallaHome.SetActive(true);
        pantallaTienda.SetActive(false);
        pantallaBanco.SetActive(false);
        pantallaCine.SetActive(false);

        if (textoAvisoHome != null) textoAvisoHome.text = "";
    }
    public void AbrirAppTienda()
    {
        if (GameManager.Instance.faseActual != FaseJuego.Fase3_Cierre)
        {
            StartCoroutine(AnimacionAvisoHome("¡Solo disponible al CIERRE!"));
            return;
        }

        pantallaHome.SetActive(false);
        pantallaTienda.SetActive(true);

        pantallaHome.SetActive(false);
        pantallaTienda.SetActive(true);
    }
    public void AbrirAppBanco() { pantallaHome.SetActive(false); pantallaBanco.SetActive(true); ActualizarUIBanco(); }
    public void AbrirAppCine() { pantallaHome.SetActive(false); pantallaCine.SetActive(true); }

    // ----------------------------------------------------------------------
    // LÓGICA DE LA TIENDA
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
            Button btnComp = btn.GetComponent<Button>();
            btnComp.onClick.RemoveAllListeners();
            btnComp.onClick.AddListener(() => AgregarAlCarrito(idx));
        }
    }

    void AgregarAlCarrito(int index)
    {
        if (carrito.Count >= maxItemsCarrito) { StartCoroutine(AnimacionAvisoCarrito("¡Carrito lleno!", Color.red)); return; }

        carrito.Add(catalogo[index]);
        precioTotalCarrito += catalogo[index].precio;
        ActualizarCarritoVisual();
    }

    void ActualizarCarritoVisual()
    {
        foreach (Transform child in contenedorListaCarrito) Destroy(child.gameObject);

        int numeroDeItem = 1;

        foreach (var item in carrito)
        {
            GameObject linea = Instantiate(lineaCarritoPrefab, contenedorListaCarrito);
            linea.transform.localScale = Vector3.one;
            string text = $"{numeroDeItem}. {item.nombre} ({item.precio}$)";
            linea.GetComponent<TextMeshProUGUI>().SetText(text);
            numeroDeItem++;
        }
        textoTotalPrecio.text = $"Total: ${precioTotalCarrito} ({carrito.Count}/{maxItemsCarrito})";
        textoTotalPrecio.color = Color.black; // O Color.white si el fondo es oscuro
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
        }
        else
        {
            StartCoroutine(AnimacionAvisoCarrito("¡Sin fondos!", Color.red));
        }
    }

    public void CancelarCarrito()
    {
        carrito.Clear();
        precioTotalCarrito = 0;
        ActualizarCarritoVisual();
    }

    // ----------------------------------------------------------------------
    // LÓGICA DEL BANCO
    // ----------------------------------------------------------------------
    void ActualizarUIBanco()
    {
        if (textoDeudaUI) textoDeudaUI.text = $"Deuda Restante:\n<color=red>${EconomyManager.Instance.deuda}</color>";
        if (textoDineroDisponibleBanco) textoDineroDisponibleBanco.text = $"Tu Dinero: ${EconomyManager.Instance.dineroActual}";
    }

    public void PagarDeuda(int cantidad)
    {
        if (EconomyManager.Instance.deuda <= 0)
        {
            Debug.Log("¡Ya eres libre de deudas!");
            StartCoroutine(AnimacionAvisoBanco("¡LIBRE DE DEUDAS!", Color.green));
            return;
        }

        if (EconomyManager.Instance.PagarDeuda(cantidad))
        {
            ActualizarUIBanco();
        }
        else
        {
            StartCoroutine(AnimacionAvisoBanco("¡FONDOS INSUFICIENTES!", Color.red));
        }
    }

    public void PagarDeudaTodo()
    {
        if (EconomyManager.Instance.deuda <= 0) return;

        // LÓGICA MEJORADA: Pagamos el menor valor entre (lo que tengo) y (lo que debo)
        // Evita pagar 5000 si solo debes 100.
        int cantidadAPagar = Mathf.Min(EconomyManager.Instance.dineroActual, EconomyManager.Instance.deuda);

        if (cantidadAPagar > 0)
        {
            PagarDeuda(cantidadAPagar);
        }
        else
        {
            StartCoroutine(AnimacionAvisoBanco("¡FONDOS INSUFICIENTES!", Color.red));
        }
    }

    public void PagarCantidadPersonalizada()
    {
        if (string.IsNullOrEmpty(inputCantidadPago.text)) return;

        if (int.TryParse(inputCantidadPago.text, out int cantidadA_Pagar))
        {
            if (cantidadA_Pagar > 0)
            {
                PagarDeuda(cantidadA_Pagar);
                inputCantidadPago.text = "";
            }
        }
        else
        {
            inputCantidadPago.text = "";
        }
    }

    // ----------------------------------------------------------------------
    // LÓGICA DEL CINE (NUEVA: NOTICIAS Y GÉNEROS)
    // ----------------------------------------------------------------------

    // Llamar a esto al iniciar el juego o dormir
    [ContextMenu("Forzar Nuevo Día")]
    public void NuevoDiaCine()
    {
        if (tablaDeNoticias.Count == 0 || peliculasDisponibles.Count == 0) return;
        if (postersDecoracion != null)
        {
            foreach (Image poster in postersDecoracion) poster.gameObject.SetActive(false);
        }

        // 1. Elegir noticia al azar
        noticiaActual = tablaDeNoticias[UnityEngine.Random.Range(0, tablaDeNoticias.Count)];

        // 2. Actualizar texto UI
        if (textoNoticiaDia != null)
            textoNoticiaDia.text = $"NOTICIA DEL DÍA:\n\n\"{noticiaActual.headline}\"";

        // 3. Buscar 3 películas (Correcta, Neutral, Incorrecta) y asignarlas a botones
        PrepararOpcionesPeliculas();

        // 4. Reactivar botones para elegir
        for (int i = 0; i < botonesEleccion.Length; i++)
        {
            if (botonesEleccion[i] != null) botonesEleccion[i].interactable = true;
        }
    }

    public void PrepararOpcionesPeliculas()
    {
        // Buscamos una peli para cada caso
        MovieAsset peliCorrecta = BuscarPeliPorGenero(noticiaActual.correctGenre);
        MovieAsset peliNeutral = BuscarPeliPorGenero(noticiaActual.neutralGenre);
        MovieAsset peliIncorrecta = BuscarPeliPorGenero(noticiaActual.incorrectGenre);

        // --- CORRECCIÓN: Usamos la clase explícita en lugar de 'dynamic' ---
        List<OpcionTemporal> opciones = new List<OpcionTemporal>
        {
            new OpcionTemporal { Peli = peliCorrecta, Gen = noticiaActual.correctGenre },
            new OpcionTemporal { Peli = peliNeutral, Gen = noticiaActual.neutralGenre },
            new OpcionTemporal { Peli = peliIncorrecta, Gen = noticiaActual.incorrectGenre }
        };

        // Barajar (Shuffle) con Linq
        opciones = opciones.OrderBy(x => UnityEngine.Random.value).ToList();

        // Asignar a los 3 botones fijos
        for (int i = 0; i < 3; i++)
        {
            if (i >= botonesEleccion.Length) break;

            peliculasOpcion[i] = opciones[i].Peli;
            generosOpcion[i] = opciones[i].Gen;

            // Poner el poster en el botón
            if (postersEleccion[i] != null)
                postersEleccion[i].sprite = peliculasOpcion[i].posterImage;

            // Limpiar y asignar evento click
            botonesEleccion[i].onClick.RemoveAllListeners();
            int index = i;
            TextMeshProUGUI textoBoton = botonesEleccion[i].GetComponentInChildren<TextMeshProUGUI>();

            if (textoBoton != null)
            {
                textoBoton.text = peliculasOpcion[i].title + "\n" + peliculasOpcion[i].genre;
            }
            botonesEleccion[i].onClick.AddListener(() => ElegirPelicula(index));
        }
    }

    MovieAsset BuscarPeliPorGenero(CinemaGenre generoBuscado)
    {
        // Busca todas las pelis de ese género
        var candidatas = peliculasDisponibles.Where(p => p.genre == generoBuscado).ToList();

        if (candidatas.Count > 0)
            return candidatas[UnityEngine.Random.Range(0, candidatas.Count)];

        // Fallback: Si no tienes pelis de ese género, devuelve la primera que encuentre
        Debug.LogWarning($"¡Falta peli de género {generoBuscado}! Usando fallback.");
        return peliculasDisponibles[0];
    }

    public void ElegirPelicula(int indexBoton)
    {
        CinemaGenre generoElegido = generosOpcion[indexBoton];
        MovieAsset peliElegida = peliculasOpcion[indexBoton];

        string resultado = "";

        // Lógica de puntuación
        if (generoElegido == noticiaActual.correctGenre)
        {
            multiplicadorClientes = 1.5f; // +50% clientes
            resultado = "<color=green>¡ÉXITO TOTAL!</color>";
        }
        else if (generoElegido == noticiaActual.neutralGenre)
        {
            multiplicadorClientes = 1.0f; // Normal
            resultado = "<color=yellow>Recepción Normal.</color>";
        }
        else
        {
            multiplicadorClientes = 0.6f; // -40% clientes
            resultado = "<color=red>Fracaso de taquilla...</color>";
        }

        // Feedback visual en la propia noticia
        textoNoticiaDia.text = $"ESTRENO: {peliElegida.title}\n\n{resultado}\n(Afluencia esperada: {multiplicadorClientes}x)";
        if (postersDecoracion != null)
        {
            foreach (Image poster in postersDecoracion)
            {
                poster.gameObject.SetActive(true);
                poster.sprite = peliElegida.posterImage;
            }
        }

        // 1. Primero comprobamos si es nulo para evitar errores
        if (TaskManager.Instance != null)
        {
            // Debug útil: Mostramos que existe y llamamos al método
            Debug.Log("TaskManager encontrado. Marcando película como completada...");
            TaskManager.Instance.MarcarPeliculaCompletada();
        }
        else
        {
            // 2. Si es nulo, lanzamos un error para que te enteres
            Debug.LogError("¡ERROR CRÍTICO! TaskManager.Instance es NULL. ¿Has puesto el prefab del TaskManager en la escena?");
        }

        // Bloquear botones tras elegir
        foreach (var btn in botonesEleccion) btn.interactable = false;
    }

    // ----------------------------------------------------------------------
    // ANIMACIONES
    // ----------------------------------------------------------------------

    System.Collections.IEnumerator AnimacionAvisoCarrito(string mensaje, Color colorAviso)
    {
        string textoOriginal = textoTotalPrecio.text;
        Color colorOriginal = textoTotalPrecio.color;

        textoTotalPrecio.text = mensaje;
        textoTotalPrecio.color = colorAviso;

        yield return new WaitForSeconds(1f);

        textoTotalPrecio.text = textoOriginal;
        textoTotalPrecio.color = colorOriginal;
        ActualizarCarritoVisual();
    }

    // He mejorado tu animación del banco para que acepte Texto personalizado también
    System.Collections.IEnumerator AnimacionAvisoBanco(string mensaje = "", Color? colorFlash = null)
    {
        Color colorRojo = colorFlash ?? Color.red; // Si es null, usa rojo
        string textoOriginal = textoDineroDisponibleBanco.text;

        if (mensaje != "") textoDineroDisponibleBanco.text = mensaje;

        for (int i = 0; i < 3; i++)
        {
            textoDineroDisponibleBanco.color = colorRojo;
            yield return new WaitForSeconds(.2f);
            textoDineroDisponibleBanco.color = Color.white;
            yield return new WaitForSeconds(.2f);
        }

        textoDineroDisponibleBanco.text = textoOriginal; // Restauramos texto original
        ActualizarUIBanco();
    }

    System.Collections.IEnumerator AnimacionAvisoHome(string mensaje)
    {
        if (textoAvisoHome != null)
        {
            textoAvisoHome.text = mensaje;
            textoAvisoHome.color = Color.red;
            yield return new WaitForSeconds(1.5f);
            textoAvisoHome.text = ""; // Borramos el mensaje
        }
    }

    // Clase temporal auxiliar para evitar usar 'dynamic'
    private class OpcionTemporal
    {
        public MovieAsset Peli;
        public CinemaGenre Gen;
    }
}
