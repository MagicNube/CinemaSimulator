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
        CuboVacio,
        VasoVacio
    }

    public TipoDeItem tipoDeItem;

    // Nivel 1 = Peque�o, 2 = Mediano, 3 = Grande
    public int nivel = 1;

    // �ESTA ES LA CLAVE DE TODO EL SISTEMA!
    // Si este es un item "vac�o" (ej. CuboVacio),
    // arrastra aqu� el prefab "lleno" que le corresponde.
    public GameObject prefabItemLleno;
}
