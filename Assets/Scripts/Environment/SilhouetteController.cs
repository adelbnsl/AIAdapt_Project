using UnityEngine;
/*
    SilhouetteController.cs
    Rôle : Gère le comportement d'une silhouette inquiétante dans l'environnement du jeu
    Fonctionnalités principales :
        - Apparition à des positions aléatoires prédefinies.
        - Adaption des durées d'apparition et des délais de réapparition selon le niveau
        de peur du joueur.
        - Possibilité de disparaître lorsque le joueur détourne son regard après avoir 
        vu la sihouette.
        - Possibilité de se déplacer vers le joueur lorsqu'elle n'est pas observée.
        - Détection des obstacles pendant ses déplacements.
        - Disparition lorsque le joueur s'approche trop près.
        - Augmentation du niveau de peur à chaque apparition. 
    Lorsque la peur atteint 0.7, la silhouette disparaît et aucune nouvelle apparition n'est 
    autorisée.
    */

public class SilhouetteController : MonoBehaviour
{
    // Objet représentant la silhouette dans la scène.
    public GameObject silhouette;

    // Référence au gestionnaire du niveau de peur.
    public FearManager fearManager;

    // Liste des positions possibles pour faire apparaître la silhouette dans l'environnement.
    public Transform[] positionsApparition;

    // Durée maximale pendant laquelle la silhouette reste visible avant de disparaître.
    public float dureeApparition = 3f; 

    // Temps écoulé depuis l'apparition de la silhouette.
    private float tempsVisible = 0f;

    // Indique si la silhouette est actuellement visible.
    private bool silhouetteVisible = false;

    // Indique si le joueur a déjà regardé la silhouette depuis sa dernière apparition.
    private bool silhouetteDejaVue = false;

    // Détermine si la silhouette doit disparaître lorsque le joueur détourne son regard.
    private bool disparitionAuDetournement = false;

    //Vitesse de déplacement de la silhouette vers le joueur.
    public float vitesseDeplacement = 2.5f;

    // Distance à partir de laquelle la silhouette arrête de se rapprocher du joueur.
    public float distanceArret = 3.5f;

    // Couches utilisées pour détecter les obstacles empêchant le déplacement de la silhouette.
    public LayerMask couchesObstacles;

    // Indique si la silhouette peut se déplacer pendant son apparition actuelle.
    private bool silhouetteMobile = false;

    // Délai à attendre avant une nouvelle apparition.
    public float delaiReapparition = 5f;

    // Quantité de peur ajoutée lors de chaque apparition.
    [Range(0f,1f)]
    public float augmentationPeur = 0.2f;

    // Référence à la position du joueur.
    public Transform joueur;

    //Distance minimale entre le joueur et la silhouette. Si le joueur est trop proche, la silhouette disparait.
    public float distanceDisparition = 3f;

    // Référence à la caméra du joueur. Permet de déterminer s'il regarde la silhouette ou non.
    public Transform cameraJoueur;

    // Valeur aléatoire utilisée pour calculer les délais de réaparition.
    private float variationAleatoire = 0f;

