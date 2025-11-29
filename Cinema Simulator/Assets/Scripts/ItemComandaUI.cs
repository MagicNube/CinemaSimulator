using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemComandaUI : MonoBehaviour
{
    public Image icono;
    public TMP_Text texto;

    public Sprite cruzSprite;
    public Sprite tickSprite;

    private bool entregado = false;

    public void Configurar(string nombre)
    {
        texto.text = nombre;
        SetEstado(false);
    }

    public void SetEstado(bool estaEntregado)
    {
        entregado = estaEntregado;
        if (estaEntregado)
            icono.sprite = tickSprite;
        else
            icono.sprite = cruzSprite;
    }
}
