using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine.AI;
using System.Collections;

public class PlayerController : Unit
{
    private delegate void PerformRightClickActionDelegate(Vector3 target);
    private PerformRightClickActionDelegate performRightClickActionDelegate;
    private bool isActing;
    public event Action OnAttack;
    public event Action OnSummon;
    public event Action OnActionEnds;


    [Header("Attacks")]
    public List<IPlayerSpell> playerSpells = new List<IPlayerSpell>();
    public static IPlayerSpell currentSpell;
    public SpellType spellType;
    private const float attackAnimationTime = 1.5f;
    private const float summonAnimationTime = 1.5f;

    

    [Header("Isometric compensation")]
    [SerializeField] private float depthMultiplier = 1.5f;
    private NetworkVariable<Vector3> camForward = new(
        Vector3.forward,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner
    );

    [Header("anticipation")]
    private List<TimedInput> timedAgentSteps = new List<TimedInput>();
    private List<Vector3> pendingDestinations = new List<Vector3>();
    [SerializeField] private AnticipatedNetworkTransform anticipatedNetworkTransform;

    [Header("network destination sync")]
    private NetworkVariable<Vector3> syncedDestination = new(
        default,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private enum ActionType
    {
        Attack,
        Summon
    }
    [SerializeField] private float destinationSyncTolerance = 0.01f;
    
    void Update()
    {
        if (isActing) return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            StartCoroutine(ActionCorroutine(attackAnimationTime, ActionType.Attack));
            AttackOnServerRpc(transform.position);
            OnAttack?.Invoke();

        }
        if (Input.GetKeyDown(KeyCode.Q))
        {
            StartCoroutine(ActionCorroutine(attackAnimationTime, ActionType.Summon));
            SummonOnServerRpc(transform.position);
            OnSummon?.Invoke();
        }
    }

    void FixedUpdate()
    {

        if (IsServer)return;
        if (!IsOwner)return;
        if (isActing)return;

        Move(false, false);
        MovePlayerOnServerRpc();
    }

    void LateUpdate()
    {
        if (IsOwner) return;
        if (IsServer) return;

        agent.nextPosition = transform.position;
    }


    public override void OnNetworkSpawn()
    {
        Debug.Log("player spawned");

        agent.updatePosition = false;
        agent.updateRotation = false;
       

        if (!IsServer && Camera.main != null)
            camForward.Value = Vector3.ProjectOnPlane(Camera.main.transform.forward, Vector3.up).normalized;

        if (IsOwner && !IsServer)
        {
            anticipatedNetworkTransform.StaleDataHandling = StaleDataHandling.Reanticipate;
        }
        else
        {
            anticipatedNetworkTransform.StaleDataHandling = StaleDataHandling.Ignore;
        }

        if (IsServer) anticipatedNetworkTransform.PositionThreshold = 0;
        
        syncedDestination.OnValueChanged += OnSyncedDestinationChanged;

        if (!IsOwner) return;

        InputController.Instance.OnMouseRightClickDown += MouseController_OnMouseRightClickDown;
        InputController.Instance.OnMouseRightClickUp += MouseController_OnMouseRightClickUp;
        InputController.Instance.OnNumKeyDown += OnNumKeyDown;

        performRightClickActionDelegate += CalculateLocalPath;

        agent.SetDestination(transform.position);
        pendingDestinations.Add(transform.position);
    }

    public override void OnNetworkDespawn()
    {
        syncedDestination.OnValueChanged -= OnSyncedDestinationChanged;

        InputController.Instance.OnMouseRightClickDown -= MouseController_OnMouseRightClickDown;
        InputController.Instance.OnMouseRightClickUp -= MouseController_OnMouseRightClickUp;
        InputController.Instance.OnNumKeyDown -= OnNumKeyDown;
        base.OnNetworkDespawn();
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
            AddStepToHistory(nextPosition);
        }
       

        if (toNextPosition.sqrMagnitude < 0.001f) return;

        Vector3 flatDir = new Vector3(toNextPosition.x, 0f, toNextPosition.z);
        float flatDist = flatDir.magnitude;

        if (flatDist > 0.0001f)
        {
            float alignment = Mathf.Abs(Vector3.Dot(flatDir / flatDist, camForward.Value));
            float scale = Mathf.Lerp(1f, depthMultiplier, alignment);
            toNextPosition.x *= scale;
            toNextPosition.z *= scale;
        }
        

        Vector3 targetPosition = transform.position + toNextPosition;

        if (!isRollback)agent.nextPosition = targetPosition;

        if (anticipatedNetworkTransform == null) return;

        anticipatedNetworkTransform.AnticipateMove(targetPosition);
    }

    private void AddStepToHistory(Vector3 nextPosition)
    {
        timedAgentSteps.Add(new TimedInput
        {
            time = NetworkManager.Singleton.LocalTime.Time,
            stampedTarget = nextPosition
        });
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

        pendingDestinations.Add(targetPosition);

        agent.SetDestination(targetPosition);
        CalculateServerPathRpc(targetPosition);
    }
    #region ServerRpcs

    [Rpc(SendTo.Server)]
    private void MovePlayerOnServerRpc()
    {
        Move(true, false);
    }

    [Rpc(SendTo.Server)]
    private void CalculateServerPathRpc(Vector3 targetPosition)
    {
        if (!agent.isOnNavMesh) return;

        syncedDestination.Value = targetPosition;
        agent.SetDestination(targetPosition);
    }
    [Rpc(SendTo.Server)]
    private void SummonOnServerRpc(Vector3 position)
    {
        transform.position = position; //This might require revision with hit targets. With high latency there was a discrepancy in positions at attack events, fix works atleast in isolation
        StartCoroutine(ActionCorroutine(summonAnimationTime, ActionType.Summon));
    }

    [Rpc(SendTo.Server)]
    void AttackOnServerRpc(Vector3 position)
    {
        transform.position = position; //This might require revision with hit targets. With high latency there was a discrepancy in positions at attack events, fix works atleast in isolation
        StartCoroutine(ActionCorroutine(attackAnimationTime, ActionType.Attack));
    }
    #endregion
    #region Coroutines

    private IEnumerator ActionCorroutine(float timeAttack, ActionType actionType)
    {
        agent.isStopped = true;
        isActing = true;
        yield return new WaitForSeconds(timeAttack);
        isActing = false;
        agent.nextPosition = transform.position;
        agent.isStopped = false;
        OnActionEnds?.Invoke();
    }

    


    #endregion
    private void OnSyncedDestinationChanged(Vector3 previousValue, Vector3 newValue)
    {
        if (!IsSpawned) return;
        if (!agent.isOnNavMesh) return;

       
        if (IsOwner)
        {
            int matchIndex = pendingDestinations.FindIndex(d => ArePointsClose(d, newValue, destinationSyncTolerance));
            if (matchIndex >= 0)
            {
                pendingDestinations.RemoveAt(matchIndex);
                return;
            }
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

        foreach (TimedInput item in timedAgentSteps)
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
        timedAgentSteps.RemoveAll(x => x.time < time);
    }

    public void Clear()
    {
        timedAgentSteps.Clear();
    }

    public override void PerformRightClickAction(Vector3 targetPosition)
    {
        performRightClickActionDelegate(targetPosition);
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

