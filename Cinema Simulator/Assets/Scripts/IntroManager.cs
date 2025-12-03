using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class IntroManager : MonoBehaviour
{
    [Header("Referencias UI")]
    public CanvasGroup panelNegroIntro; // Panel negro para el fade inicial
    public GameObject panelCartaAbuelo; // El panel con la imagen de la carta y texto

    public GameObject panelInterfaz;

    [Header("Configuración")]
    public float velocidadFade = 0.5f; // Qué tan rápido se va el negro
    public float esperaAntesDeCarta = 1.0f; // Tiempo entre que se ve el juego y sale la carta

    private ControladorInteraccion jugador;

    void Start()
    {
        // 1. Buscamos al jugador y lo BLOQUEAMOS inmediatamente
        jugador = FindObjectOfType<ControladorInteraccion>();
        if (jugador != null) jugador.AlternarControlJugador(false);

        // 2. Preparamos la UI: Pantalla negra visible, Carta oculta
        if (panelNegroIntro != null)
        {
            panelNegroIntro.gameObject.SetActive(true);
            panelNegroIntro.alpha = 1; // Totalmente negro
        }
        if (panelCartaAbuelo != null) panelCartaAbuelo.SetActive(false);

        if (panelInterfaz != null) panelInterfaz.SetActive(false);

        // 3. Iniciamos la secuencia
        StartCoroutine(SecuenciaIntro());
    }

    IEnumerator SecuenciaIntro()
    {
        // Esperamos un segundo en negro absoluto para que cargue todo
        yield return new WaitForSeconds(1f);

        // --- FASE 1: FADE IN (De negro a transparente) ---
        float t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime * velocidadFade;
            // Lerp de 1 (negro) a 0 (transparente)
            panelNegroIntro.alpha = Mathf.Lerp(1f, 0f, t);
            yield return null;
        }
        panelNegroIntro.gameObject.SetActive(false); // Apagamos el negro

        // --- FASE 2: ESPERA DRAMÁTICA ---
        yield return new WaitForSeconds(esperaAntesDeCarta);

        // --- FASE 3: MOSTRAR CARTA ---
        panelCartaAbuelo.SetActive(true);

        // Desbloqueamos el ratón para que pueda cerrar la carta
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // Esta función la llamará el botón "Cerrar / Empezar" de la carta
    public void CerrarCarta()
    {
        panelCartaAbuelo.SetActive(false);

        // --- FASE 4: EMPEZAR JUEGO ---
        // Devolvemos el control al jugador
        if (jugador != null) jugador.AlternarControlJugador(true);

        // Bloqueamos el ratón para jugar
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        panelInterfaz.SetActive(true);

        if (GameManager.Instance != null)
        {
            GameManager.Instance.IniciarPartida();
        }

        // Destruimos este objeto porque la intro ya no sirve para nada
        Destroy(gameObject);
    }
}