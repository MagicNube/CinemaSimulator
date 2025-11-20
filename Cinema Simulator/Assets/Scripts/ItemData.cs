using UnityEngine;


[System.Serializable]
public class Pedido
{
    public string textoDelPedido;
    public ItemData.TipoDeItem itemRequerido;
}


public class ItemData : MonoBehaviour
{
    public enum TipoDeItem
    {
        Bebida,
        Palomitas,
        Perrito,
        CuboVacio,
        VasoVacio,
        Ticket,
        CajaPalomitas,
        CajaBebidas,
        CajaEnvasesPalomitas,
        CajaEnvasesBebidas,
        CajaPerritos
    }

    public TipoDeItem tipoDeItem;
    public int nivel = 1;
    public GameObject prefabItemLleno;

    [HideInInspector]
    public Vector3 escalaOriginal;

    void Awake()
    {
        escalaOriginal = transform.localScale;
    }
}
