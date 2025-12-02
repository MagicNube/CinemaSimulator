using UnityEngine;

public class LightingManager : MonoBehaviour
{
    [Header("Referencias")]
    public Light sol; // Arrastra tu Directional Light aquí

    [Header("Configuración de Transición")]
    public float velocidadTransicion = 2.0f; // Qué tan rápido cambia la luz

    // Definimos cómo se ve el sol en cada fase
    [System.Serializable]
    public struct AmbienteFase
    {
        public string nombreFase; // Solo para que te aclares en el inspector
        public Vector3 rotacionSol; // El ángulo del sol (Eje X es la altura)
        public Color colorLuz;      // El color de la luz
        public float intensidad;    // Brillo (1 para día, 0.2 para noche)
    }

    [Header("Configuración por Fases")]
    public AmbienteFase ambienteFase1; // Mañana
    public AmbienteFase ambienteFase2; // Mediodía
    public AmbienteFase ambienteFase3; // Atardecer/Cierre
    public AmbienteFase ambienteFase4; // Noche/Limpieza

    // Variables internas para la interpolación
    private Quaternion rotacionObjetivo;
    private Color colorObjetivo;
    private float intensidadObjetivo;

    void Start()
    {
        if (sol == null)
        {
            Debug.LogError("¡Falta asignar la Luz (Sol) en el LightingManager!");
            return;
        }

        // Nos suscribimos al evento de cambio de fase del GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AlCambiarFase += ActualizarIluminacion;

            // Inicializamos con la fase actual
            ActualizarIluminacion(GameManager.Instance.faseActual);

            // Aplicamos instantáneamente al inicio para que no haga transición rara
            sol.transform.rotation = rotacionObjetivo;
            sol.color = colorObjetivo;
            sol.intensity = intensidadObjetivo;
        }
    }

    void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.AlCambiarFase -= ActualizarIluminacion;
    }

    void Update()
    {
        if (sol == null) return;

        // --- MAGIA: INTERPOLACIÓN SUAVE (Lerp) ---
        // Movemos la rotación actual hacia la objetivo poco a poco
        sol.transform.rotation = Quaternion.Slerp(sol.transform.rotation, rotacionObjetivo, Time.deltaTime * velocidadTransicion);

        // Cambiamos el color suavemente
        sol.color = Color.Lerp(sol.color, colorObjetivo, Time.deltaTime * velocidadTransicion);

        // Cambiamos la intensidad suavemente
        sol.intensity = Mathf.Lerp(sol.intensity, intensidadObjetivo, Time.deltaTime * velocidadTransicion);
    }

    // Esta función se llama sola cuando el GameManager cambia de fase
    void ActualizarIluminacion(FaseJuego nuevaFase)
    {
        AmbienteFase perfil = new AmbienteFase();

        switch (nuevaFase)
        {
            case FaseJuego.Fase1_Preparacion: // MAÑANA
                perfil = ambienteFase1;
                break;
            case FaseJuego.Fase2_Servicio:    // MEDIODÍA
                perfil = ambienteFase2;
                break;
            case FaseJuego.Fase3_Cierre:      // ATARDECER
                perfil = ambienteFase3;
                break;
            case FaseJuego.Fase4_Limpieza:    // NOCHE
                perfil = ambienteFase4;
                break;
        }

        // Guardamos los objetivos para que el Update haga el trabajo sucio
        rotacionObjetivo = Quaternion.Euler(perfil.rotacionSol);
        colorObjetivo = perfil.colorLuz;
        intensidadObjetivo = perfil.intensidad;
    }
}