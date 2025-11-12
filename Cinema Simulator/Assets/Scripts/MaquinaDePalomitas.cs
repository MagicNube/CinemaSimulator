// MaquinaDePalomitas.cs
using UnityEngine;

public class MaquinaDePalomitas : MonoBehaviour
{
    // Esta función será llamada por el jugador cuando haga clic
    public void Interactuar(ControladorInteraccion jugador)
    {
        // 1. Ver qué item tiene el jugador
        GameObject itemSujetado = jugador.itemActual;

        // 2. Si no tiene nada, no hacemos nada
        if (itemSujetado == null)
        {
            Debug.Log("¡Necesitas un cubo vacío primero!");
            return;
        }

        // 3. Obtener el "DNI" del item
        ItemData data = itemSujetado.GetComponent<ItemData>();

        // 4. Validar el item
        if (data == null)
        {
            Debug.Log("Eso no es un item válido.");
            return;
        }

        // 5. ¡LA LÓGICA CLAVE!
        // ¿Es un CuboVacio Y tiene un "Prefab Lleno" asignado?
        if (data.tipoDeItem == ItemData.TipoDeItem.CuboVacio && data.prefabItemLleno != null)
        {
            // ¡Sí! Le decimos al jugador que intercambie el item
            // por el prefab "lleno" que tenía guardado
            jugador.AsignarItem(data.prefabItemLleno);
        }
        else if (data.tipoDeItem == ItemData.TipoDeItem.Palomitas)
        {
            Debug.Log("¡Tu cubo ya está lleno!");
        }
        else
        {
            Debug.Log("¡Eso no se puede rellenar de palomitas!");
        }
    }
}