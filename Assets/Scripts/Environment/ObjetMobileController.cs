using UnityEngine;
/* 
    ObjetMobileController.cs
    Rôle : Gère le changement de position d'un objet du décor, notamment la chaise présente dans le couloir.
    Fonctionnalités principales : 
        - Déplacement entre 2 positions prédéfinies.
        - Déclenchement du déplacement selon une proba.
        - Adaptation de cette proba au niveau de peur du joueur.
        - Utilisation du numéro de boucle pour choisir la position.
    3 comportements sont utilisées :
        - Peur Faible (< 0.3) : 80% de chances de déplacement.
        - Peur moyenne (0.3 à 0.6) : 40% de chances de déplacemet.
        - Peur élevée (>= 0.7) : pas de déplacement.
    L'objectif est de créer des changements inattendus dans l'environnement lorsque le joueur est calme, tout en réduisant les évènements inquiétants lorsque son niveau de peur est trop élevée.
*/

public class ObjetMobileController : MonoBehaviour
{
    // Référence au gestionnaire du niveau de peur.
    public FearManager fearManager;

    // 1ère position de l'objet dans les coordonnées globales de la scène.
    public Vector3 positionInitiale;

    // 2nde position possible de l'objet dans les coordonnées globales de la scène.
    public Vector3 positionAlternative; 
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // Méthode publique appelée par CouloirLoopController après chaque boucle. Détermine si l'objet doit changer de position en fonction du niveau de peur et du numéro de boucle.
    public void ChangerPosition(int numeroBoucle)
    {
        // Vérifie que le gestionnaire de peur existe.
        if (fearManager == null)
        {
            return;
        }

        // Récupère le niveau de peur actuel du joueur.
        float peur = fearManager.niveauPeur;

        // Proba qu'un changemment de position soit appliqué.
        float probabiliteDeplacement;

        // PEUR FAIBLE : 80% de chances d'appliquer un changement de position.
        if (peur < 0.3f)
        {
            probabiliteDeplacement = 0.8f;
        }

        // PEUR MOYENNE : 40% de chances d'appliquer un changement de position.
        else if (peur < 0.7f)
        {
            probabiliteDeplacement = 0.4f;
        }

        // PEUR ÉLEVÉE : Aucun changement de position n'est autorisé.
        else
        {
            probabiliteDeplacement = 0f;
        }

        // Génère une valeur aléatoire entre 0 et 1. Le changement est effectué si cette valeur est < à la proba calculée.
        if (Random.value < probabiliteDeplacement) {

            // BOUCLE PAIRE : Place l'objet à sa position initiale.
            if (numeroBoucle % 2 == 0)
            {
                transform.position = positionInitiale;
            }

            // BOUCLE IMPAIRE : Place l'objet à sa position alternative.
            else
            {
                transform.position = positionAlternative;
            }
            Debug.Log("La chaise a changé de position à la boucle " + numeroBoucle);
        }
    }
}
