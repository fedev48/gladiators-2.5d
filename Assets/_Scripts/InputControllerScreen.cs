using UnityEngine;
using UnityEngine.InputSystem;

public class InputControllerScreen : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;

    private InputSystem_Actions inputActions;
    private CharacterController controller;

    void Awake()
    {
        inputActions = new InputSystem_Actions();
        controller = GetComponent<CharacterController>();
    }

    void OnEnable()  => inputActions.Player.Enable();
    void OnDisable() => inputActions.Player.Disable();

    void Update()
    {
        Vector2 input = inputActions.Player.Move.ReadValue<Vector2>();
        Vector3 move = new Vector3(input.x, 0f, input.y) * moveSpeed * Time.deltaTime;
        controller.Move(move);
    }
}
