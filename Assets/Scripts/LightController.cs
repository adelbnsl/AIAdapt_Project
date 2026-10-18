using UnityEngine;
using System.Collections;

public class LightController : MonoBehaviour
{
    public Light lampe;
    public FearManager fearManager;
    public float intensiteNormale = 2f;
    public float intervalle = 0.5f;
    public float seuilPeurElevee = 0.7f;
    public float seuilPeurFaible = 0.3f;

    

    private float chronometre = 0f;
    private bool lampeAllumee = true; 
    private bool clignotementEvenementiel = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (clignotementEvenementiel)
        {
            return;
        }
        if(fearManager.niveauPeur >= seuilPeurElevee)
        {
            lampe.intensity = intensiteNormale;
            lampeAllumee = true;
            chronometre = 0f;
            return; 
        }
        chronometre += Time.deltaTime;
        if (chronometre >= intervalle)
        {
            lampeAllumee = !lampeAllumee;
            lampe.intensity = lampeAllumee ? intensiteNormale : 0f;
            chronometre = 0f;
            if (fearManager.niveauPeur < seuilPeurFaible)
            {
                intervalle = Random.Range(0.5f,1.5f);
            }
            else
            {
                intervalle = Random.Range(2f, 4f);
            }
        }        
    }
    public void DeclencherClignotment()
    {
        if (!clignotementEvenementiel)
        {
            StartCoroutine(ClignotementPonctuel());
        }
    }
    IEnumerator ClignotementPonctuel()
    {
        clignotementEvenementiel = true;
        float intensiteInitiale = lampe.intensity;
        lampe.intensity = 0f;
        yield return new WaitForSeconds(0.2f);
        lampe.intensity = intensiteInitiale;
        clignotementEvenementiel = false;
    }
}
