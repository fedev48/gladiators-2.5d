using UnityEngine;

public class StartZone : MonoBehaviour
{
    private void OnTriggerExit(Collider other)
    {
        Debug.Log("algoSalioLLamado2");
        GameManager.Instance.PlayerExitedStartZone(other);
    }
}
