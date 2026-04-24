using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine.AI;

public class PlayerController : Unit
{
    private delegate void PerformRightClickActionDelegate(Vector3 target);

    private PerformRightClickActionDelegate performRightClickActionDelegate;
    private Rigidbody rb;

    [Header("spells")]
    public List<IPlayerSpell> playerSpells = new List<IPlayerSpell>();
    public static IPlayerSpell currentSpell;
    public SpellType spellType;

    [Header("anticipation")]
    private List<TimedInput> timedInputs = new List<TimedInput>();
    [SerializeField] private AnticipatedNetworkTransform anticipatedNetworkTransform;

    [Header("network destination sync")]
    private NetworkVariable<Vector3> syncedDestination = new(
        default,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    [SerializeField] private float destinationSyncTolerance = 0.01f;
    private Vector3 lastPredictedDestination;

    public override void OnNetworkSpawn()
    {
        Debug.Log("player spawned");

        agent.updatePosition = false;
        agent.updateRotation = false;

        if (IsOwner && !IsServer)
        {
            anticipatedNetworkTransform.StaleDataHandling = StaleDataHandling.Reanticipate;
        }
        else
        {
            anticipatedNetworkTransform.StaleDataHandling = StaleDataHandling.Ignore;
        }

        if (IsServer) anticipatedNetworkTransform.PositionThreshold = 0;//this is to force the nt to update the position every tick, even if there's no delta in the positions
        
        syncedDestination.OnValueChanged += OnSyncedDestinationChanged;

        if (!IsOwner) return;

        InputController.Instance.OnMouseRightClickDown += MouseController_OnMouseRightClickDown;
        InputController.Instance.OnMouseRightClickUp += MouseController_OnMouseRightClickUp;
        InputController.Instance.OnNumKeyDown += OnNumKeyDown;

        performRightClickActionDelegate += CalculateLocalPath;

        agent.SetDestination(transform.position);
        lastPredictedDestination = transform.position;
    }

    public override void OnNetworkDespawn()
    {
        syncedDestination.OnValueChanged -= OnSyncedDestinationChanged;
        base.OnNetworkDespawn();
    }

    void FixedUpdate()
    {

        if (IsServer)
        { 
            if (ArePointsClose(syncedDestination.Value, agent.destination, 0.01f)) syncedDestination.Value = agent.destination;

            return;
        }

        if (!IsOwner) return;

        Move(false, false);
        MovePlayerOnServerRpc();
    }

    private void Move(bool isServer, bool isRollback, Vector3 nextPosition = default)
    {
        if (nextPosition == default)
        {
            nextPosition = agent.nextPosition;
        }

        Vector3 toNextPosition = nextPosition - transform.position;

        if (!isServer && !isRollback)
        {
            timedInputs.Add(new TimedInput
            {
                time = NetworkManager.Singleton.LocalTime.Time,
                stampedTarget = nextPosition
            });
        }

        if (toNextPosition.sqrMagnitude < 0.0001f) return;

        float step = agent.speed * Time.fixedDeltaTime;
        Vector3 move = Vector3.ClampMagnitude(toNextPosition, step);

        Vector3 targetPosition = transform.position + move;
        if (anticipatedNetworkTransform == null) return;

        anticipatedNetworkTransform.AnticipateMove(targetPosition);
    }

    [Rpc(SendTo.Server)]
    private void MovePlayerOnServerRpc()
    {
        Move(true, false);
    }

    void OnDisable()
    {
        if (!IsOwner) return;

        InputController.Instance.OnMouseRightClickDown -= MouseController_OnMouseRightClickDown;
        InputController.Instance.OnMouseRightClickUp -= MouseController_OnMouseRightClickUp;
        InputController.Instance.OnNumKeyDown -= OnNumKeyDown;
    }

    private void OnNumKeyDown(object sender, InputController.NumDownInfo e)
    {
        int numKey = e.numKey;

        if (numKey == 1)
        {
            spellType = SpellType.SpellThrow;
            currentSpell = playerSpells[0];
        }

        performRightClickActionDelegate = null;
        performRightClickActionDelegate = currentSpell.LoadAttack;
        InputController.Instance.OnMouseRightClickUp += currentSpell.AttackReleased;

        currentSpell.OnAttackFinished += (s, ev) =>
        {
            performRightClickActionDelegate = CalculateLocalPath;
            InputController.Instance.OnMouseRightClickUp -= currentSpell.AttackReleased;
        };
    }

    private void MouseController_OnMouseRightClickDown(object sender, EventArgs e)
    {
    }

    private void MouseController_OnMouseRightClickUp(object sender, EventArgs e)
    {
    }

    public override void CalculateLocalPath(Vector3 targetPosition)
    {
        if (!agent.isOnNavMesh) return;

        agent.SetDestination(targetPosition);
        lastPredictedDestination = targetPosition;


        CalculateServerPathRpc(targetPosition);

        Debug.Log("Calculate");
    }

    [Rpc(SendTo.Server)]
    private void CalculateServerPathRpc(Vector3 targetPosition)
    {
        if (!agent.isOnNavMesh) return;

        syncedDestination.Value = targetPosition;
        agent.SetDestination(targetPosition);
    }

    private void OnSyncedDestinationChanged(Vector3 previousValue, Vector3 newValue)
    {
        if (!IsSpawned) return;
        if (!agent.isOnNavMesh) return;

       
        if (IsOwner && ArePointsClose(lastPredictedDestination, newValue, destinationSyncTolerance))
        {
            return;
        }

        agent.SetDestination(newValue);
    }

    private bool ArePointsClose(Vector3 a, Vector3 b, float tolerance)
    {
        return Vector3.SqrMagnitude(a - b) <= tolerance;
    }

    public override void OnReanticipate(double lastRoundTripTime)
    {
       
        if (!IsOwner)
        { 
            anticipatedNetworkTransform.Smooth(anticipatedNetworkTransform.PreviousAnticipatedState, anticipatedNetworkTransform.AnticipatedState, 0.5f);
            return;
        }

        var previousState = anticipatedNetworkTransform.PreviousAnticipatedState;
        double authorityTime = NetworkManager.LocalTime.Time - lastRoundTripTime;

        foreach (TimedInput item in timedInputs)
        {
            if (item.time <= authorityTime) continue;
            Move(false, true, item.stampedTarget);
        }

        RemoveBefore(authorityTime);

        float dist = Vector3.SqrMagnitude(previousState.Position - anticipatedNetworkTransform.AnticipatedState.Position);

        if (ArePointsClose (previousState.Position,anticipatedNetworkTransform.AnticipatedState.Position, 0.5f))
        {
            anticipatedNetworkTransform.AnticipateState(previousState);
        }
        else
        {
            anticipatedNetworkTransform.Smooth(previousState, anticipatedNetworkTransform.AnticipatedState, 0.5f);
        }
    }

    public void RemoveBefore(double time)
    {
        timedInputs.RemoveAll(x => x.time < time);
    }

    public void Clear()
    {
        timedInputs.Clear();
    }

    public override void PerformRightClickAction(Vector3 targetPosition)
    {
        performRightClickActionDelegate(targetPosition);
    }

    void LateUpdate()
    {
        if (IsOwner) return;
        if (IsServer) return;

        agent.nextPosition = transform.position;
    }
}

public enum SpellType
{
    None,
    SpellThrow
};

public struct TimedInput
{
    public double time;
    public Vector3 stampedTarget;
}