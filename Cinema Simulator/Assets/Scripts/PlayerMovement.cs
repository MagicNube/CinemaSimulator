using UnityEngine;

// Estas líneas se aseguran de que el GameObject tenga los componentes necesarios
[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Animator))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Referencias")]
    public Transform playerCamera; // Arrastra tu Main Camera aquí
    private CharacterController controller;
    private Animator animator;

    [Header("Movimiento")]
    public float moveSpeed = 4f;
    public float sprintSpeed = 10f;
    public float gravity = -19.62f; // Doble de la gravedad normal para un feeling más "arcade"

    [Header("Vista (Ratón)")]
    public float mouseSensitivity = 2f;
    private float cameraVerticalAngle = 0f; // Ángulo de la cámara (para mirar arriba/abajo)

    // Variables internas
    private Vector3 velocity; // Velocidad para la gravedad

    void Start()
    {
        // Obtener los componentes
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();

        // Esconder y bloquear el cursor en el centro de la pantalla
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // --- 1. GESTIÓN DE LA VISTA (RATÓN) ---

        // Obtener la entrada del ratón
        float mouseX = Input.GetAxisRaw("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxisRaw("Mouse Y") * mouseSensitivity;

        // Rotar el CUERPO del jugador (eje Y) de izquierda a derecha
        transform.Rotate(Vector3.up * mouseX);

        // Rotar la CÁMARA (eje X) de arriba a abajo
        cameraVerticalAngle -= mouseY;

        // Limitar la rotación vertical (para no "romperse el cuello")
        cameraVerticalAngle = Mathf.Clamp(cameraVerticalAngle, -60f, 60f);

        // Aplicar la rotación vertical solo a la cámara
        playerCamera.localRotation = Quaternion.Euler(cameraVerticalAngle, 0f, 0f);


        // --- 2. GESTIÓN DEL MOVIMIENTO (TECLADO) ---

        // Obtener la entrada del teclado
        bool isSprinting = Input.GetKey(KeyCode.LeftShift);
        float currentSpeed = isSprinting ? sprintSpeed : moveSpeed;

        float horizontal = Input.GetAxis("Horizontal"); // A/D
        float vertical = Input.GetAxis("Vertical");     // W/S

        // Crear el vector de entrada (sin normalizar inmediatamente)
        Vector3 rawInput = new Vector3(horizontal, 0f, vertical);

        // Deadzone para evitar movimientos residuales por el suavizado del Input.GetAxis
        const float deadZone = 0.1f;
        if (rawInput.sqrMagnitude < deadZone * deadZone)
        {
            rawInput = Vector3.zero;
        }
        else if (rawInput.sqrMagnitude > 1f)
        {
            // Normalizar solo si la magnitud supera 1 (p. ej. joystick en diagonal)
            rawInput.Normalize();
        }

        // Convertir a dirección global y mover
        Vector3 moveDirection = transform.TransformDirection(rawInput);
        controller.Move(moveDirection * currentSpeed * Time.deltaTime);


        // --- 3. GESTIÓN DE LA GRAVEDAD ---

        // Si estamos tocando el suelo, reseteamos la velocidad vertical
        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; // Un pequeño valor para mantenerlo pegado al suelo
        }

        // Aplicar gravedad constantemente
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);


        // --- 4. GESTIÓN DEL ANIMATOR ---

        /*
        // TODO: Actualizar el parámetro "Speed" del Animator.
        // Usamos inputDirection.magnitude, que será 0 (quieto) o 1 (moviéndose).
        if (inputDirection.magnitude > 0)
        {
        // Si nos estamos moviendo Y esprintando -> 2
        // Si nos estamos moviendo Y NO esprintando -> 1
        animationSpeedValue = isSprinting ? 1.0f : 1.0f;
        }

    // *** LÍNEA MODIFICADA ***
        animator.SetFloat("Speed", animationSpeedValue);
    */
    }
}
