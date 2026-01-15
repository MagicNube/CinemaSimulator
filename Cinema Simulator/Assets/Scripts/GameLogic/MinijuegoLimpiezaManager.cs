using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MinijuegoLimpiezaManager : MonoBehaviour
{
    public static MinijuegoLimpiezaManager Instance;

    [Header("Configuraci�n")]
    public float tiempoLimite = 60f;
    private float tiempoRestante;
    private bool juegoActivo = false;
    private List<GameObject> basuraEnJuego = new List<GameObject>();

    [Header("Referencias Generales")]
    public ControlAleatorioBasura generadorBasura;
    [Tooltip("Arrastra la zona de activación del minijuego para bloquearla")]
    public Collider zonaActivacionCollider;
    [Tooltip("Distancia en unidades que se moverá el collider para bloquear (ejemplo: 2 para mover 2 metros hacia un lado)")]
    public float distanciaMovimiento = 2f;
    
    private Vector3 posicionOriginalCollider;
    private bool posicionGuardada = false;

    [Header("Audio")]
    [Tooltip("Arrastra aqu� el componente AudioSource que usar� la M�SICA DE FONDO")]
    public AudioSource audioSourceMusica;
    [Tooltip("Arrastra aqu� el componente AudioSource que usar� los EFECTOS (Win/Lose)")]
    public AudioSource audioSourceSFX;

    [Header("Clips de Audio")]
    public AudioClip musicaTension;
    public AudioClip sonidoVictoria;
    public AudioClip sonidoDerrota;

    [Header("Tus Paneles UI")]
    public GameObject panelPregunta;
    public GameObject panelTimer;

    [Header("Textos del Timer")]
    public TextMeshProUGUI textoTimer;
    public TextMeshProUGUI textoContadorBasura;

    private ControladorInteraccion jugador;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    [System.Obsolete]
    void Start()
    {
        ResetearMinijuego();
    }

    void ResetearMinijuego()
    {
        if (panelPregunta != null) panelPregunta.SetActive(false);
        if (panelTimer != null) panelTimer.SetActive(false);
        juegoActivo = false;
        tiempoRestante = tiempoLimite;
        basuraEnJuego.Clear();
        jugador = FindObjectOfType<ControladorInteraccion>();
        
        // Guardar posición original del collider si aún no se ha guardado
        if (zonaActivacionCollider != null && !posicionGuardada)
        {
            posicionOriginalCollider = zonaActivacionCollider.transform.position;
            posicionGuardada = true;
        }
    }

    void Update()
    {
        if (juegoActivo) GestionarTiempo();
    }

    public void MostrarPregunta()
    {
        if (juegoActivo) return;
        if (generadorBasura == null) return;


        jugador.AlternarControlJugador(false);
        if (panelPregunta != null) panelPregunta.SetActive(true);
        if (panelTimer != null) panelTimer.SetActive(false);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void RechazarMinijuego()
    {
        if (panelPregunta) panelPregunta.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        jugador.AlternarControlJugador(true);
    }

    public void IniciarMinijuego()
    {
        generadorBasura.GenerarBasura();
        ActualizarListaBasura();

        jugador.AlternarControlJugador(true);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        ActualizarListaBasura();
        tiempoRestante = tiempoLimite;
        juegoActivo = true;

        if (panelPregunta) panelPregunta.SetActive(false);
        if (panelTimer) panelTimer.SetActive(true);

        if (textoTimer) textoTimer.gameObject.SetActive(true);
        if (textoContadorBasura)
        {
            textoContadorBasura.gameObject.SetActive(true);
            textoContadorBasura.color = Color.white;
        }

        // --- BLOQUEAR LA SALIDA DE LA SALA (con delay para que el jugador pueda entrar primero) ---
        StartCoroutine(BloquearSalaConDelay());

        // --- AUDIO: INICIAR M�SICA ---
        if (audioSourceMusica != null && musicaTension != null)
        {
            audioSourceMusica.clip = musicaTension;
            audioSourceMusica.loop = true; // Que se repita si el audio es corto
            audioSourceMusica.Play();
        }

        ActualizarHUD();
    }

    IEnumerator BloquearSalaConDelay()
    {
        // Esperar medio segundo para que el jugador entre completamente
        yield return new WaitForSeconds(0.5f);
        
        if (zonaActivacionCollider != null)
        {
            // Convertir el trigger en collider sólido
            zonaActivacionCollider.isTrigger = false;
            
            // Mover el collider sobre el eje X global (izquierda/derecha)
            zonaActivacionCollider.transform.position = posicionOriginalCollider + Vector3.right * distanciaMovimiento;
            
            Debug.Log("Puerta bloqueada - No se puede salir hasta terminar el minijuego");
        }
    }

    void GestionarTiempo()
    {
        tiempoRestante -= Time.deltaTime;
        if (tiempoRestante <= 0)
        {
            tiempoRestante = 0;
            FinDelJuego(false);
        }
        ActualizarHUD();
    }

    public void ObjetoRecogido(GameObject objetoRecogido)
    {
        Debug.Log("He recibido un aviso de recogida!: " + objetoRecogido.name); // <--- A�ADE ESTO
        if (!juegoActivo) return;

        GameObject objetoEncontrado = null;
        for (int i = 0; i < basuraEnJuego.Count; i++)
        {
            if (basuraEnJuego[i] == null) continue;
            if (basuraEnJuego[i] == objetoRecogido || objetoRecogido.transform.IsChildOf(basuraEnJuego[i].transform))
            {
                objetoEncontrado = basuraEnJuego[i];
                break;
            }
        }

        if (objetoEncontrado != null) basuraEnJuego.Remove(objetoEncontrado);
        basuraEnJuego.RemoveAll(item => item == null);

        if (basuraEnJuego.Count == 0) FinDelJuego(true);
        else ActualizarHUD();
    }

    void FinDelJuego(bool victoria)
    {
        juegoActivo = false;

        // --- DESBLOQUEAR LA SALIDA DE LA SALA ---
        if (zonaActivacionCollider != null)
        {
            // Devolver el collider a su posición original
            zonaActivacionCollider.transform.position = posicionOriginalCollider;
            
            // Convertir de nuevo en trigger
            zonaActivacionCollider.isTrigger = true;
            
            Debug.Log("Puerta desbloqueada - Ya puedes salir de la sala");
        }

        // --- AUDIO: PARAR M�SICA Y LANZAR SFX ---
        if (audioSourceMusica != null) audioSourceMusica.Stop();

        if (audioSourceSFX != null)
        {
            if (victoria && sonidoVictoria != null)
                audioSourceSFX.PlayOneShot(sonidoVictoria);
            else if (!victoria && sonidoDerrota != null)
                audioSourceSFX.PlayOneShot(sonidoDerrota);
        }

        if (!victoria) BloquearBasuraRestante();

        StartCoroutine(SecuenciaFinPartida(victoria));
    }

    IEnumerator SecuenciaFinPartida(bool victoria)
    {
        if (textoTimer) textoTimer.gameObject.SetActive(false);

        if (textoContadorBasura)
        {
            textoContadorBasura.gameObject.SetActive(true);
            if (victoria)
            {
                textoContadorBasura.text = "¡FELICIDADES!\nLIMPIEZA COMPLETADA";
                textoContadorBasura.color = Color.green;
            }
            else
            {
                textoContadorBasura.text = "TIEMPO AGOTADO\nNO HAS RECOGIDO TODO";
                textoContadorBasura.color = Color.red;
            }
        }

        yield return new WaitForSeconds(4f);

        if (panelTimer != null) panelTimer.SetActive(false);
    }

    void ActualizarHUD()
    {
        if (!juegoActivo) return;

        if (textoTimer)
        {
            textoTimer.text = Mathf.FloorToInt(tiempoRestante / 60).ToString("00") + ":" + Mathf.FloorToInt(tiempoRestante % 60).ToString("00");
            textoTimer.color = (tiempoRestante < 10) ? Color.red : Color.white;
        }

        if (textoContadorBasura)
        {
            textoContadorBasura.text = "Basura: " + basuraEnJuego.Count;
        }
    }

    void ActualizarListaBasura()
    {
        basuraEnJuego.Clear();
        if (generadorBasura != null)
        {
            foreach (var obj in generadorBasura.objetosBasura)
            {
                if (obj != null && obj.activeInHierarchy) basuraEnJuego.Add(obj);
            }
        }
    }

    int CalcularBasuraRestante()
    {
        int count = 0;
        foreach (var item in basuraEnJuego) if (item != null && item.activeInHierarchy) count++;
        return count;
    }

    void BloquearBasuraRestante() { foreach (var item in basuraEnJuego) if (item != null) { Collider c = item.GetComponent<Collider>(); if (c) c.enabled = false; } }
}