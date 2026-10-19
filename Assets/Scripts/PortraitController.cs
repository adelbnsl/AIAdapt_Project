using UnityEngine;

public class PortraitController : MonoBehaviour
{
    public Texture[] portraits;
    public Renderer renduPortrait;
    public FearManager fearManager;
    public float vitessePulsation = 2f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (fearManager == null || renduPortrait == null)
        {
            return;
        }
        float peur = fearManager.niveauPeur;
        if (peur < 0.3f)
        {
            renduPortrait.material.SetColor(
                "_BaseColor",
                new Color(1f, 0.85f, 0.85f)
            );
        }
        else if (peur < 0.7f)
        {
            renduPortrait.material.SetColor(
                "_BaseColor",
                new Color(0.7f, 0.8f, 1f)
            );
        }
        else
        {
            renduPortrait.material.SetColor(
                "_BaseColor",
                Color.white
            );
        }
        Color couleurEmission;
        if (peur < 0.3f)
        {
            couleurEmission = new Color(0.25f, 0.02f, 0.02f);
        }
        else if (peur < 0.7f)
        {
            couleurEmission = new Color(0.02f, 0.04f, 0.12f);
        }
        else
        {
            couleurEmission = Color.black;
        }
        float intensitePulsation = 1f;
        if (peur < 0.7f)
        {
            float temps = Time.time;

            float pulsation = Mathf.Sin(
                Time.time * vitessePulsation
                + 0.4f * Mathf.Sin(temps * 0.7f)
            );
            intensitePulsation = 0.85f + 0.15f * pulsation;

        }
        couleurEmission *= intensitePulsation;
        renduPortrait.material.EnableKeyword("_EMISSION");
        renduPortrait.material.SetColor("_EmissionColor", couleurEmission);
    }
    public void ChangerPortrait(int numeroBoucle)
    {
        if (portraits == null || portraits.Length == 0 || renduPortrait == null)
        {
            return;
        }
        int indexPortrait = numeroBoucle % portraits.Length;
        if (fearManager != null && fearManager.niveauPeur >= 0.7f)
        {
            indexPortrait = 0;
        }
        renduPortrait.material.SetTexture("_BaseMap", portraits[indexPortrait]);
        Debug.Log("Boucle : " + numeroBoucle + " | Portrait sélectionné : " + indexPortrait);
    }
}
