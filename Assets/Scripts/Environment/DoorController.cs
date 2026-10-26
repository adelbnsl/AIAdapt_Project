using UnityEngine;
/* 
    DoorController.cs
    Rôle : Gère l'ouverture et la fermeture d'une porte avec laquelle le joueur peut intéragir.
    Fonctionnalitées principales :
        - Détection de la proximité du joueur.
        - Interraction avec la touche E.
        - Ouverture et fermeture progressives de la porte.
        - Adaptation de la vitesse en rotation en fonction du niveau de peur du joueur.
        - Possibilité d'ouvrir ou de fermer instantanément la porte depuis un autre script.
    3 vitesses sont utilisés :
        - Peur faible (< 0.3) : 90 degrés/s.
        - Peur moyenne (0.3 à 0.6) : 60 degrés/s.
        - Peur élevée (>= 0.7) : 30 degrés/s.
*/

public class DoorController : MonoBehaviour
{
    // Référence au joueur permettant de calculer sa distance par rapport à la porte.
    public Transform joueur;

    // Distance maximale autorisant l'interaction du joueur avec la porte.
    public float distanceInteraction = 2f;

    // Pivot autour duquel la porte effectue sa rotation.
    public Transform pivotPorte;

    // Référence au gestionnaire du niveau de peur.
    public FearManager fearManager;

    // Indique si la porte est considérée comme ouverte. Par défaut, elle est fermée.
    private bool porteOuverte;

    // Vitesse de rotation de la porte en degrés/s. Cette valeur est adaptée au niveau de peur.
    public float vitesseOuverture = 60f;

    // Angle de rotation à atteindre lorsque la porte doit être ouverte.
    public float angleOuverture = -90f;

    // Angle que la porte doit progressivement atteindre. 0 correspond à la position fermée.
    private float angleCible;


    // Update is called once per frame
    void Update()
    {

        if (joueur == null || pivotPorte == null) return;
        
        // Vitesse selon le niveau de peur
        if (fearManager != null)
        {
            float peur = fearManager.niveauPeur;
            if (peur< 0.3f) vitesseOuverture = 90f; // PEUR FAIBLE : La porte s'ouvre et se ferme rapidement avec une vitesse de 90 degrés/s.
            else if (peur <0.7f) vitesseOuverture = 60f; // PEUR MOYENNE : La porte s'ouvre et se ferme rapidement avec une vitesse de 60 degrés/s.
            else vitesseOuverture = 30f; // PEUR ÉLEVÉE : La porte s'ouvre et se ferme rapidement avec une vitesse de 30 degrés/s.
        }
        
        // Calcule la distance entre le joueur et l'objet portant ce script.
        float distance = Vector3.Distance(transform.position, joueur.position);
        

        // Vérifie que le joueur est suffisamment proche et qu'il vient d'appuyer sur la touche E.
        if (distance <= distanceInteraction && Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("Le joueur intéragit la porte.");
            porteOuverte = !porteOuverte;
            angleCible = porteOuverte ? angleOuverture : 0f;
        }

        // Convertit l'angle cible en rotation Quaternion autour de l'axe vertical Y.
        Quaternion rotationCible = Quaternion.Euler(0f, angleCible, 0f);
        
        // Fait progressivement touner la porte vers la rotation cible. Quaternion.RotateTowars permet une rotation à vitesse contrôlée, sans changement brusque. Time.deltaTime rend cette rotation indépendante du nombre de fps.
        pivotPorte.localRotation = Quaternion.RotateTowards(
            pivotPorte.localRotation,
            rotationCible,
            vitesseOuverture * Time.deltaTime
        );

    }

    // Méthode publique permettant à un autre script de fermer instantanément la porte. Utile lors de la reinitialisation du couloir après une boucle.
    public void FermerPorte()
    {
        // Indique que la porte est fermée.
        porteOuverte = false;

        // Reinitialise l'angle cible.
        angleCible = 0f;

        // Replace immédiatement la porte dans sa rotation initiale.
        pivotPorte.localRotation = Quaternion.Euler(0f, 0f, 0f);
    }
    
    // Méthode publique permettant à un autre script d'ouvrir instantanément la porte sans animation progressive.
    public void OuvrirPorte()
    {
        porteOuverte = true;
        // Définit l'angle cible de la porte ouverte.
        angleCible = angleOuverture;
        // Applique immédiatement la rotation d'ouverture. 
        pivotPorte.localRotation = Quaternion.Euler(0f, angleOuverture, 0f);
    }
}
