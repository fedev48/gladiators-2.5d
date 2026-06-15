using System;
using Unity.Netcode;
using UnityEngine;

public class ColorChanger : NetworkBehaviour
{

    [SerializeField] private AnticipatedNetworkVariable<bool> isActive = new (false, StaleDataHandling.Ignore);
    [SerializeField] private Renderer render;

    public override void OnNetworkSpawn()
    {
        isActive.OnAuthoritativeValueChanged += IsActive_OnAuthoritativeValueChanged;
        ApplyColor (isActive.Value);
    }

    private void IsActive_OnAuthoritativeValueChanged(AnticipatedNetworkVariable<bool> variable, in bool previousValue, in bool newValue)
    {
        ApplyColor(newValue);
    }

    public void Toogle()
    {
        bool next = !isActive.Value;

        isActive.Anticipate(next);
        ApplyColor(next);
        ToggleValueRpc();

    }

    [Rpc (SendTo.Server)]
    private void ToggleValueRpc()
    {
        isActive.AuthoritativeValue = !isActive.AuthoritativeValue;
    }

    private void ApplyColor(bool active) => render.material.color = active ? Color.blue : Color.red;
    

    
}
