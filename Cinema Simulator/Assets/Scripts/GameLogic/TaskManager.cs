using UnityEngine;
using TMPro;

public class TaskManager : MonoBehaviour
{
    public static TaskManager Instance; // Singleton para acceder fácil

    [Header("UI Tablón")]
    public TextMeshProUGUI textoListaTareas;

    [Header("Estado Tareas (Fase 1)")]
    public bool haElegidoPelicula = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    void Start()
    {
        // Nos suscribimos al evento de cambio de fase del GameManager
        // Así, cada vez que cambie la fase, el tablón se actualiza solo
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AlCambiarFase += ActualizarTablon;
        }

        // Pintamos el estado inicial
        ActualizarTablon(FaseJuego.Fase1_Preparacion);
    }

    void OnDestroy()
    {
        // Buena práctica: desuscribirse al destruir para evitar errores
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AlCambiarFase -= ActualizarTablon;
        }
    }

    // Esta función la llamaremos desde TabletManager cuando elijas peli
    public void MarcarPeliculaCompletada()
    {
        haElegidoPelicula = true;
        ActualizarTablon(GameManager.Instance.faseActual);
    }

    // Esta función la llamaremos al iniciar un nuevo día para resetear el check
    public void ResetearTareasDiarias()
    {
        haElegidoPelicula = false;
        ActualizarTablon(FaseJuego.Fase1_Preparacion);
    }

    // Lógica principal de pintado
    public void ActualizarTablon(FaseJuego fase)
    {
        if (textoListaTareas == null) return;

        string contenido = "";

        switch (fase)
        {
            case FaseJuego.Fase1_Preparacion:
                contenido += "<size=120%><b><u>PLANIFICACIÓN</u></b></size>\n\n";

                // Tarea Obligatoria con lógica visual
                if (haElegidoPelicula)
                    contenido += "<color=green><s><b>·</b> Elegir Película</s></color>\n";
                else
                    contenido += "<color=red><b>·</b> Elegir Película (Obligatorio)</color>\n";

                // Tareas recordatorio (sin lógica compleja por ahora)
                contenido += "<b>·</b> Recoger Cajas\n";
                contenido += "<b>·</b> Reponer Máquinas\n";
                break;

            case FaseJuego.Fase2_Servicio:
                contenido += "<size=120%><b><u>SERVICIO</u></b></size>\n\n";
                contenido += ">> ATENDER CLIENTES <<\n";
                contenido += "- Vender entradas\n";
                contenido += "- Servir snacks\n";
                contenido += "- ¡Cuidado con la cola!\n";
                break;

            case FaseJuego.Fase3_Cierre:
                contenido += "<size=120%><b><u>CIERRE</u></b></size>\n\n";
                contenido += "<b>·</b> Revisar Balance (Banco)\n";
                contenido += "<b>·</b> Realizar Pedidos (Tienda)\n";
                contenido += "Interactúa con la puerta para terminar.\n";
                break;

            case FaseJuego.Fase4_Limpieza:
                contenido += "<size=120%><b><u>LIMPIEZA</u></b></size>\n\n";
                contenido += "<b>·</b> Limpiar basura de la sala\n";
                contenido += "(Opcional: Salir para omitir)\n";
                break;
        }

        textoListaTareas.text = contenido;
    }
}