using UnityEngine;

public class FireLightVariation : MonoBehaviour
{
    [SerializeField] private float minRange = 1f;
    [SerializeField] private float maxRange = 2f;
    [SerializeField] private float flickerSpeed = 8f;
    [SerializeField] Light pointLight;
    [SerializeField] private float minIntensity = 1;
    [SerializeField] private float maxIntensity= 2;
    private float noiseSeed;
    

    private void Awake()
    {
        if (pointLight == null)
            pointLight = GetComponent<Light>();

        noiseSeed = Random.value * 1000f;
    }

    private void Update()
    {
        float noise = Mathf.PerlinNoise(noiseSeed, Time.time * flickerSpeed);

        pointLight.intensity = Mathf.Lerp(minIntensity, maxIntensity, noise);
        pointLight.range = Mathf.Lerp(minRange, maxRange, noise);
    }
}
