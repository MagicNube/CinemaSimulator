using UnityEngine;
using System;
using TMPro; // Para usar Eventos (Action)

// Definimos las fases fuera de la clase para que sean accesibles globalmente
public enum FaseJuego
{
    Fase1_Preparacion, // Tiempo congelado. Stock, Tablet, Elegir Peli.
    Fase2_Servicio,    // Tiempo corre. Clientes entran. Estrés.
    Fase3_Cierre,      // Tiempo congelado. Gestión, Mejoras.
    Fase4_Limpieza     // Minijuego opcional.
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Estado del Juego")]
    public FaseJuego faseActual;
    public int diaActual = 1;

    [Header("Configuración Fase Servicio")]
    public float duracionDiaMinutos = 20f; // 20 minutos reales
    private float tiempoRestanteServicio;

    // Evento al que otros scripts se pueden suscribir para saber cuándo cambia la fase
    // Ejemplo: Los clientes escuchan esto para dejar de venir.
    public event Action<FaseJuego> AlCambiarFase;

    [Header("Configuración Inicio")]
    public bool hayIntroAlInicio = true;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        if (hayIntroAlInicio)
        {
            Debug.Log("GameManager: Esperando a que termine la Intro...");
            // No hacemos nada. El IntroManager nos llamará luego.
        }
        else
        {
            // Si no hay intro (modo pruebas), empezamos directo
            IniciarPartida();
        }
    }

    public void IniciarPartida()
    {
        Debug.Log("GameManager: ¡Intro terminada! Arrancando el día.");
        CambiarFase(FaseJuego.Fase1_Preparacion);
    }

    void Update()
    {
        // Lógica exclusiva de la FASE 2 (Servicio)
        if (faseActual == FaseJuego.Fase2_Servicio)
        {
            if (tiempoRestanteServicio > 0)
            {
                tiempoRestanteServicio -= Time.deltaTime;
            }
            else
            {
                // Se acabó el tiempo -> Pasamos a Cierre automáticamente
                Debug.Log("¡Fin del servicio! Cerrando puertas...");
                CambiarFase(FaseJuego.Fase3_Cierre);
            }
        }
    }

    // Función central para movernos entre estados
    public void CambiarFase(FaseJuego nuevaFase)
    {
        faseActual = nuevaFase;

        if (TransitionManager.Instance != null)
        {
            Debug.Log("Llamando transición");
            TransitionManager.Instance.IniciarTransicion(nuevaFase);
        }
        else
        {
            Debug.LogWarning("⚠️ GameManager: No encuentro el TransitionManager para poner la pantalla negra.");
        }

        // Avisamos a todo el juego de que la fase ha cambiado
        AlCambiarFase?.Invoke(nuevaFase);

        Debug.Log($"--- CAMBIO DE FASE: {nuevaFase} ---");

        switch (nuevaFase)
        {
            case FaseJuego.Fase1_Preparacion:
                // Lógica de inicio de día
                // Aquí podrías llamar a TabletManager para generar nueva noticia
                break;

            case FaseJuego.Fase2_Servicio:
                // Configurar temporizador (minutos * 60 segundos)
                tiempoRestanteServicio = duracionDiaMinutos * 60f;
                // Aquí activarías el "Spawner de Clientes"
                break;

            case FaseJuego.Fase3_Cierre:
                // Detener Spawner de Clientes
                // Mostrar resumen de ganancias del día
                break;

            case FaseJuego.Fase4_Limpieza:
                // Activar minijuego de basura
                break;
        }
    }

    [Obsolete]
    public void AvanzarSiguienteFase()
    {
        switch (faseActual)
        {
            case FaseJuego.Fase1_Preparacion:
                if (TaskManager.Instance != null && !TaskManager.Instance.haElegidoPelicula)
                {
                    return;
                }
                CambiarFase(FaseJuego.Fase2_Servicio);
                break;

            case FaseJuego.Fase2_Servicio:
                CambiarFase(FaseJuego.Fase3_Cierre);
                break;

            case FaseJuego.Fase3_Cierre:
                CambiarFase(FaseJuego.Fase4_Limpieza);
                break;

            case FaseJuego.Fase4_Limpieza:
                FinalizarDia();
                break;
        }
    }

    // Llamado desde la Puerta Principal en Fase 1
    public void AbrirCine()
    {
        if (faseActual == FaseJuego.Fase1_Preparacion)
        {
            CambiarFase(FaseJuego.Fase2_Servicio);
        }
    }

    // Llamado desde la Sala en Fase 3
    public void EmpezarLimpieza()
    {
        if (faseActual == FaseJuego.Fase3_Cierre)
        {
            CambiarFase(FaseJuego.Fase4_Limpieza);
        }
    }

    // Llamado al terminar limpieza o saltarla en Fase 3
    [Obsolete]

    public void FinalizarDia()
    {
        diaActual++;

        FindObjectOfType<TabletManager>().NuevoDiaCine();

        if (HUDManager.Instance != null)
        {
            HUDManager.Instance.ActualizarFecha();
        }

        if (TaskManager.Instance != null)
        {
            TaskManager.Instance.ResetearTareasDiarias();
        }

        CambiarFase(FaseJuego.Fase1_Preparacion);
    }

    public void FinalizarServicioPorFaltaDeClientes()
    {
        if (faseActual == FaseJuego.Fase2_Servicio)
        {
            Debug.Log("¡Se han acabado los clientes! Cerrando antes de tiempo.");
            CambiarFase(FaseJuego.Fase3_Cierre);
        }
    }
}