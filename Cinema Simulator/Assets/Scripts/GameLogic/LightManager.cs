using UnityEngine;

public class LightingManager : MonoBehaviour
{
    [Header("Referencias")]
    public Light sol;

    [Header("Configuración de Transición")]
    public float velocidadTransicion = 0.5f; // Un poco más lento para que sea suave

    [System.Serializable]
    public struct AmbienteFase
    {
        public string nombreFase;
        public Vector3 rotacionSol;
        public Color colorLuzSol;
        public float intensidadSol;

        [Header("Nueva Variable de Oscuridad")]
        public Color colorAmbiente; // <--- ESTO CONTROLA LA OSCURIDAD GENERAL
    }

    [Header("Configuración por Fases")]
    public AmbienteFase ambienteFase1; // Mañana
    public AmbienteFase ambienteFase2; // Mediodía
    public AmbienteFase ambienteFase3; // Atardecer
    public AmbienteFase ambienteFase4; // Noche

    // Variables internas
    private Quaternion rotacionObjetivo;
    private Color colorSolObjetivo;
    private float intensidadSolObjetivo;
    private Color colorAmbienteObjetivo;

    void Start()
    {
        if (sol == null) return;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.AlCambiarFase += ActualizarIluminacion;
            ActualizarIluminacion(GameManager.Instance.faseActual);

            // Aplicar instantáneo al inicio
            sol.transform.rotation = rotacionObjetivo;
            sol.color = colorSolObjetivo;
            sol.intensity = intensidadSolObjetivo;
            RenderSettings.ambientLight = colorAmbienteObjetivo; // Aplicar ambiente
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

        // 1. SOL
        sol.transform.rotation = Quaternion.Slerp(sol.transform.rotation, rotacionObjetivo, Time.deltaTime * velocidadTransicion);
        sol.color = Color.Lerp(sol.color, colorSolObjetivo, Time.deltaTime * velocidadTransicion);
        sol.intensity = Mathf.Lerp(sol.intensity, intensidadSolObjetivo, Time.deltaTime * velocidadTransicion);

        // 2. AMBIENTE (El truco de la oscuridad)
        // Interpolamos el color general del mundo
        RenderSettings.ambientLight = Color.Lerp(RenderSettings.ambientLight, colorAmbienteObjetivo, Time.deltaTime * velocidadTransicion);
    }

    void ActualizarIluminacion(FaseJuego nuevaFase)
    {
        AmbienteFase perfil = new AmbienteFase();

        switch (nuevaFase)
        {
            case FaseJuego.Fase1_Preparacion: perfil = ambienteFase1; break;
            case FaseJuego.Fase2_Servicio: perfil = ambienteFase2; break;
            case FaseJuego.Fase3_Cierre: perfil = ambienteFase3; break;
            case FaseJuego.Fase4_Limpieza: perfil = ambienteFase4; break;
        }

        rotacionObjetivo = Quaternion.Euler(perfil.rotacionSol);
        colorSolObjetivo = perfil.colorLuzSol;
        intensidadSolObjetivo = perfil.intensidadSol;
        colorAmbienteObjetivo = perfil.colorAmbiente;
    }
}