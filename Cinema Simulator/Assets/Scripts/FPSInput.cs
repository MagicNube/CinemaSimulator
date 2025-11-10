using UnityEngine;
[RequireComponent(typeof(CharacterController))] // obliga a que el GameObject tenga cierto componente
public class FPSInput : MonoBehaviour
{
    public float speed = 6.0f;
    public float gravity = -9.8f;
    private CharacterController _charController;
    void Start()
    {
        _charController = GetComponent<CharacterController>();
    }
    void Update()
    {
        float deltaX = Input.GetAxis("Horizontal"); // Las teclas asociadas están en:
        float deltaZ = Input.GetAxis("Vertical"); // Edit\Project Settings\Input
        Vector3 movement = new Vector3(deltaX, 0, deltaZ);
        movement = Vector3.ClampMagnitude(movement, 1.0f) * speed;
        movement.y = gravity;
        movement = transform.TransformDirection(movement); // convierte desde el sistema local al global
        _charController.Move(movement * Time.deltaTime); // no movemos el transform para que se calculen
    } // las colisiones
}