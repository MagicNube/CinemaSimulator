using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MinijuegoLimpiezaManager : MonoBehaviour
{
    public static MinijuegoLimpiezaManager Instance;

    [Header("Configuración")]
    public float tiempoLimite = 60f;
    private float tiempoRestante;
    private bool juegoActivo = false;
    private List<GameObject> basuraEnJuego = new List<GameObject>();

    [Header("Referencias Generales")]
    public ControlAleatorioBasura generadorBasura;

    [Header("Audio")]
    [Tooltip("Arrastra aquí el componente AudioSource que usará la MÚSICA DE FONDO")]
    public AudioSource audioSourceMusica;
    [Tooltip("Arrastra aquí el componente AudioSource que usará los EFECTOS (Win/Lose)")]
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

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        if (panelPregunta != null) panelPregunta.SetActive(false);
        if (panelTimer != null) panelTimer.SetActive(false);
    }

    void Update()
    {
        if (juegoActivo) GestionarTiempo();
    }

    public void MostrarPregunta()
    {
        if (juegoActivo) return;
        if (generadorBasura == null || generadorBasura.objetosBasura == null || generadorBasura.objetosBasura.Count == 0) return;

        ActualizarListaBasura();
        if (CalcularBasuraRestante() > 0)
        {
            if (panelPregunta != null) panelPregunta.SetActive(true);
            if (panelTimer != null) panelTimer.SetActive(false);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    public void RechazarMinijuego()
    {
        if (panelPregunta) panelPregunta.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void IniciarMinijuego()
    {
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

        // --- AUDIO: INICIAR MÚSICA ---
        if (audioSourceMusica != null && musicaTension != null)
        {
            audioSourceMusica.clip = musicaTension;
            audioSourceMusica.loop = true; // Que se repita si el audio es corto
            audioSourceMusica.Play();
        }

        ActualizarHUD();
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
        if (!juegoActivo) return;

        // Buscar coincidencia exacta o padre/hijo
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

        // --- AUDIO: PARAR MÚSICA Y LANZAR SFX ---
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