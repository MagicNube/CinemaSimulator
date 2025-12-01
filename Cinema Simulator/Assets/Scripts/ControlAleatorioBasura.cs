using System.Collections.Generic;
using UnityEngine;

public class ControlAleatorioBasura : MonoBehaviour
{
    [Header("Configuración")]
    [Tooltip("Porcentaje de objetos que se quedarán visibles (0 a 100)")]
    [Range(0, 100)]
    public float porcentajeVisibilidad = 50f;

    [Header("Lista de Objetos")]
    [Tooltip("Usa el botón derecho en el título del script -> Cargar Solo Prefabs")]
    public List<GameObject> objetosBasura;

    void Start()
    {
        AplicarAleatoriedad();
    }

    void AplicarAleatoriedad()
    {
        foreach (GameObject obj in objetosBasura)
        {
            if (obj != null)
            {
                float randomVal = Random.Range(0f, 100f);
                bool debeSerVisible = randomVal < porcentajeVisibilidad;
                obj.SetActive(debeSerVisible);
            }
        }
    }

    [ContextMenu("Cargar Solo Prefabs (Estructura de Filas)")]
    void CargarSoloPrefabs()
    {
        objetosBasura = new List<GameObject>();

        foreach (Transform contenedorFila in this.transform)
        {
            if (contenedorFila.childCount == 0)
            {
                objetosBasura.Add(contenedorFila.gameObject);
                continue;
            }
            foreach (Transform prefabBasura in contenedorFila)
            {
                objetosBasura.Add(prefabBasura.gameObject);
            }
        }
        Debug.Log($"Se han cargado {objetosBasura.Count} objetos principales.");
    }
}