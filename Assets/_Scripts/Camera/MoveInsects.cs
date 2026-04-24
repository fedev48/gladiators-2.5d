using UnityEngine;

public class MoveInsects : MonoBehaviour
{
    [SerializeField] ParticleSystem insects;
    [SerializeField] float yPosOffset;

    
    void Update()
    {   Ray ray = new Ray(transform.position, transform.forward);


        if (Physics.Raycast(ray, out RaycastHit hitInfo, Mathf.Infinity, LayerMask.GetMask("Ground")))
        {
           
            insects.transform.position = new Vector3 (hitInfo.point.x, hitInfo.point.y+yPosOffset ,hitInfo.point.z);
            
        }
    }
}
