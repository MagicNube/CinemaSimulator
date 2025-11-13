using UnityEngine;

public class ItemData : MonoBehaviour
{
    public enum TipoDeItem
    {
        Bebida,
        Palomitas,
        Perrito,
        CuboVacio,
        VasoVacio,
        Ticket
    }

    public TipoDeItem tipoDeItem;

    [HideInInspector]
    public Vector3 escalaOriginal;

    // Nivel 1 = Peque�o, 2 = Mediano, 3 = Grande
    public int nivel = 1;

    public GameObject prefabItemLleno;

    void Awake()
    {
        escalaOriginal = transform.localScale;
    }
}
