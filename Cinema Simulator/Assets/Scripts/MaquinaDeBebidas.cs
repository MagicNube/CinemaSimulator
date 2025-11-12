// MaquinaDeBebidas.cs
using UnityEngine;

// Asegúrate de que tiene los componentes necesarios
[RequireComponent(typeof(Outline))] // O como se llame tu script de outline
[RequireComponent(typeof(Collider))]
public class MaquinaDeBebidas : MonoBehaviour
{
    // Esta función será llamada por el jugador cuando haga clic
    public void Interactuar(ControladorInteraccion jugador)
    {
        // 1. Ver qué item tiene el jugador
        GameObject itemSujetado = jugador.itemActual;

        // 2. Si no tiene nada, no hacemos nada
        if (itemSujetado == null)
        {
            Debug.Log("¡Necesitas un vaso vacío primero!");
            return;
        }

        // 3. Obtener el "DNI" del item
        ItemData data = itemSujetado.GetComponent<ItemData>();

        if (data == null)
        {
            Debug.Log("Eso no es un item válido.");
            return;
        }

        // 4. ¡LA LÓGICA CLAVE!
        // ¿Es un VasoVacio Y tiene un "Prefab Lleno" asignado?
        if (data.tipoDeItem == ItemData.TipoDeItem.VasoVacio && data.prefabItemLleno != null)
        {
            // ¡Sí! Le decimos al jugador que intercambie el item
            // por el prefab "lleno" que tenía guardado
            jugador.AsignarItem(data.prefabItemLleno);
        }
        else if (data.tipoDeItem == ItemData.TipoDeItem.Bebida)
        {
            Debug.Log("¡Tu vaso ya está lleno!");
        }
        else
        {
            Debug.Log("¡Eso no se puede rellenar de bebida!");
        }
    }
}