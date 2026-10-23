using UnityEngine;

public class SilhouetteController : MonoBehaviour
{
    public GameObject silhouette;
    public FearManager fearManager;
    public Transform[] positionsApparition;

    public float dureeApparition = 3f; 

    private float tempsVisible = 0f;
    private bool silhouetteVisible = false;
    private bool silhouetteDejaVue = false;
    private bool disparitionAuDetournement = false;
    public float vitesseDeplacement = 2.5f;
    public float distanceArret = 3.5f;
    public LayerMask couchesObstacles;
    private bool silhouetteMobile = false;
    public float delaiReapparition = 5f;

    [Range(0f,1f)]
    public float augmentationPeur = 0.2f;

    public Transform joueur;
    public float distanceDisparition = 3f;
    public Transform cameraJoueur;
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
            if (JoueurRegardeSilhouette())
            {
                silhouetteDejaVue = true;
            } 
            else if (silhouetteDejaVue && disparitionAuDetournement)
            {
                FaireDisparaitre();
                return;
            } 
            DeplacerSilhouette();

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
        if (positionsApparition != null && positionsApparition.Length > 0)
        {
            int indexAleatoire = Random.Range(0, positionsApparition.Length);
            Transform pointChoisi = positionsApparition[indexAleatoire];
            if (pointChoisi != null)
            {
                silhouette.transform.position = pointChoisi.position;
            }
        }
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
        silhouetteDejaVue = false;
        disparitionAuDetournement = Random.value < 0.7f;
        silhouetteMobile = false;
        if (!disparitionAuDetournement)
        {
            if (fearManager.niveauPeur < 0.3f)
            {
                silhouetteMobile = Random.value < 0.6f;
            }
            else if (fearManager.niveauPeur < 0.7f)
            {
                silhouetteMobile = Random.value < 0.3f;
            }
        }
        silhouetteVisible = true;
        silhouette.SetActive(true);
        fearManager.AugmenterPeur(augmentationPeur);
    }
    void DeplacerSilhouette()
    {
        if (!silhouetteMobile || joueur == null)
        {
            return;
        }
        if (JoueurRegardeSilhouette())
        {
            return;
        }
        Vector3 direction = joueur.position - silhouette.transform.position;
        direction.y = 0f;
        float distance = direction.magnitude;
        if (distance > distanceArret)
        {
            Vector3 directionNormalisee = direction.normalized;
            Vector3 origineRayon = silhouette.transform.position + Vector3.up * 0.5f;
            bool obstacleDevant = Physics.Raycast(
                origineRayon,
                directionNormalisee,
                0.7f,
               couchesObstacles,
               QueryTriggerInteraction.Ignore
            );
            if (!obstacleDevant)
            {
                silhouette.transform.position += directionNormalisee * vitesseDeplacement * Time.deltaTime;
            }
        } 
    }
    bool JoueurRegardeSilhouette()
    {
        if (cameraJoueur == null || silhouette == null)
        {
            return false;
        }
        Vector3 directionSilhouette = silhouette.transform.position - cameraJoueur.position;
        float angle = Vector3.Angle(
            cameraJoueur.forward,
            directionSilhouette
        );
        return angle < 45f;
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
