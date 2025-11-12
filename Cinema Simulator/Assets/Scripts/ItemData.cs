// ItemData.cs
using UnityEngine;

public class ItemData : MonoBehaviour
{
    // Usamos un 'enum' para definir los tipos de item
    public enum TipoDeItem
    {
        Bebida,
        Palomitas,
        Perrito,
        CuboVacio // Un tipo nuevo para los cubos vacíos
    }

    public TipoDeItem tipoDeItem;

    // Nivel 1 = Pequeño, 2 = Mediano, 3 = Grande
    public int nivel = 1;

    // ¡ESTA ES LA CLAVE DE TODO EL SISTEMA!
    // Si este es un item "vacío" (ej. CuboVacio),
    // arrastra aquí el prefab "lleno" que le corresponde.
    public GameObject prefabItemLleno;
}
