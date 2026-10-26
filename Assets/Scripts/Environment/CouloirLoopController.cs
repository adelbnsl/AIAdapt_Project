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
    public int nombresBoucles;

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
    
    
    //  Appelée automatiquement par Unity lorsqu'un Collider entre dans la zone Trigger associée à cet objet.
    void OnTriggerEnter(Collider other)
    {
        if (other.transform != joueur) return;
        
        CharacterController controller = joueur.GetComponent<CharacterController>();
        // Désactive temporairement le CharacterController pour permettre la téléportation sans interférence avec la gestion des collisions.
        if (controller != null) controller.enabled = false;
        
        
        // Téléporte le joueur au début du couloir et restaure l'orientation définie au point de retour.
        joueur.position = pointRetour.position;
        joueur.rotation = pointRetour.rotation;
        
        nombresBoucles++;
        EventBus.CurrentLoop = nombresBoucles;
        Debug.Log("Boucle numéro : " + nombresBoucles);
        
        
        if (porte != null) porte.FermerPorte();
        if (porteEntree != null) porteEntree.OuvrirPorte();
        if (portraitController != null) portraitController.ChangerPortrait(nombresBoucles);
        if (chaiseHorreur != null) chaiseHorreur.ChangerPosition(nombresBoucles);
        
        // Niveau de peur lu UNE seule fois pour que toutes les décisions soient cohérentes.
        float peur = (fearManager != null) ? fearManager.niveauPeur : 0f;
        
        // ADAPTATION DE LA COULEUR DE L'ÉCLAIRAGE EN FONCTION DU NIVEAU DE PEUR.
        
        // Vérifie que l'objet entrant dans la zone correspond bien au joueur.
        if (other.transform == joueur)
            // Couleur de la lampe.
            if (lampeCouloir != null)
            {
                // PEUR FAIBLE : L'environnement devient progressivement plus inquiétant au fil des boucles. 
                if (peur < 0.3f)
                {
                    lampeCouloir.color = (nombresBoucles == 1)
                        ? new Color(1f, 0.8f, 0.6f)   // 1ère boucle : orangé
                        : new Color(1f, 0.3f, 0.2f);  // ensuite : rougeâtre
                }
                
                // PEUR MOYENNE : L'éclairage devient jaune afin de conserver une ambiance inquiétante mais modérée.
                else if (peur < 0.7f) lampeCouloir.color = new Color(1f, 0.9f, 0.2f);
                // PEUR ÉLEVÉE : L'éclairage devient blanc afin de réduire l'intensité visuelle de l'environnement.
                else lampeCouloir.color = Color.white;
            }
            
        
        // GESTION DE L'APPARITION DE L'OBJET HORRIFIQUE.
        if (objetHorreur != null)
        {
            float probabiliteApparition;
            if (peur < 0.3f) probabiliteApparition = 0.8f; // PEUR FAIBLE : 80% de chances de faire apparaître l'objet.
            else if (peur < 0.7f) probabiliteApparition = 0.4f; // PEUR MOYENNE : 40% de chances de faire apparaître l'objet.
            else probabiliteApparition = 0f; // PEUR ÉLEVÉE : L'objet ne doit pas apparaître.
 
            objetHorreur.SetActive(Random.value < probabiliteApparition);
        }
        
        
        
        //  Réactive le CharacterController après la téléportation et les modification du couloir.
        if (controller != null) controller.enabled = true;
    }
}
