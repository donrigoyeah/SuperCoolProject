using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionHandler : MonoBehaviour
{
    [SerializeField] private Light explosionLight;
    [SerializeField] private float LightDimmDuration = 1f;
    [SerializeField] private float LightIntensity = 10f;



    private void Awake()
    {
        if(explosionLight == null) {explosionLight = GetComponentInChildren<Light>();}
    }

    private void Start()
    {

        StartCoroutine(DimmExplosionLight());
    }


    IEnumerator DimmExplosionLight()
    {
        explosionLight.intensity = LightIntensity;


        float timer = 0f;
        while(timer < LightDimmDuration)
        {
            timer += Time.deltaTime;
            explosionLight.intensity = Mathf.Lerp(LightIntensity, 0f, timer / LightDimmDuration);

            yield return null;
        }

        explosionLight.intensity = 0f; // Ensure light is completely dimmed at the end

    }
}