    // Temps écoulé depuis la dernière disparition.
    private float tempsInvisible = 0f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Cache la silhouette au lancement du jeu.
        silhouette.SetActive(false); 
        // Génère une 1ère valeur aléatoire entre 0 et 1.
        variationAleatoire = Random.Range(0f,1f);
    }

    // Update is called once per frame
    void Update()
    {
        // PEUR ÉLEVÉE : Si le niveau de peur atteint 0.7, la silhouette disparaît immédiatement. Les autres comportements sont suspendues. 
        if (fearManager.niveauPeur >= 0.7f)
        {
            if (silhouetteVisible)
            {
                FaireDisparaitre();
            }
            return; 
        }

        // CAS 1 : La silhouette est actuellement visible.
        if (silhouetteVisible)
        {

            // Vérifie si le joueur regarde la silhouette. Si oui; on mémorise qu'il l'as déjà vue. 
            if (JoueurRegardeSilhouette())
            {
                silhouetteDejaVue = true;
            } 
            // Si le joueur a déjà vu la silhouette et que sa disparition au détournement est activée, elle disparaît dès qu'il détourne son regard.
            else if (silhouetteDejaVue && disparitionAuDetournement)
            {
                FaireDisparaitre();
                return;
            } 
            // Tente de déplacer la silhouette vers le joueur si son déplacement est autorisé.
            DeplacerSilhouette();
            
            // PEUR FAIBLE : La silhouette peut rester visible 4s.
            if(fearManager.niveauPeur < 0.3f)
            {
                dureeApparition = 4f;
            }
            // PEUR MOYENNE : La silhouette peur rester visible 2s.
            else if(fearManager.niveauPeur < 0.7f)
            {
                dureeApparition = 2f;
            }
            else
            // PEUR ÉLEVÉE : La silhouette peut rester visible 1s.
            {
                dureeApparition = 1f; 
            }
            // Incrémente le temps de visibilité.
            tempsVisible += Time.deltaTime;

            //Debug.Log("Temps visible : " + tempsVisible + " | Durée autorisée : " + dureeApparition);

            // Fait disparaître la silhouette lorsque sa durée maximale de visibilité est atteinte.
            if (tempsVisible >= dureeApparition)
            {
                FaireDisparaitre();
            }
        }
        // CAS 2 : La silhouette est actuellement invisible.
        else
        {
            // PEUR FAIBLE : Délai de réapparition comprise entre 4 et 7s.
            if(fearManager.niveauPeur < 0.3f)
            {
                delaiReapparition = Mathf.Lerp(4f,7f,variationAleatoire);
            }
            // PEUR MOYENNE : Délai de réapparition comprise entre 9 et 13s.
            else if(fearManager.niveauPeur < 0.7f)
            {
                delaiReapparition = Mathf.Lerp(9f,13f,variationAleatoire);
            }
            // PEUR ÉLEVÉE : Délai de réapparition comprise entre 18 et 25s.
            else
            {
                delaiReapparition = Mathf.Lerp(18f,25f,variationAleatoire);
            }

            // Incrémente le temps passé sans silhouette.
            tempsInvisible += Time.deltaTime;

            // Fait réapparaitre la silhouette lorsque le délai d'attente et atteint.
            if (tempsInvisible >= delaiReapparition)
            {
                FaireApparaitre();
            }
        }

        // Vérifie la proximité entre le joueur et la silhouette lorsqu'elle est active.
        if (silhouette.activeSelf && joueur != null)
        {
            // Calcule la distance entre les 2 objets.
            float distance = Vector3.Distance(
                joueur.position,
                silhouette.transform.position
            );

            // Si le joueur s'approche à 3m ou moins, la silhouette disparaît immédiatement.
            if (distance <= distanceDisparition)
            {
                FaireDisparaitre();
                Debug.Log("Silhouette disparue : joueur trop proche.");
            }
        }
    }

    // Gère l'apparition de la silhouette : position, durée de visibilité, comportement aléatoire et augmentation de la peur.
    void FaireApparaitre()
    {
        // Vérifie que des positions d'apparition existant.
        if (positionsApparition != null && positionsApparition.Length > 0)
        {
            // Sélectionne une position aléatoire dans la liste.
            int indexAleatoire = Random.Range(0, positionsApparition.Length);
            Transform pointChoisi = positionsApparition[indexAleatoire];

            // Place la silhouette à la position sélectionnée.
            if (pointChoisi != null)
            {
                silhouette.transform.position = pointChoisi.position;
            }
        }
        
        // Vérifie que la silhouette n'apparaît pas trop près du joueur. 
        if (joueur != null)
        {
            float distance = Vector3.Distance(
                joueur.position,
                silhouette.transform.position
            );

            // Si la position est trop proche, l'apparition est annulée et le temps d'attente est réinitialisé.
            if (distance <= distanceDisparition)
            {
                tempsInvisible = 0f;
                return;
            }
        }

        // Détermine la durée d'apparition en fonction du niveau de peur.
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

        // Reinitialise les variables de visibilité.
        tempsVisible = 0f;
        silhouetteDejaVue = false;

        // Définit aléatoirement si la silhouette disparaîtra lorsque le joueur détournera son regard avec une proba de 70%.
        disparitionAuDetournement = Random.value < 0.7f;
        silhouetteMobile = false;

        // Le déplacement n'est envisagé que si la disparition au détournement est désactivée.
        if (!disparitionAuDetournement)
        {

            // PEUR FAIBLE : 60% de chances de rendre la silhouette mobile.
            if (fearManager.niveauPeur < 0.3f)
            {
                silhouetteMobile = Random.value < 0.6f;
            }
            // PEUR MOYENNE : 30% de chances de rendre la silhouette mobile. 
            else if (fearManager.niveauPeur < 0.7f)
            {
                silhouetteMobile = Random.value < 0.3f;
            }
        }

        // Augmente et affiche la silhouette.
        silhouetteVisible = true;
        silhouette.SetActive(true);

        // Augmente la peur du joueur après l'apparition. L'augmentation effective dépend du mode simulationEvenements dans FearManager.
        fearManager.AugmenterPeur(augmentationPeur);
    }
    
    // Déplace la silhouette vers la joueur uniquement lorsqu'elle est mobile, non observée et qu'aucun obstacle ne bloque son chemin. 
    void DeplacerSilhouette()
    {
        // Annule le déplacement si la silhouette est immobile ou si la référence du joueur est absente.
        if (!silhouetteMobile || joueur == null)
        {
            return;
        }

        // La silhouette ne se déplace pas lorsque le joueur la regarde.
        if (JoueurRegardeSilhouette())
        {
            return;
        }

        // Calcule la direction vers le joueur.
        Vector3 direction = joueur.position - silhouette.transform.position;

        // Ignore la différence de hauteur pour que le déplacement reste horizontale.
        direction.y = 0f;

        // Calcule la distance horizontale au joueur.
        float distance = direction.magnitude;

        // La silhouette avance uniquement si elle se trouve au delà de sa distance d'arrêt.
        if (distance > distanceArret)
        {
            // Normalise la direction pour conserver une vitesse de déplacement constante.
            Vector3 directionNormalisee = direction.normalized;

            // Place l'origine du rayon légèrement au dessus du sol.
            Vector3 origineRayon = silhouette.transform.position + Vector3.up * 0.5f;

            // Lance un rayon de 0.7m vers le joueur pour détecter un obstacle sur les couches choisies. 
            bool obstacleDevant = Physics.Raycast(
                origineRayon,
                directionNormalisee,
                0.7f,
               couchesObstacles,
               QueryTriggerInteraction.Ignore
            );

            // Si aucun obstacle n'est détecté, déplace la silhouette vers le joueur.
            if (!obstacleDevant)
            {
                silhouette.transform.position += directionNormalisee * vitesseDeplacement * Time.deltaTime;
            }
        } 
    }

    // Détermine si le joueur regarde approximativement dans la direction de la silhouette.
    bool JoueurRegardeSilhouette()
    {
        // Vérifie que les références existent.
        if (cameraJoueur == null || silhouette == null)
        {
            return false;
        }

        // Calcule le vecteur allant de la caméra vers la silhouette.
        Vector3 directionSilhouette = silhouette.transform.position - cameraJoueur.position;

        // Calcule l'angle entre la direction du regard et la direction de la silhouette.
        float angle = Vector3.Angle(
            cameraJoueur.forward,
            directionSilhouette
        );
        
        // La silhouette est considérée comme regardée si elle se trouve a moins de 45 degrés de l'axe central de la caméra. 
        return angle < 45f;
    }
    
    // Fait disparaître la silhouette et reinitialise les compteurs pour préparer sa prochaine apparition.
    void FaireDisparaitre()
    {

        //Debug.Log("Disparition : peur = " + fearManager.niveauPeur + " | temps visible = " + tempsVisible + " | durée autorisée = " + dureeApparition);

        // Désactive l'objet silhouette dans la scène.
        silhouette.SetActive(false);

        // Indique que la silhouette n'est pas visible.
        silhouetteVisible = false;

        // Reinitialise les compteurs de temps.
        tempsVisible = 0f;
        tempsInvisible = 0f;

        // Génère une nouvelle valeur aléatoire pour le prochain délai de réapparition.
        variationAleatoire = Random.Range(0f,1f);
         
    }
}
