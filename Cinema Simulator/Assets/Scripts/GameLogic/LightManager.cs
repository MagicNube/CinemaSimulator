using UnityEngine;

public class LightingManager : MonoBehaviour
{
    [Header("Referencias")]
    public Light sol;

    [Header("Configuración de Transición")]
    public float velocidadTransicion = 0.5f;

    [System.Serializable]
    public struct AmbienteFase
    {
        public string nombreFase;
        public Vector3 rotacionSol;
        public Color colorLuzSol;
        public float intensidadSol;

        [Header("Nueva Variable de Oscuridad")]
        public Color colorAmbiente;
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

            sol.transform.rotation = rotacionObjetivo;
            sol.color = colorSolObjetivo;
            sol.intensity = intensidadSolObjetivo;
            RenderSettings.ambientLight = colorAmbienteObjetivo;
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

        sol.transform.rotation = Quaternion.Slerp(sol.transform.rotation, rotacionObjetivo, Time.deltaTime * velocidadTransicion);
        sol.color = Color.Lerp(sol.color, colorSolObjetivo, Time.deltaTime * velocidadTransicion);
        sol.intensity = Mathf.Lerp(sol.intensity, intensidadSolObjetivo, Time.deltaTime * velocidadTransicion);

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