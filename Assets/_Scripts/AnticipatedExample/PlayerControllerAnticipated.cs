using System;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerControllerAnticipated : NetworkBehaviour
{

    [SerializeField] float moveSpeed = 5f;
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private float halfHeight;

    [SerializeField] private AnticipatedNetworkTransform anticipatedNetworkTransform;

    private readonly List<TimedMove> timedMoves = new();



    private InputSystem_Actions inputActions;
    private Camera cam;
   
    void Awake()
    {
        inputActions = new InputSystem_Actions();
        if (cam == null) cam = Camera.main;
    }

   
    public override void OnNetworkSpawn() 
    {
        anticipatedNetworkTransform.PositionThreshold = 0; //solo ejemplo

        if (!IsOwner) return;

        inputActions.Player.Enable();
        anticipatedNetworkTransform.StaleDataHandling = StaleDataHandling.Reanticipate;

        inputActions.Player.Interact.performed += InputActions_Interact;
    }

    private void InputActions_Interact(InputAction.CallbackContext context)
    {
        Debug.Log("E Pressed");

        foreach (Collider collider in Physics.OverlapSphere(transform.position, 2f))
        {
            if (collider.TryGetComponent(out ColorChanger changer))
            {
                changer.Toogle();
                break;
            }
        }
    }

    public override void OnReanticipate(double lastRoundTripTime)
    {
        var previousAnticipatedState = anticipatedNetworkTransform.PreviousAnticipatedState;
        var authoritativeState = anticipatedNetworkTransform.AuthoritativeState;
        double authorityTime = NetworkManager.LocalTime.Time - lastRoundTripTime;


        Vector3 replayPos = authoritativeState.Position;

        foreach (TimedMove item in timedMoves)
        {
            if (authorityTime>item.time) continue;
            replayPos += item.delta;
        }

        anticipatedNetworkTransform.AnticipateMove(replayPos);

        RemoveBefore(authorityTime);

        float dist = Vector3.SqrMagnitude(previousAnticipatedState.Position - replayPos);

        if (dist<0.25f*0.25f)
        {
            anticipatedNetworkTransform.AnticipateMove(previousAnticipatedState.Position);
        }
        else if(dist<3f*3f)
        {
            anticipatedNetworkTransform.Smooth(previousAnticipatedState, anticipatedNetworkTransform.AnticipatedState, 0.5f);
        }
    }


    void FixedUpdate()
    {
        if (!IsOwner) return;

        Vector2 input = inputActions.Player.Move.ReadValue<Vector2>();
        Vector3 worldMove = GetCameraRelativeMove(input)* moveSpeed * Time.fixedDeltaTime;

        ApplyMove(worldMove);

        if (IsServer) return;
        timedMoves.Add(new TimedMove
        {
            time = NetworkManager.LocalTime.Time,
            delta = worldMove
        });
        // RemoveBefore(NetworkManager.LocalTime.Time);

        MoveOnServerRpc(worldMove);
        
    }





    private void ApplyMove(Vector3 delta)
    {
        Vector3 newPos = transform.position + delta;

        if (Physics.Raycast(newPos + Vector3.up, Vector3.down,out RaycastHit hit, 10f, groundMask))
        {
            newPos.y = hit.point.y + halfHeight;
        }

        anticipatedNetworkTransform.AnticipateMove(newPos);
    }

    [Rpc(SendTo.Server)]
    private void MoveOnServerRpc(Vector3 input) => ApplyMove(input);

    private void RemoveBefore(double time) => timedMoves.RemoveAll(x => time > x.time);

    Vector3 GetCameraRelativeMove(Vector2 input)
    {
        Vector3 forward = Vector3.ProjectOnPlane(cam.transform.forward, Vector3.up).normalized;
        Vector3 right = Vector3.ProjectOnPlane(cam.transform.right, Vector3.up).normalized;
        return forward * input.y + right * input.x;
    }
}

public struct TimedMove
{
    public double time;
    public Vector3 delta;
}
