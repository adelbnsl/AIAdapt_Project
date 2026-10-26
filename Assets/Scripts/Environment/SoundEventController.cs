using UnityEngine;
/*
    SoundEventController.cs
    Rôle : Gère les évènements sonores aléatoire du jeu afin de renforcer l'ambiance horrifique.
    3 évènements possibles :
        - Lecture d'un son accompagné d'un clignotement lumineux. 
        - Bruit de pas provenant de derrière le joueur.
        - Chuchotement provenant de derrière le joueur. 
    La fréquence des évènements dépend du niveau de peur :
        - Peur faible (< 0.3) : Evènements fréquents (5 à 10s).
        - Peur moyenne (0.3 à 0.6) : Evènements espacés (12 à 20s).
        - Peur élevé (>= 0.7) : Aucun évènements automatique. 
    Chaque déclenchement automatique augmente le niveau de peur. L'objectif est d'adapter 
    l'intensité des évènements sonores pour éviter de surcharger le joueur lorsqu'il a trop
    peur.
*/ 

public class SoundEventController : MonoBehaviour
{
    // Source audio principale utilisée pour le 1er type d'évènement sonore. 
    private AudioSource audioSource;

    // Référence au gestionnaire du niveau de peur.
    public FearManager fearManager;

    // Source audio permettant de diffuser des sons derrière le joueur.
    public AudioSource audioSourceDerriere;

    // Extraits audio utilisées pour les évènements. 
    public AudioClip sonPas;
    public AudioClip sonChuchotement;

    // Référence au contrôleur de lumière pour déclencher un clignotement lors de certains évènements.
    public LightController lightController;

    // Quantité de peur ajoutée après chaque évènement automatique. La valeur est comprise entre 0 et 1.
    [Range(0f, 1f)]
    public float augmentationPeur = 0.2f;

    // Active ou désactive le déclenchement automatique des évènements sonores.
    public bool declenchementAutomatique;

    // Délais minimum et maximum entre les évènements lorsque le niveau de peur est faible.
    public float delaiMinimum = 5f;
    public float delaiMaximum = 10f;

    // Délais et minimum est maximum entre les évènements lorsque le niveau de peur est moyen. 
    public float delaiMinimumPeurMoyenne = 12f;
    public float delaiMaximumPeurMoyenne = 20f;
    
    // Intensités publiées avec chaque événement.
    [Range(0f, 1f)] public float intensiteSonPrincipal = 0.8f;
    [Range(0f, 1f)] public float intensitePas = 0.5f;
    [Range(0f, 1f)] public float intensiteChuchotement = 0.4f;

    // Chronomètre mesurant le temps écoulé depuis le dernier évènement automatique.    
    private float chronometre;

    //Délai aléatoire à atteindre avannt le prochain évènement.
    private float prochainDeclanchement;

    // Mémorise la catégorie de peur précédente afin de détecter un changement de niveau. -1 indique qu'aucune catégorie n'as encore été enrengistrée. 
    private int niveauPeurPrecedent = -1;
    
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Récupère la source audio principale.
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) Debug.LogWarning("SoundEventController : aucun AudioSource sur cet objet.");

        // Calcule le délair initial avant le 1er évènement. 
        prochainDeclanchement = CalculerDelai();
    }

    // Update is called once per frame
    void Update()
    {
        // Récupère la catégorie de peur actuelle du joueur.
        int categorieActuelle = ObtenirCategoriePeur();

        // Si le joueur change de catégorie de peur, un nouveau délai est calculé pour adapter la fréquence des évènements. 
        if (categorieActuelle != niveauPeurPrecedent)
        {
            niveauPeurPrecedent = categorieActuelle;
            prochainDeclanchement = CalculerDelai();
        }

        // Les évènements automatiques sont autorisés uniquement si cette fonctionnalité est activée et si le niveau de peur reste < à 0.7. 
        if (declenchementAutomatique && fearManager.niveauPeur < 0.7){
            
            // Incrémente le chronomètre avec le temps écoulé.
            chronometre += Time.deltaTime;

            // Déclenche un évènemenet lorsque le délair est atteint.
            if (chronometre >= prochainDeclanchement)
                {
                    // Sélectionne et joue un évènement sonore aléatoire.
                    JouerEvenementSonore();

                    // Augmente la peur du joueur après l'évènement.
                    fearManager.AugmenterPeur(augmentationPeur);

                    // Reinitialise le chronomètre.
                    chronometre = 0f;

                    // Calcule un nouveau délai aléatoire avant le prochain évènement.
                    prochainDeclanchement = CalculerDelai();
                }
        }
    }

    // Calcule un délai aléatoire en fonction du niveau de peur actuel du joueur.
    float CalculerDelai()
    {
        // PEUR FAIBLE : Les évènements sont plus fréquents.
        if (fearManager.niveauPeur < 0.3f) return Random.Range(delaiMinimum, delaiMaximum);

        // PEUR MOYENNE OU ÉLEVÉE : Les évènements automatique restent cependant désactivées lorsque la peur atteint 0.7. 
        return Random.Range(delaiMinimumPeurMoyenne, delaiMaximumPeurMoyenne);
    }

    // Détermine la catégorie de peur du joueur. 0 = Faible, 1 = Moyen, 2 = Élevée 
    int ObtenirCategoriePeur()
    {
        if (fearManager.niveauPeur < 0.3f) return 0;
        if (fearManager.niveauPeur < 0.7f) return 1;
        return 2;
    }

    //Positionne aléatoirement la source audio secondaire dans une zone située derrière le joueur, selon les coordonnées locales de son objet parent. 
    void PositionnerSonDerriereJoueur()
    {
        if (audioSourceDerriere == null) return; 
        
        // Génère une position horizontale aléatoire.
        float postitionX = Random.Range(-2f, 2f);

        // Génère une profondeur aléatoire derrière le joueur.
        float positionZ = Random.Range(-4f, -1.5f);

        // Applique la position à la source audio secondaire.
        audioSourceDerriere.transform.localPosition = new Vector3(postitionX, 0f, positionZ);
    }

    // Sélectionne aléatoirement l'un des 3 évènements sonores.
    void JouerEvenementSonore()
    {
        int evenement = Random.Range(0, 3);
 
        // ÉVÉNEMENT 0 : son principal + clignotement (un seul événement composite publié).
        if (evenement == 0)
        {
            if (audioSource == null) return;
            audioSource.Play();
            if (lightController != null) lightController.DeclencherClignotment();
            EventBus.Publish("son_clignotement",
                EventCategory.Son | EventCategory.Lumiere | EventCategory.Obscurite,
                intensiteSonPrincipal);
        }
        // ÉVÉNEMENT 1 : pas derrière le joueur.
        else if (evenement == 1)
        {
            if (sonPas != null && audioSourceDerriere != null)
            {
                PositionnerSonDerriereJoueur();
                audioSourceDerriere.PlayOneShot(sonPas);
                EventBus.Publish("son_pas",
                    EventCategory.Son | EventCategory.Derriere | EventCategory.Approche,
                    intensitePas);
            }
        }
        // ÉVÉNEMENT 2 : chuchotement derrière le joueur.
        else
        {
            if (sonChuchotement != null && audioSourceDerriere != null)
            {
                PositionnerSonDerriereJoueur();
                audioSourceDerriere.PlayOneShot(sonChuchotement);
                EventBus.Publish("son_chuchotement",
                    EventCategory.Son | EventCategory.Derriere | EventCategory.Entite,
                    intensiteChuchotement);
            }
        }
    }
}
