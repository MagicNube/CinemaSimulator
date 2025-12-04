using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class TransitionManager : MonoBehaviour
{
    public static TransitionManager Instance;

    [Header("UI Referencias")]
    public CanvasGroup panelNegroCG; // Para hacer Fade In/Out
    public TextMeshProUGUI textoTituloFase; // "FASE DE SERVICIO"
    public TextMeshProUGUI textoSubtitulo; // "Día 1 | Semana 1"

    [Header("UI Informe Diario")]
    public GameObject panelInforme; // El contenedor del informe
    public TextMeshProUGUI textoInformeDetalles; // Donde ponemos los números

    public TextMeshProUGUI textoContinuar;

    [Header("Configuración")]
    public float duracionFade = 1.0f;
    public float tiempoEsperaPantalla = 2.0f; // Cuánto rato se queda en negro

    public bool transicionando = false;

    private ControladorInteraccion jugador;

    void Awake()
    {
        // Configuración Singleton básica
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    [System.Obsolete]
    void Start()
    {
        // Estado inicial limpio
        panelNegroCG.alpha = 0;
        panelNegroCG.blocksRaycasts = false;
        panelInforme.SetActive(false);
        if (textoContinuar != null) textoContinuar.gameObject.SetActive(false);
        jugador = FindObjectOfType<ControladorInteraccion>();
    }

    void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.AlCambiarFase -= IniciarTransicion;
    }

    public void IniciarTransicion(FaseJuego nuevaFase)
    {
        transicionando = true;
        StopAllCoroutines();
        panelNegroCG.gameObject.SetActive(true);
        textoTituloFase.text = "";
        textoSubtitulo.text = "";
        StartCoroutine(SecuenciaTransicion(nuevaFase));
    }

    IEnumerator SecuenciaTransicion(FaseJuego fase)
    {
        if (jugador != null) jugador.AlternarControlJugador(false);
        // 1. FADE IN (La pantalla se va a negro)
        panelNegroCG.blocksRaycasts = true; // Bloqueamos clicks
        yield return Fade(0, 1);

        // 2. CONFIGURAR TEXTOS (Mientras está en negro)
        panelInforme.SetActive(false); // Por defecto apagado
        textoSubtitulo.text = ""; // Por defecto vacío

        bool esperarInput = false;

        switch (fase)
        {
            case FaseJuego.Fase1_Preparacion:
                if (GameManager.Instance.diaActual > 1)
                {
                    MostrarInformeFinDia();
                    esperarInput = true;
                }

                textoTituloFase.text = "FASE DE PLANIFICACIÓN";
                textoSubtitulo.text = $"DÍA {GameManager.Instance.diaActual} | SEMANA {(GameManager.Instance.diaActual - 1) / 2 + 1}";
                break;

            case FaseJuego.Fase2_Servicio:
                textoTituloFase.text = "FASE DE SERVICIO";
                textoSubtitulo.text = "¡Abre las puertas!";
                break;

            case FaseJuego.Fase3_Cierre:
                textoTituloFase.text = "FASE DE CIERRE";
                textoSubtitulo.text = "Gestión y Stock";
                break;

            case FaseJuego.Fase4_Limpieza:
                textoTituloFase.text = "FASE DE LIMPIEZA";
                break;
        }

        if (esperarInput)
        {

            textoContinuar.gameObject.SetActive(true);

            // Espera de seguridad
            yield return null;
            yield return new WaitUntil(() => Input.anyKeyDown);

            textoContinuar.gameObject.SetActive(false);
            panelInforme.SetActive(false);
            EconomyManager.Instance.ResetearDatosDiarios();
        }
        else
        {
            yield return new WaitForSecondsRealtime(tiempoEsperaPantalla);
        }

        // 4. FADE OUT (Vuelve el juego)
        yield return Fade(1, 0);
        panelNegroCG.blocksRaycasts = false; // Desbloqueamos clicks
        transicionando = false;
        if (jugador != null) jugador.AlternarControlJugador(true);
    }

    void MostrarInformeFinDia()
    {
        panelInforme.SetActive(true);
        textoTituloFase.text = "RESUMEN DEL DÍA";

        int ingresos = EconomyManager.Instance.ingresosHoy;
        int gastos = EconomyManager.Instance.gastosHoy;
        int balance = ingresos - gastos;

        string colorBalance = balance >= 0 ? "green" : "red";

        textoInformeDetalles.text =
            $"Ingresos: <color=green>+${ingresos}</color>\n" +
            $"Gastos: <color=red>-${gastos}</color>\n" +
            $"----------------\n" +
            $"Balance: <color={colorBalance}>${balance}</color>\n\n" +
            $"<i>Mañana recibirás tus pedidos...</i>";
    }

    // Función auxiliar para fundido suave
    IEnumerator Fade(float start, float end)
    {
        float timer = 0;
        while (timer < duracionFade)
        {
            // CAMBIO: Usamos unscaledDeltaTime para que funcione aunque el juego esté en pausa
            timer += Time.unscaledDeltaTime;

            panelNegroCG.alpha = Mathf.Lerp(start, end, timer / duracionFade);
            yield return null;
        }
        panelNegroCG.alpha = end;
    }
}
