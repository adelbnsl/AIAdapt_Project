using UnityEngine;

public class FogController : MonoBehaviour
{
    public FearManager fearManager;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float densiteCible;
        if (fearManager.niveauPeur < 0.3f)
        {
            densiteCible = 0.12f;
        }
        else if (fearManager.niveauPeur < 0.7f)
        {
            densiteCible = 0.08f;
        }
        else
        {
            densiteCible = 0.04f;
        }
        RenderSettings.fogDensity = Mathf.MoveTowards(
            RenderSettings.fogDensity,
            densiteCible,
            0.02f * Time.deltaTime
        );
    }
}
