using UnityEngine;

public class SilhouetteController : MonoBehaviour
{
    public GameObject silhouette;
    public FearManager fearManager;

    public float dureeApparition = 3f; 

    private float tempsVisible = 0f;
    private bool silhouetteVisible = false;
    public float delaiReapparition = 5f;

    [Range(0f,1f)]
    public float augmentationPeur = 0.2f;

    public Transform joueur;
    public float distanceDisparition = 3f;
    private float variationAleatoire = 0f;
    private float tempsInvisible = 0f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
       silhouette.SetActive(false); 
       variationAleatoire = Random.Range(0f,1f);
    }

    // Update is called once per frame
    void Update()
    {
        if (fearManager.niveauPeur >= 0.7f)
        {
            if (silhouetteVisible)
            {
                FaireDisparaitre();
            }
            return; 
        }
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

            //Debug.Log("Temps visible : " + tempsVisible + " | Durée autorisée : " + dureeApparition);

            if (tempsVisible >= dureeApparition)
            {
                FaireDisparaitre();
            }
        }
        else
        {
            if(fearManager.niveauPeur < 0.3f)
            {
                delaiReapparition = Mathf.Lerp(4f,7f,variationAleatoire);
            }
            else if(fearManager.niveauPeur < 0.7f)
            {
                delaiReapparition = Mathf.Lerp(9f,13f,variationAleatoire);
            }
            else
            {
                delaiReapparition = Mathf.Lerp(18f,25f,variationAleatoire);
            }
            tempsInvisible += Time.deltaTime;
            if (tempsInvisible >= delaiReapparition)
            {
                FaireApparaitre();
            }
        }
        if (silhouette.activeSelf && joueur != null)
        {
            float distance = Vector3.Distance(
                joueur.position,
                silhouette.transform.position
            );
            if (distance <= distanceDisparition)
            {
                FaireDisparaitre();
                Debug.Log("Silhouette disparue : joueur trop proche.");
            }
        }
    }
    void FaireApparaitre()
    {
        if (joueur != null)
        {
            float distance = Vector3.Distance(
                joueur.position,
                silhouette.transform.position
            );
            if (distance <= distanceDisparition)
            {
                tempsInvisible = 0f;
                return;
            }
        }
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
        Debug.Log("Délai réel avant réapparition : " + tempsInvisible.ToString("F2") + " secondes");
        tempsVisible = 0f;
        silhouetteVisible = true;
        silhouette.SetActive(true);
        fearManager.AugmenterPeur(augmentationPeur);
    }
    void FaireDisparaitre()
    {

        //Debug.Log("Disparition : peur = " + fearManager.niveauPeur + " | temps visible = " + tempsVisible + " | durée autorisée = " + dureeApparition);

        silhouette.SetActive(false);
        silhouetteVisible = false;
        tempsVisible = 0f;
        tempsInvisible = 0f;
        variationAleatoire = Random.Range(0f,1f);
         
    }
}
