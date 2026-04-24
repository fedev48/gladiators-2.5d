using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class GateController : NetworkBehaviour
{
    [SerializeField] float gateOpenDistance = 5f;
    [SerializeField] List<ParticleSystem> gateParticles = new List<ParticleSystem>();

    Vector3 closedPos;

    public override void OnNetworkSpawn()
    {
        GameManager.Instance.OnGameStart += GameManager_OnGameStart;
        GameManager.Instance.OnRoundStart += GameManager_OnRoundStart;
        closedPos = transform.position;
    }

    private void GameManager_OnRoundStart(int obj)
    {
        StartCoroutine(MoveGateCoroutine(false));
    }

    private void GameManager_OnGameStart()
    {
        StartCoroutine(MoveGateCoroutine(true));
    }

    private IEnumerator MoveGateCoroutine(bool open)
    {
        StartGateParticlesRpc();

        Vector3 startPos = transform.position;
        Vector3 endPos = open ? closedPos + Vector3.up * gateOpenDistance : closedPos;
        float duration = 2f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            transform.position = Vector3.Lerp(startPos, endPos, elapsed / duration);
            yield return null;
        }

        transform.position = endPos;
        StoptGateParticlesRpc();
    }

    [Rpc(SendTo.NotServer, Delivery = RpcDelivery.Unreliable)]
    public void StartGateParticlesRpc()
    {
        foreach( ParticleSystem gateParticle in gateParticles)
        {
            
            gateParticle.Play();
        }
        
    }

    [Rpc(SendTo.NotServer)]
    public void StoptGateParticlesRpc()
    {
        foreach( ParticleSystem gateParticle in gateParticles)
        {
           
            gateParticle.Stop();
        }
        
    }

}

