using UnityEngine;

public class SilhouetteController : MonoBehaviour
{
    public GameObject silhouette;
    public FearManager fearManager;

    public float dureeApparition = 3f; 

    private float tempsVisible = 0f;
    private bool silhouetteVisible = false;
    public float delaiReapparition = 5f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
       silhouette.SetActive(false); 
       Invoke(nameof(FaireApparaitre), 5f); 
    }

    // Update is called once per frame
    void Update()
    {
        if (silhouetteVisible)
        {
            if(fearManager.niveauPeur < 0.3f)
            {
                dureeApparition = 4f;
            }
            else if(fearManager.niveauPeur < 0.7f)
            {
                dureeApparition = 2f;
            }
            else
            {
                dureeApparition = 1f; 
            }
            tempsVisible += Time.deltaTime;

            Debug.Log("Temps visible : " + tempsVisible + " | Durée autorisée : " + dureeApparition);

            if (tempsVisible >= dureeApparition)
            {
                FaireDisparaitre();
            }
        }
    }
    void FaireApparaitre()
    {
        if (fearManager.niveauPeur < 0.3f)
        {
            dureeApparition = 4f;
        }
        else if (fearManager.niveauPeur < 0.7f)
        {
            dureeApparition = 2f;
        }
        else
        {
            dureeApparition = 1f;
        }
        tempsVisible = 0f;
        silhouetteVisible = true;
        silhouette.SetActive(true);
    }
    void FaireDisparaitre()
    {

        Debug.Log("Disparition : peur = " + fearManager.niveauPeur + " | temps visible = " + tempsVisible + " | durée autorisée = " + dureeApparition);

        silhouette.SetActive(false);
        silhouetteVisible = false;
        Invoke(nameof(FaireApparaitre), delaiReapparition); 
    }
}
