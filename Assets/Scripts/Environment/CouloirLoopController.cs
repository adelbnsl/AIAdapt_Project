using UnityEngine;
/* 
    CouloirLoopController.cs
    Rôle : Gère le système de boucle infinie du couloir.
    Lorsqu'un joueur traverse une zone de déclenchement, il est ramené au début du couloir.
    A chaque nouvelle boucle, on : 
        - Incrémente le compteur de boucles.
        - Réinitialise la porte de sortie.
        - Ouvre la porte d'entrée.
        - Modifie le portrait selon le nombre de boucle.
        - Adapte la couleur de l'éclairage au niveau de peur.
        - Détermine aléatoirement l'apparition d'un objet horrifique. 
    L'objectif est de créer un environnement évolutif qui s'adapte à la peur du joueur tout en conservant l'illusion d'un couloir infini.
*/

public class CouloirLoopController : MonoBehaviour
{
    // Référence au joueur qui traverse le couloir.
    public Transform joueur;

    // Position et orientation auxquelles le joueur est téléporté après chaque boucle.
    public Transform pointRetour;

    // Compteur du nombre de boucles effectuées. Initialisé à 0 au démarrage du jeu.
    public int nombresBoucles = 0;

    // Référence au gestionnaire du niveau de peur.
    public FearManager fearManager;

    // Lampe du couloir dont la couleur est adaptée au niveau de peur du joueur et au nombre de boucles.
    public Light lampeCouloir;

    // Objet horrifique dont l'apparition est aléatoire.
    public GameObject objetHorreur;

    // Porte de sortie à refermer après chaque boucle.
    public DoorController porte;

    // Porte d'entrée à ouvrir après chaque boucle.
    public DoorController porteEntree;

    // Contrôleur permettant de modifier le portrait selon le nombre de boucles effectuées. 
    public PortraitController portraitController;

    // Contrôleur permettant de modifier la position de la chaise horrifique à chaque boucle.
    public ObjetMobileController chaiseHorreur;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    //  Appelée automatiquement par Unity lorsqu'un Collider entre dans la zone Trigger associée à cet objet.
    void OnTriggerEnter(Collider other)
    {
        // Vérifie que l'objet entrant dans la zone correspond bien au joueur.
        if (other.transform == joueur)
        {
            // Récupère le CharacterController du joueur.
            CharacterController controller = joueur.GetComponent<CharacterController>();

            // Désactive temporairement le CharacterController pour permettre la téléportation sans interférence avec la gestion des collisions.
            if (controller != null)
            {
                controller.enabled = false;
            }

            // Téléporte le joueur au début du couloir et restaure l'orientation définie au point de retour.
            joueur.position = pointRetour.position;
            joueur.rotation = pointRetour.rotation;

            // Incrémente le compteur de boucles.
            nombresBoucles++;
            Debug.Log("BOUCLE DETECTEE : " + nombresBoucles);

            // Referle instantanément la porte de sortie.
            if (porte != null)
            {
                porte.FermerPorte();
            }

            // Ouvre instantanément la porte d'entrée.
            if (porteEntree != null)
            {
                porteEntree.OuvrirPorte();
            }

            // Demande au contrôler du portrait de modifier son apparence selon le nombre de boucles.
            if (portraitController != null)
            {
                portraitController.ChangerPortrait(nombresBoucles);
            }

            // Demande au contrôler de la chaise de modifier sa position selon le nombre de boucles.
            if (chaiseHorreur != null)
            {
                chaiseHorreur.ChangerPosition(nombresBoucles);
            }
            Debug.Log("Boucle numéro : " + nombresBoucles);

            // ADAPTATION DE LA COULEUR DE L'ÉCLAIRAGE EN FONCTION DU NIVEAU DE PEUR.
            // PEUR FAIBLE : L'environnement devient progressivement plus inquiétant au fil des boucles. 
            if (fearManager.niveauPeur < 0.3f)
            {
                Debug.Log("Peur faible : évènements horrifiques renforcées");

                // 1ère boucle : éclairage légérement orangé.
                if (nombresBoucles == 1)
                {
                    lampeCouloir.color = new Color(1f, 0.8f, 0.6f);
                }

                // A partir de la 2nde boucle : éclairage rougeâtre plus inquiétant.
                else if (nombresBoucles >= 2)
                {
                    lampeCouloir.color = new Color(1f, 0.3f, 0.2f);
                }
            }

            // PEUR MOYENNE : L'éclairage devient jaune afin de conserver une ambiance inquiétante mais modérée.
            else if (fearManager.niveauPeur < 0.7f)
            {
                Debug.Log("Peur moyenne : évènements horrifiques modérés");
                lampeCouloir.color = new Color(1f, 0.9f, 0.2f);
            }

            // PEUR ÉLEVÉE : L'éclairage devient blanc afin de réduire l'intensité visuelle de l'environnement.
            else
            {
                Debug.Log("Peur élevée : évènements horrifiques réduits");
                lampeCouloir.color = Color.white;
            }

            // GESTION DE L'APPARITION DE L'OBJET HORRIFIQUE.
            // A partir de la 1ère boucle :
            if (nombresBoucles >= 1)
            {
                // Proba d'appartition de l'objet horrifique.
                float probabiliteApparition;

                // PEUR FAIBLE : 80% de chances de faire apparaître l'objet.
                if (fearManager.niveauPeur < 0.3f)
                {
                    probabiliteApparition = 0.8f;
                }

                // PEUR MOYENNE : 40% de chances de faire apparaître l'objet.
                else if (fearManager.niveauPeur < 0.7f)
                {
                    probabiliteApparition = 0.4f;
                }

                // PEUR ÉLEVÉE : L'objet ne doit pas apparaître. 
                else
                {
                    probabiliteApparition = 0f;
                }

                // Génère une valeur aléatoire entre 0 et 1 et la compare à la proba d'apparition.
                bool apparition = Random.value < probabiliteApparition;

                // Active ou non l'objet horrifique selon le résultat du tirage précedant. 
                objetHorreur.SetActive(apparition);
            }
            
            //  Réactive le CharacterController après la téléportation et les modification du couloir.
            if (controller != null)
            {
                controller.enabled = true;
            }
            Debug.Log("Joueur téléporté au début du couloir.");

        }
    }
}
