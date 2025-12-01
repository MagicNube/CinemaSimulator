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

        // Reseteamos visuales: Aseguramos que ambos textos se ven y están en blanco
        if (textoTimer) textoTimer.gameObject.SetActive(true);
        if (textoContadorBasura)
        {
            textoContadorBasura.gameObject.SetActive(true);
            textoContadorBasura.color = Color.white; // Reseteamos color por si acaso
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

        if (basuraEnJuego.Contains(objetoRecogido))
        {
            basuraEnJuego.Remove(objetoRecogido);
        }

        int restantes = CalcularBasuraRestante();

        if (restantes <= 0)
        {
            Debug.Log("¡Toda la basura recogida!");
            FinDelJuego(true);
        }
        else
        {
            ActualizarHUD();
        }
    }

    void FinDelJuego(bool victoria)
    {
        juegoActivo = false;

        if (!victoria) BloquearBasuraRestante();

        StartCoroutine(SecuenciaFinPartida(victoria));
    }

    IEnumerator SecuenciaFinPartida(bool victoria)
    {
        // 1. Ocultamos el Timer para que no estorbe (según tu petición)
        if (textoTimer) textoTimer.gameObject.SetActive(false);

        // 2. Usamos el texto del CONTADOR para mostrar el mensaje final
        if (textoContadorBasura)
        {
            textoContadorBasura.gameObject.SetActive(true);

            if (victoria)
            {
                // Mensaje de victoria
                textoContadorBasura.text = "¡FELICIDADES!\nLIMPIEZA COMPLETADA";
                textoContadorBasura.color = Color.green;
            }
            else
            {
                // Mensaje de derrota
                textoContadorBasura.text = "TIEMPO AGOTADO\nNO HAS RECOGIDO TODO";
                textoContadorBasura.color = Color.red;
            }
        }

        // 3. Esperamos 4 segundos para leerlo
        yield return new WaitForSeconds(4f);

        // 4. Cerramos el panel completo
        if (panelTimer != null) panelTimer.SetActive(false);
    }

    void ActualizarHUD()
    {
        // Solo actualizamos si el juego está activo para no machacar el mensaje de victoria
        if (!juegoActivo) return;

        if (textoTimer)
        {
            textoTimer.text = Mathf.FloorToInt(tiempoRestante / 60).ToString("00") + ":" + Mathf.FloorToInt(tiempoRestante % 60).ToString("00");
            textoTimer.color = (tiempoRestante < 10) ? Color.red : Color.white;
        }

        if (textoContadorBasura)
        {
            textoContadorBasura.text = "Basura: " + CalcularBasuraRestante();
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
        foreach (var item in basuraEnJuego)
        {
            // Verificamos null y activeInHierarchy por seguridad
            if (item != null && item.activeInHierarchy) count++;
        }
        return count;
    }

    void BloquearBasuraRestante() { foreach (var item in basuraEnJuego) if (item != null) { Collider c = item.GetComponent<Collider>(); if (c) c.enabled = false; } }
}