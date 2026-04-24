using UnityEngine;

public class FreeCameraRig : MonoBehaviour
{
    [SerializeField] private float screenRelativeSpeed = 5f;
    [SerializeField] private Transform cameraReference;
    [SerializeField] private float groundY = 0f;

    private void Start()
    {
        Vector3 pos = transform.position;
        pos.y = groundY;
        transform.position = pos;
    }

    private void Update()
    {
        if (cameraReference == null)
            return;

        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        Vector2 input = new Vector2(horizontal, vertical);

        if (input.sqrMagnitude > 1f)
            input.Normalize();

        if (input.sqrMagnitude < 0.0001f)
        {
            ClampToGround();
            return;
        }


        Vector3 groundRight = Vector3.ProjectOnPlane(cameraReference.right, Vector3.up).normalized;
        Vector3 groundForward = Vector3.ProjectOnPlane(cameraReference.forward, Vector3.up).normalized;


        groundRight.y = 0f;
        groundForward.y = 0f;
        groundRight.Normalize();
        groundForward.Normalize();


        Vector2 rightOnScreen = new Vector2(
            Vector3.Dot(groundRight, cameraReference.right),
            Vector3.Dot(groundRight, cameraReference.up)
        );

        Vector2 forwardOnScreen = new Vector2(
            Vector3.Dot(groundForward, cameraReference.right),
            Vector3.Dot(groundForward, cameraReference.up)
        );


        Vector2 desiredScreenMove = input * screenRelativeSpeed * Time.deltaTime;

        float det = rightOnScreen.x * forwardOnScreen.y - rightOnScreen.y * forwardOnScreen.x;

        if (Mathf.Abs(det) < 0.0001f)
        {
            ClampToGround();
            return;
        }

        float a = (desiredScreenMove.x * forwardOnScreen.y - desiredScreenMove.y * forwardOnScreen.x) / det;
        float b = (rightOnScreen.x * desiredScreenMove.y - rightOnScreen.y * desiredScreenMove.x) / det;

        Vector3 worldMove = groundRight * a + groundForward * b;
        worldMove.y = 0f;

        transform.position += worldMove;
        ClampToGround();
    }

    private void ClampToGround()
    {
        Vector3 pos = transform.position;
        pos.y = groundY;
        transform.position = pos;
    }
}