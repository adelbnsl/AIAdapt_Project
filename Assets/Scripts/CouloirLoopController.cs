using UnityEngine;

public class CouloirLoopController : MonoBehaviour
{
    public Transform joueur;
    public Transform pointRetour;
    public int nombresBoucles = 0;
    public FearManager fearManager;
    public Light lampeCouloir;
    public GameObject objetHorreur;
    public DoorController porte;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void OnTriggerEnter(Collider other)
    {
        if (other.transform == joueur)
        {
            CharacterController controller = joueur.GetComponent<CharacterController>();
            if (controller != null)
            {
                controller.enabled = false;
            }
            joueur.position = pointRetour.position;
            joueur.rotation = pointRetour.rotation;
            nombresBoucles++;
            if (porte != null)
            {
                porte.FermerPorte();
            }
            Debug.Log("Boucle numéro : " + nombresBoucles);
            if (fearManager.niveauPeur < 0.3f)
            {
                Debug.Log("Peur faible : évènements horrifiques renforcées");
                if (nombresBoucles == 1)
                {
                    lampeCouloir.color = new Color(1f, 0.8f, 0.6f);
                }
                else if (nombresBoucles >= 2)
                {
                    lampeCouloir.color = new Color(1f, 0.3f, 0.2f);
                }
            }
            else if (fearManager.niveauPeur < 0.7f)
            {
                Debug.Log("Peur moyenne : évènements horrifiques modérés");
                lampeCouloir.color = new Color(1f, 0.9f, 0.2f);
            }
            else
            {
                Debug.Log("Peur élevée : évènements horrifiques réduits");
                lampeCouloir.color = Color.white;
            }

            if (nombresBoucles >= 1)
            {
                float probabiliteApparition;
                if (fearManager.niveauPeur < 0.3f)
                {
                    probabiliteApparition = 0.8f;
                }
                else if (fearManager.niveauPeur < 0.7f)
                {
                    probabiliteApparition = 0.4f;
                }
                else
                {
                    probabiliteApparition = 0f;
                }
                bool apparition = Random.value < probabiliteApparition;
                objetHorreur.SetActive(apparition);
            }
            
            if (controller != null)
            {
                controller.enabled = true;
            }
            Debug.Log("Joueur téléporté au début du couloir.");

        }
    }
}
