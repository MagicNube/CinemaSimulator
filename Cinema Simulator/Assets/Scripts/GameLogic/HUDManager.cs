using UnityEngine;
using TMPro;

public class HUDManager : MonoBehaviour
{
    // Singleton para llamarlo fácil desde cualquier sitio
    public static HUDManager Instance;

    [Header("Referencias UI")]
    public TextMeshProUGUI textoFechaGlobal; // El texto en la esquina de la pantalla
    [Header("Referencias Timer Servicio")]
    public GameObject panelTimerCompleto; // Panel del timer
    public TextMeshProUGUI textoNumerosTimer; // Texto del timer

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // Al empezar, actualizamos la fecha para que no salga vacía
        ActualizarFecha();

        if (panelTimerCompleto != null)
            panelTimerCompleto.gameObject.SetActive(false);
    }

    void Update()
    {
        if (GameManager.Instance == null) return;

        // LÓGICA DEL TIMER
        if (GameManager.Instance.faseActual == FaseJuego.Fase2_Servicio)
        {
            // 1. Si el panel está apagado, lo encendemos
            if (panelTimerCompleto != null && !panelTimerCompleto.activeSelf)
                panelTimerCompleto.SetActive(true);

            // 2. Actualizamos el texto de los números
            if (textoNumerosTimer != null)
            {
                float tiempo = GameManager.Instance.tiempoRestanteServicio;
                if (tiempo < 0) tiempo = 0;

                int minutos = Mathf.FloorToInt(tiempo / 60);
                int segundos = Mathf.FloorToInt(tiempo % 60);

                textoNumerosTimer.text = string.Format("{0:00}:{1:00}", minutos, segundos);
            }
        }
        else
        {
            // Si NO es la fase de servicio y el panel sigue encendido, lo apagamos
            if (panelTimerCompleto != null && panelTimerCompleto.activeSelf)
                panelTimerCompleto.SetActive(false);
        }
    }

    public void ActualizarFecha()
    {
        if (textoFechaGlobal != null && GameManager.Instance != null)
        {
            int dia = GameManager.Instance.diaActual;

            // Formato: "DÍA 5 | SEMANA 3"
            textoFechaGlobal.text = $"Dia: {dia} - Semana: {(dia - 1) / 2 + 1}";
        }
    }
}