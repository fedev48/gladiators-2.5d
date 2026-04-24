using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkSessionManager : MonoBehaviour
{
    public static NetworkSessionManager Instance {get; private set;}
    private NetworkManager NetworkManager => NetworkManager.Singleton;

    public Action<string> OnAllClientsSceneLoaded;

    [SerializeField] int numberOfPlayersNeededToStartRound;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance=this;
    }

    void Start()
    {
        NetworkManager.OnClientConnectedCallback += NetworkManager_OnClientConnected;
        NetworkManager.OnClientDisconnectCallback += NetworkManager_OnClientDisconnected;
        NetworkManager.OnServerStarted += NetworkManager_OnServerStarted;
        NetworkManager.OnServerStopped += NetworkManager_OnServerStopped;
       
    }

    private void SceneManager_OnLoadEventCompleted(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        OnAllClientsSceneLoaded?.Invoke(sceneName);
    }

    private void NetworkManager_OnServerStopped(bool obj)
    {
        
    }

    private void NetworkManager_OnServerStarted()
    {
        NetworkManager.SceneManager.OnLoadEventCompleted += SceneManager_OnLoadEventCompleted;
    }

    private void NetworkManager_OnClientDisconnected(ulong obj)
    {
        
    }

    private void NetworkManager_OnClientConnected(ulong clientId)
    {
        Debug.Log("Client Connected: " +clientId);

        if (clientId == NetworkManager.ServerClientId)
        {
            Debug.Log("Se conecto host");
        }
        else
        {
            Debug.Log("Se conecto cliente");
        }

        if(!NetworkManager.IsServer) return;

        if (NetworkManager.ConnectedClients.Count == numberOfPlayersNeededToStartRound)
        {
            NetworkManager.SceneManager.LoadScene(SceneNames.GAME_SCENE, LoadSceneMode.Single);
        }

    }
}
