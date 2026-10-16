using UnityEngine;

public class FearManager : MonoBehaviour
{
    [Range(0f, 1f)]
    public float niveauPeur = 0f;

    public bool simulationActive = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
       if (simulationActive)
        {
            niveauPeur = (Mathf.Sin(Time.time * 0.5f) + 1f) / 2f;
        } 
    }
}
