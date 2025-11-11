// Pon este script en tu Cámara principal o en el objeto Jugador
using UnityEngine;

public class InteracionMaquinaBebidas : MonoBehaviour
{
    // ---- VARIABLES DE INTERACCIÓN ----
    public float distanciaInteraccion = 3f; // Distancia máxima para interactuar
    public Camera camaraJugador; // Arrastra tu cámara aquí en el Inspector

    // ---- VARIABLES DEL VASO ----
    public GameObject vasoPrefab; // Arrastra tu Prefab del vaso aquí
    public Transform puntoDeAgarre; // Arrastra tu objeto vacío "PuntoDeAgarre" aquí
    private GameObject vasoActual; // Para guardar el vaso que ya tenemos

    // ---- VARIABLES DE RESALTADO ----
    private Transform objetoMirado; // El objeto que estamos mirando ahora

    void Start()
    {
        // Si no asignas la cámara, la busca automáticamente
        if (camaraJugador == null)
        {
            camaraJugador = Camera.main;
        }
    }

    void Update()
    {
        // Creamos un rayo desde la cámara hacia adelante
        Ray ray = new Ray(camaraJugador.transform.position, camaraJugador.transform.forward);
        RaycastHit hit; // Variable para guardar la información del golpe

        Transform seleccionActual = null;

        // 1. LÓGICA DE DETECCIÓN (PARA RESALTAR)
        if (Physics.Raycast(ray, out hit, distanciaInteraccion))
        {
            // Si golpeamos un objeto con el tag "MaquinaBebidas"
            if (hit.collider.CompareTag("MaquinaBebidas"))
            {
                seleccionActual = hit.transform;
            }
        }

        // Comprobamos si hemos cambiado de objeto mirado
        if (seleccionActual != objetoMirado)
        {
            // Dejamos de mirar el objeto anterior
            if (objetoMirado != null)
            {
                // Aquí va tu código para DESACTIVAR el resaltado
                // Ejemplo: objetoMirado.GetComponent<Outline>()?.enabled = false;
                Debug.Log("Dejando de mirar " + objetoMirado.name);
            }

            // Empezamos a mirar el objeto nuevo
            if (seleccionActual != null)
            {
                // Aquí va tu código para ACTIVAR el resaltado
                // Ejemplo: seleccionActual.GetComponent<Outline>()?.enabled = true;
                Debug.Log("Mirando " + seleccionActual.name);
            }

            // Actualizamos el objeto que estamos mirando
            objetoMirado = seleccionActual;
        }


        // 2. LÓGICA DE INTERACCIÓN (AL HACER CLIC)
        if (Input.GetMouseButtonDown(0)) // Si el jugador hace clic izquierdo
        {
            // Y si estamos mirando un objeto interactuable (la máquina)
            if (objetoMirado != null)
            {
                CogerBebida();
            }
        }
    }

    void CogerBebida()
    {
        // Si ya tenemos un vaso, no hacemos nada.
        if (vasoActual != null)
        {
            Debug.Log("Ya tienes un vaso.");
            return;
        }

        // 1. Instanciamos (creamos) el vaso desde el Prefab
        vasoActual = Instantiate(vasoPrefab, puntoDeAgarre.position, puntoDeAgarre.rotation);

        // 2. Lo hacemos hijo del "PuntoDeAgarre"
        // Esto es CLAVE: hace que el vaso se mueva con tu mano/cámara
        vasoActual.transform.parent = puntoDeAgarre;

        Debug.Log("¡Has cogido un vaso!");
    }
}
