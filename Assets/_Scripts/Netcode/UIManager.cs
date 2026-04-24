using System;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] Button startClientButton;
    [SerializeField] Button startServerButton;


    public event Action OnStartClientButton;
    private NetworkManager NetworkManager => NetworkManager.Singleton;

    void Awake()
    {
        startServerButton.onClick.AddListener(()=>
        {
            NetworkManager.StartServer();
        });

        startClientButton.onClick.AddListener(()=>
        {
           

            NetworkManager.StartClient();
        });
    }
}
