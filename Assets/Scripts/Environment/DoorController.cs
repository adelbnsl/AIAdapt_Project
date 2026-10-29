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
    private bool porteOuverte = false;

    // Vitesse de rotation de la porte en degrés/s. Cette valeur est adaptée au niveau de peur.
    public float vitesseOuverture = 60f;

    // Angle de rotation à atteindre lorsque la porte doit être ouverte.
    public float angleOuverture = -90f;

    // Angle que la porte doit progressivement atteindre. 0 correspond à la position fermée.
    private float angleCible = 0f;

    // Texte affichant la touche E près de la poignée.
    public GameObject texteInteraction;

    // Point 3D indiquant où afficher la touche E.
    public Transform pointInteraction;

    // Point d'interaction situé près de la poignée arrière.
    // Permet d'afficher la touche E du côté où se trouve le joueur.
    public Transform pointInteractionArriere;

    // Caméra du joueur utilisée pour calculer la position à l'écran.
    public Camera cameraJoueur;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // Calcule la distance entre le joueur et l'objet portant ce script.
        float distance = Vector3.Distance(
            transform.position,
            joueur.position
        );

        // PEUR FAIBLE : La porte s'ouvre et se ferme rapidement avec une vitesse de 90 degrés/s.
        if (fearManager.niveauPeur < 0.3f)
        {
            vitesseOuverture = 90f;
        }

        // PEUR MOYENNE : La porte s'ouvre et se ferme rapidement avec une vitesse de 60 degrés/s.
        else if (fearManager.niveauPeur < 0.7f)
        {
            vitesseOuverture = 60f; 
        }

        // PEUR ÉLEVÉE : La porte s'ouvre et se ferme rapidement avec une vitesse de 30 degrés/s.
        else
        {
            vitesseOuverture = 30f;
        }

        // Vérifie que le joueur est suffisamment proche et qu'il vient d'appuyer sur la touche E.
        if (distance <= distanceInteraction && Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("Le joueur intéragit la porte.");

            // Si la porte est fermée, on demande son ouverture.
            if (porteOuverte == false)
            {

                // Définit l'angle cible corespondant à la position ouverte.
                angleCible = angleOuverture;

                // Mémorise que la porte est ouverte.
                porteOuverte = true;
            }
            // Si la porte est ouverte, on commande sa fermeture.
            else
            {
                // Définit l'angle cible correspondant à la position fermée.
                angleCible = 0f;

                // Mémorise que la porte est fermée.
                porteOuverte = false;
            }
        }

        // Convertit l'angle cible en rotation Quaternion autour de l'axe vertical Y.
        Quaternion rotationCible = Quaternion.Euler(0f, angleCible, 0f);
        
        // Fait progressivement touner la porte vers la rotation cible. Quaternion.RotateTowars permet une rotation à vitesse contrôlée, sans changement brusque. Time.deltaTime rend cette rotation indépendante du nombre de fps.
        pivotPorte.localRotation = Quaternion.RotateTowards(
            pivotPorte.localRotation,
            rotationCible,
            vitesseOuverture * Time.deltaTime
        );

        // Actualise la position et la visibilité de la touche E.
        MettreAJourIndicationInteraction();
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
        // Indique que la porte est fermée.
        porteOuverte = false;

        // Définit l'angle cible de la porte ouverte.
        angleCible = angleOuverture;

        // Applique immédiatement la rotation d'ouverture. 
        pivotPorte.localRotation = Quaternion.Euler(0f, angleOuverture, 0f);
    }

    // Met à jour l'indication d'interaction de la porte. Affiche la touche E losque le joueur est suffisamment proche et que le point d'interaction se trouve devant la caméra. Convertit la position 3D de la poignée en coodonnée écran pour que le E suive les mouvement de la caméra et de la porte.
    void MettreAJourIndicationInteraction()
    {
        // Vérifie que les référence nécessaires existent.
        if (texteInteraction == null || pointInteraction == null || cameraJoueur == null || joueur == null)
        {
            return;
        }

        // Calcule la distance entre le joueur et la porte.
        float distance = Vector3.Distance(transform.position, joueur.position);

        // Détermine de quel coté de la porte se trouve le joueur. Le calcul utilise l'orientation de la porte même quand elle tourne.
        Vector3 directionJoueur = joueur.position - transform.position;
        float coteJoueur = Vector3.Dot(transform.forward, directionJoueur);

        // Selectionne le point correspondant au coté du joueur.
        Transform pointSelectionne = pointInteraction;
        if (coteJoueur > 0f && pointInteractionArriere != null)
        {
            pointSelectionne = pointInteractionArriere;
        }
        // Convertit la position 3D de la poignée en position écran.
        Vector3 positionEcran = cameraJoueur.WorldToScreenPoint(pointSelectionne.position);

        // Determine si l'indicateur doit apparaître.
        bool afficher = distance <= distanceInteraction && positionEcran.z > 0f;

        // Afficher ou masquer le E.
        texteInteraction.SetActive(afficher);
        if (afficher)
        {
            // Place le E près de la poignée à l'écran.
            RectTransform rectTexte = texteInteraction.GetComponent<RectTransform>();
            rectTexte.position = positionEcran;
        }
    }
}   