using UnityEngine;
/* 
    InterrupteurController.cs
    Rôle : Gère l'interactionn entre le joueur et un interrupteur mural, ainsi que le mouvement physique de son bouton.
    Fonctionnement :
        - Le joueur peur interagir avec l'interrupteur en appuyant sur E lorsqu'il se trouve à une distance suffisament proche.
        - Chaque interaction inverse l'état de l'interrupteur :
            * Allumé : Le bouton est légérement enfoncé.
            * Eteint : Le bouton revient à sa position normale.
        - Le déplacement du bouton est progressif afin de reproduire le comportement mécanique d'un véritable interrupteur.
        - La position initiale est mémorisée au démarage pour permettre son retour à la position d'origine.
        - La lumière peut s'éteindre ou non selon le niveau de peur du joueur :
            * Peur faible (< 0.3) : la lumière a 30% de ne pas répondre a l'interrupteur.
            * Peur moyenne (0.3 à 0.6) : la lumière a 15% de ne pas répondre a l'interrupteur.
            * Peur élevée (>= 0.7) : la lumière répond normalement.
    Intégration avec le système adaptatif :
        - L'interrupteur possède son propre état d'allumage.
        - Pour le moment, il contrôle uniquement le mouvement du bouton.
        Il sera relié à LightController afin de permettre au joueur d'allumer ou d'éteindre la lumière.
        - Les clignotement liée au niveau de peur seront conservés lorsque l'interrupteur autorisera l'éclairage.
    L'objectif est d'améliorer l'immersion du joueur en ajoutant une interaction réaliste avec les éléments du décor, tout en conservant le fonctionnement adaptatif de l'environnement horrifique.
*/

public class InterrupteurController : MonoBehaviour
{
    // Référence au joueur pour détecter sa proximité.
    public Transform joueur;

    // Référence au bouton physique de l'interrupteur.
    public Transform boutonInterrupteur;

    // Référence au gestionnaire du niveau de peur du joueur.
    public FearManager fearManager;

    // Référence au contrôleur de la lumière principale du couloir.
    public LightController lightController;


    // Proba que l'interrupteur échoue lorsque la peur est faible.
    [Range(0f, 1f)]
    public float probabilitéEchecPeurFaible = 0.30f;

    // Proba que l'interrupteur échoue lorsque la peur est moyenne.
    [Range(0f, 1f)]
    public float probabilitéEchecPeurMoyenne = 0.15f;

    // Distance maximale pour interagir avec l'interrupteur.
    public float distanceInteraction = 2f;

    // Etat général de l'interrupteur.
    public bool lumiereAllumee = true;

    // Position initiale du bouton.
    private Vector3 positionInitiale;

    // Vitesse de déplacement du bouton.
    public float vitesseBouton = 3f;

    // Enfoncement du bouton lorsqu'il est activé.
    public float profondeurEnfoncement = 0.15f;

    // Interface affichée lorsque le joueur peut interagir.
    public GameObject texteInteraction;

    // Point 3D indiquant où afficher la touche E.
    public Transform pointInteraction;

    // Caméra utilisée pour convertif la position 3D en position écran.
    public Camera cameraJoueur;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // La position initiale est celle où se trouve le bouton de base dans le couloir.
        positionInitiale = boutonInterrupteur.localPosition;
    }

    // Update is called once per frame
    void Update()
    {
        // Vérifie si le joueur est assez proche.
        float distance = Vector3.Distance(transform.position, joueur.position);

        // Vérifie si le joueur est assez proche et appuie sur E.
        if (distance <= distanceInteraction && Input.GetKeyDown(KeyCode.E))
        {
            // Détermine si l'interrupteur fonctionne correctement.
            if (InterractionEchoue())
            {
                // L'interrupteur ne marche pas : la lumière ne change pas d'état.
                Debug.Log("Interrupteur : échec de l'interaction !");
            }
            else
            {
                // L'interrupteur fonctionne : inverse l'état de la lumière.
                lumiereAllumee = !lumiereAllumee;

                // Transmet le nouvel état au contrôleur de lumière/
                if (lightController != null)
                {
                    lightController.DefinirEtatInterrupteur(lumiereAllumee);
                }
                Debug.Log("Interrupteur : lumière " + (lumiereAllumee ? "allumée" : "éteinte"));
            }
        }

        // Calcule la position souhaitée du bouton.
        Vector3 positionCible = positionInitiale;

        // Si la lumière est allumé.
        if (lumiereAllumee)
        {
            // Le bouton s'enfonce légèrement vers le mur.
            positionCible.x += profondeurEnfoncement;
        }
        
        // Déplacement progressif du bouton.
        boutonInterrupteur.localPosition = Vector3.MoveTowards(
            boutonInterrupteur.localPosition,
            positionCible,
            vitesseBouton * Time.deltaTime
        );

        // Actualise l'affichage et la position de la touche E.
        MettreAJourIndicationInteraction();
    }

    // Détermine si une interaction avec l'interrupteur doit échouer en fonction de peur actuel du joueur.
    bool InterractionEchoue()
    {
        // Si aucun FearManager n'est associé, l'interrupteur fonctionne normalement.
        if (fearManager == null)
        {
            return false;
        }

        // Calcule la proba d'échouer selon la peur actuel du joueur.
        float probabiliteEchec = 0f;

        // PEUR FAIBLE : risque d'échouer plus élevé.
        if (fearManager.niveauPeur < 0.3f)
        {
            probabiliteEchec = probabilitéEchecPeurFaible;
        }
        
        // PEUR MOYENNE : risque d'échec réduit.
        else if (fearManager.niveauPeur < 0.7)
        {
            probabiliteEchec = probabilitéEchecPeurMoyenne;
        }

        // PEUR ÉLEVÉE : aucun risque d'échec

        // Tirer une valeur aléatoire entre 0 et 1.
        float tirage = Random.value;

        // Renvoie true si l'interaction échoue. 
        return tirage < probabiliteEchec;
    }

    // Affiche la touche E près de l'interrupteur lorsque le joueur est suffisament proche pour interagir. Sa position suit le point de référence dans le monde 3D.
    void MettreAJourIndicationInteraction()
    {
        // Si le texte, le point d'interaction, la caméra ou le joueur ne sont pas disponible, on quitte la méthode.
        if (texteInteraction == null || pointInteraction == null || cameraJoueur == null || joueur == null)
        {
            return;
        }

        // Vérifie la distance entre le joueur et l'interrupteur.
        float distance = Vector3.Distance(transform.position, joueur.position);

        // Convertit la position du point 3D en coordonée écran.
        Vector3 positionEcran = cameraJoueur.WorldToScreenPoint(pointInteraction.position); 

        // Affiche E uniquement si le joueur est assez proche et si le point se trouve devant la caméra.
        bool afficher = distance <= distanceInteraction && positionEcran.z > 0f;
        texteInteraction.SetActive(afficher);
        if (afficher)
        {
            // Positionne le texte à l'endroit correspondant à l'écran.
            RectTransform rectTexte = texteInteraction.GetComponent<RectTransform>();
            rectTexte.position = positionEcran;
        }
    }

}
