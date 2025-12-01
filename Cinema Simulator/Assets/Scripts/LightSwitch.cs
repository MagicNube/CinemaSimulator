using UnityEngine;

public class LightSwitch : MonoBehaviour
{
    // La luz que este interruptor controlará.
    // Debes arrastrar el objeto Light de la escena a este campo en el Inspector.
    [Tooltip("Arrastra el objeto Light que debe controlar este interruptor.")]
    public Light targetLight;

    // Estado actual de la luz.
    private bool isLightOn = true;

    void Start()
    {
        // Verificar si la luz objetivo está asignada.
        if (targetLight == null)
        {
            Debug.LogError("ERROR: El interruptor '" + gameObject.name + "' no tiene asignada una luz (targetLight).", this);
        }

        // Asegurar que el estado inicial de la variable coincida con la luz.
        if (targetLight != null)
        {
            isLightOn = targetLight.enabled;
        }
    }

    // Este método público se llamará desde tu script ControladorInteraccion
    public void Interact()
    {
        // 1. Alternar el estado
        isLightOn = !isLightOn;

        // 2. Aplicar el nuevo estado a la luz.
        if (targetLight != null)
        {
            targetLight.enabled = isLightOn;
        }

        // Opcional: Puedes añadir aquí código para cambiar el modelo/material del interruptor
        // para indicar visualmente si está encendido o apagado.

        Debug.Log("Interruptor pulsado. Luz ahora: " + (isLightOn ? "ENCENDIDA" : "APAGADA"));
    }
}
