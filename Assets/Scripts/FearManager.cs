using UnityEngine;

public class FearManager : MonoBehaviour
{
    [Range(0f, 1f)]
    public float niveauPeur = 0f;

    public bool simulationActive = false;
    public bool simulationEvenements = false;
    public float vitesseDiminutionPeur = 0.05f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
       if (simulationActive && !simulationEvenements)
        {
            niveauPeur = (Mathf.Sin(Time.time * 0.5f) + 1f) / 2f;
        } 
        else if (simulationEvenements)
        {
            niveauPeur = Mathf.MoveTowards(
                niveauPeur,
                0f,
                vitesseDiminutionPeur * Time.deltaTime
            ); 
        }
    }
    public void AugmenterPeur(float quantite)
    {
        if (simulationEvenements)
        {
            niveauPeur = Mathf.Clamp01(niveauPeur + quantite);
        }
    }
}
