using UnityEngine;

public class CameraTargetKeyboardInputMove : MonoBehaviour
{
    [SerializeField] float speed = 5f;

    void Update()
    {
        Transform camera = Camera.main.transform;

        Vector3 move = Vector3.zero;

        if (Input.GetKey(KeyCode.W))
            move += camera.up;

        if (Input.GetKey(KeyCode.S))
            move -= camera.up;

        if (Input.GetKey(KeyCode.A))
            move -= camera.right;

        if (Input.GetKey(KeyCode.D))
            move += camera.right;

        if (move.sqrMagnitude > 1f)
            move.Normalize();

        transform.position += move * speed * Time.deltaTime;
    }
}