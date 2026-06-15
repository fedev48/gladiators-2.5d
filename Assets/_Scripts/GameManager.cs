using System;
using System.Collections;
using NUnit.Framework.Constraints;
using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance {private set; get;}

    [SerializeField] GameObject playerPrefab;
    [SerializeField] bool isDebuuging;

    [Header("Gate Configuration")]
    [SerializeField] int regularRoundDuration = 60;
    [SerializeField] Transform gateTransform;

    int playersOutside;
    Vector3 closedPos;

    public NetworkVariable<GameState> gameAuthoritativeState = new  NetworkVariable<GameState>( 
        GameState.None, 
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    public NetworkVariable<int> roundTimer = new  NetworkVariable<int>( 
        0, 
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );



    public event Action OnGameStart;
    public event Action<int> OnRoundStart;

    void Awake()
    {
        if(Instance!=null&&Instance!=this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    void Start()
    {
        if (isDebuuging)
        {
            NetworkManager.Singleton.StartHost(); 
            StartGameServer();
        }
    }
    public override void OnNetworkSpawn()
    {
        if (IsServer)
        { 
            gameAuthoritativeState.Value = GameState.WaitingForClientSpawn;
            NetworkSessionManager.Instance.OnAllClientsSceneLoaded += NetworkSessionManager_OnAllClientsSceneLoaded;
            
        }

        if (!IsClient) return;
        RequestSpawnPlayerRpc();
        
    }

    public override void OnNetworkDespawn()
    {
        NetworkSessionManager.Instance.OnAllClientsSceneLoaded -= NetworkSessionManager_OnAllClientsSceneLoaded;
    }


    private void NetworkSessionManager_OnAllClientsSceneLoaded(string obj)
    {
        if(!IsServer) return;

        StartGameServer();
    }

    private void StartGameServer()
    {
        gameAuthoritativeState.Value = GameState.WaitingWithOpenGate;
        OnGameStart?.Invoke();


        // StartCoroutine(RoundTimerCoroutine(regularRoundDuration));

    }

    public void PlayerExitedStartZone(Collider other)
    {
        
        if (!IsServer) return;
        if (!other.TryGetComponent(out NetworkObject netObj)) return;
        
        playersOutside++;

        if (playersOutside >= NetworkManager.Singleton.ConnectedClients.Count)
        {
            StartCoroutine(RoundTimerCoroutine(regularRoundDuration));
            gameAuthoritativeState.Value = GameState.InRound;
           
        }
    }



    [Rpc(SendTo.Server)]
    private void RequestSpawnPlayerRpc(RpcParams rpcParams = default)
    {
       
        ulong senderClientId = rpcParams.Receive.SenderClientId;
        Debug.Log("Sender Id: " + senderClientId);

        GameObject player = Instantiate(playerPrefab,Vector3.zero,Quaternion.identity);
        player.GetComponent<NetworkObject>().SpawnAsPlayerObject(senderClientId);
    }

    private IEnumerator RoundTimerCoroutine(int duration)
    {
        OnRoundStart?.Invoke(1);
        roundTimer.Value = duration;
        
        while (roundTimer.Value > 0)
        {
            yield return new WaitForSeconds(1f);
            roundTimer.Value--;
        }
        
        // cambiar estado
    }
    
}

public enum GameState
{
    None,
    WaitingForClientSpawn,
    WaitingWithOpenGate,
    InRound,
    EndGame

}
