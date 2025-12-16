using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    [Header("UI")]
    public GameObject panelPausa;

    [Header("Referencias")]
    public TabletManager tabletManager;
    private ControladorInteraccion jugador;

    private bool estaPausado = false;

    void Start()
    {
        jugador = FindObjectOfType<ControladorInteraccion>();
        if (tabletManager == null) tabletManager = FindObjectOfType<TabletManager>();

        if (panelPausa != null) panelPausa.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (TransitionManager.Instance != null && TransitionManager.Instance.transicionando) return;

            if (tabletManager != null && tabletManager.interfazAbierta) return;

            AlternarPausa();
        }
    }

    public void AlternarPausa()
    {
        estaPausado = !estaPausado;

        if (estaPausado)
        {
            ActivarPausa();
        }
        else
        {
            DesactivarPausa();
        }
    }

    void ActivarPausa()
    {
        panelPausa.SetActive(true);

        Time.timeScale = 0f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (jugador != null) jugador.AlternarControlJugador(false);
    }

    public void DesactivarPausa()
    {
        estaPausado = false;
        panelPausa.SetActive(false);

        Time.timeScale = 1f;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (jugador != null) jugador.AlternarControlJugador(true);
    }

    public void IrAlMenuPrincipal()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(0);
    }

    public void SalirDelJuego()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }
}