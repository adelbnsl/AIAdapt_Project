using System;
using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;
/* 
    UserModel.cs
    Rôle : Représente le modèle utilisateur du Projet. Son objectif est d'estimer le niveau de peur du joueur à partir des données physiologiques provenant des capteurs.
    Fonctionnalités principales : 
        - Récupération des mesures physiologiques (EDA et BPM).
        - Estimation d'un niveau de peur compris entre 0 et 1.
        - Mise à jour progressive de cette estimation.
        - Communication avec FearManager pour permettre l'adaptation de l'environnement horrifique.
    Données utilisées : 
        - EDA : activité électrodermale, exprimée en microsiemmens (µS).
        - BPM : fréquence cardiaque, exprimée en battement par minute.
    Fonctionnement prévu : 
        - Lire les données fournies par une source de capteurs.
        - Comparer les mesures aux valeurs de repos du joueur.
        - Calculer une estimation de son niveau de peur.
        - Lisser cette estimation pour éviter les variations brutales. 
        - Transmettre le résultat au gestionnaire de peur.
    Remarques :
        - Les capteurs sont actuellement simulés.
        - Le modèle pourra ensuite être adapté à des capteurs réels.
        - Les mesures physiologiques permettent d'estimer une réaction ou un état d'activation, mais ne prouvent pas à elles seules que le joueur ressent de la peur 
    Version actuelle : 
        Structure initiale et connexion à la source de capteurs. Le calcul du niveau de peur sera ajouté progressivement.
*/


public class UserModel : MonoBehaviour
{
    // Composant Unity fournissant les données physiologiques. Il doit implémenter l'interface ISensorSource.
    public MonoBehaviour sourceCapteur;
    
    // Interface utilisée pour lire les mesures des capteurs.
    private ISensorSource capteur;

    // Estimation actuelle du niveau peur du joueur. 0 = minimale, 1 = maximale.
    [UnityEngine.Range(0f, 1f)]
    public float niveauPeurEstime = 0f;

    // Dernière mesure d'activité électrodermale reçue du capteur (µS).
    public float edaActuelle = 0f;

    // Dernière mesure de fréquence cardiaque reçue du capteur (BPM).
    public float bpmActuel = 0f;

    // Valeur de référence de l'activité électrodermale du joueur. Elle représente son niveau EDA au repos, en µS.
    public float edaRepos = 2f;

    // Valeur de référence de la fréquence cardiaque du joueur. Elle représente son rythme cardiaque au repos, en BPM.
    public float bpmRepos = 70f;

    // Activation physiologique calculée à partir de l'EDA. 0 = Aucune augmentation par rapport au repos. 1 = Augmentation atteignant ou dépassant le seuil choisi.
    [UnityEngine.Range(0f, 1f)]
    public float activationEDA = 0f;

    // Activation physiologique calculée à partir du BPM. 0 = Aucune augmentation par rapport au repos. 1 = Augmentation atteignant ou dépassant le seuil choisi.
    [UnityEngine.Range(0f, 1f)]
    public float activationBPM = 0f;

    // Poids atribué à l'activation électrodermale. Il dertermine son importance dans le score global.
    [UnityEngine.Range(0f, 1f)]
    public float poidsEDA = 0.6f;

    // Poids atribué à l'activation cardiaque. Il dertermine son importance dans le score global.
    [UnityEngine.Range(0f, 1f)]
    public float poidsBPM = 0.4f; 

    // Poids attribué à l'activation physiologique du joueur. 0 = Aucune augmentation détectée par rapport au repos. 1 = Actvation maximale selon notre normalisation. 
    [UnityEngine.Range(0f, 1f)]
    public float activationGlobale = 0f;

    // Vitesse à laquelle le score lissé rejoint l'activation globale. Une valeur élevée entraîne une réaction plus rapide. Une valeur faible produit une évolution plus progressive.
    [Min(0f)]
    public float vitesseLissage = 2f;

    // Score d'activation physiologique après lissage temporel. Cette valeur servira de base au futur modèle d'estimation.
    // Poids atribué à l'activation électrodermale. Il dertermine son importance dans le score global.
    [UnityEngine.Range(0f, 1f)]
    public float activationLisse = 0f;

    // Identifiant du dernier évènement horrifique reçu. Permet de connaître le stimulus le plus récent.
    public string dernierEvenement = "Aucun";

    // Categorie(s) du dernier évènement horrifique reçu. Cette information sera conservée dans l'historique pour construire le profil de sensibilité du joueur.
    public EventCategory categoriesDernierEvenement;

    // Temps Unity auquel le dernier évènement a été reçu. Sers à analyser les réactions physiologiques après un stimulus. -1 signifie qu'aucun évènement n'a été reçu.
    public float tempsDernierEvenenement = -1f;

    // Temps écoulé depuis le dernier évènement horrifique. -1 signifie qu'aucun évènement n'as encore été reçu.
    public float tempsDepuisEvenement = -1f; 

    // Activation physiologique lissée enrengistrée au moment de la réception dernier évènement horrifique. Sert de référence pour mesurer les variation après l'évènement.
    [UnityEngine.Range(0f, 1f)]
    public float activationAvantEvenement = 0f;

    // Variation de l'activation physiologique depuis le dernier évènement. Une valeur positive montre une augmentation de l'activation. Une valeur négative montre une diminution. Cette variation ne prouve pas toute seule que l'évènement est responsable de la réaction montrée.
    [UnityEngine.Range(-1f, 1f)]
    public float variationActivation = 0f;

    // Plus forte augmentation d'activation physiologique observée depuis le derner évènement horrifique. Cette valeur reste mémorisée jusqu'au prochain évènement.
    [UnityEngine.Range(0f, 1f)]
    public float picVariationActivation = 0f;

    // Durée pendant laquelle le modèle observe la réaction physiologique après un évènement horrifique. Après cette durée, le pic d'activation n'est plus actualisé. 
    [Min(0f)]
    public float duréeObservation = 6f;

    // Historique des réactions physiologique du joueur. Chaque élement correspond à une occurence d'un évènement horrifique et aux mesures observées après cet évènement. Cette liste permet de construre progressivement un profil de sensibilité aux différents stimulis.
    public List<ReactionEvenement> historiqueReactions = new List<ReactionEvenement>();

    // Profil de sensibilité physiologique du joueur. Chaque élement correspond à une catégorie d'évènemenns horrifiques et contient les indicateurs calculés à partir des réactions potentiellement exploitables.
    // Cette liste permettra au système d'adaptation de connaître les catégories auxquelle le joueur réagit le plus.
    public List<SensibiliteCategorie> profilSensibilites = new List<SensibiliteCategorie>();

    // Catégorie d'évènements associée à la plus forte activation physiologique moyenne du joueur.
    public EventCategory categoriePlusForteActivation = (EventCategory)0;

    // Valeur de l'activation moyenne de cette catégorie. Compris entre 0 et 1.
    [UnityEngine.Range(0f, 1f)]
    public float plusForteActivationMoyenne = 0f;

    // Catégorie d'évènements associée à la plus faible activation physiologique moyenne du joueur.
    public EventCategory categoriePlusFaibleActivation = (EventCategory)0;

    // Valeur de l'activation moyenne de cette catégorie. Compris entre 0 et 1.
    [UnityEngine.Range(0f, 1f)]
    public float plusFaibleActivationMoyenne = 0f;

    // Nombre minimal de réactions exploitables nécessaires pour prendre en compte une catégorie dans la comparaison des activations physiologiques moyennes. Ce seuil est provisoire et pourra être ajusté après évaluation.
    [Min(1)]
    public int nombreMinimuReactions = 3;

    // Nombre de réactions enrengistrées dans l'historique qui respectent les critères initaux d'exploitation. Cette valeur est actualisée après chaque enrengistrement.
    public int nombreReactionExploitables = 0;

    // Indique si une fenêtre d'observation physiologique est en cours. Cette variable permet de détecter la fin d'une observation et d'éviter de traiter plusieurs fois le même évènement.
    public bool observationEnCours = false;

    // Indique si l'observation actuelle a commencé alors qu'une autre observation était encore en cours. Une telle observation peut être influencée par plusieurs évènements horrifiques rapprochés.
    public bool observationPertubee = false;

    // Variation EDA de référence utilisée pour la normalisation. Une augmentation de cette amplitute correspond à une activation de 1.
    [Min(0.001f)]
    public float amplitudeReferenceEDA = 3f;

    // Variation BPM de référence utilisée pour la normalisation. Une augmentation de cette amplitute correspond à une activation de 1.
    [Min(0.001f)]
    public float amplitudeReferenceBPM = 25f;



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Récupère la source des capteurs.
        capteur = sourceCapteur as ISensorSource;

        // Vérifie qu'une source compatible a été configurée.
        if (capteur == null)
        {
            Debug.LogError("UserModel : Aucune source de capteurs valide n'est assignée.");
        }

        // Prépare les défférentes catégories du profil utilisateur.
        InitialiserProfilSensibilites();   
    }

    // Update is called once per frame
    void Update()
    {
        // TEST TEMPORAIRE : vérifie la touche T
        // avec le nouveau Input System.
        if (Keyboard.current != null && Keyboard.current.tKey.wasPressedThisFrame)
        {
            Debug.Log("TEST : T détectée avec le nouveau Input System !");

            EventBus.Publish(
                "test_categories",
                EventCategory.Son | EventCategory.Derriere,
                0.5f
            );
        }

        // TEST TEMPORAIRE : déclenche un événement
        // appartenant aux catégories Apparition et Entite.
        if (Keyboard.current != null && Keyboard.current.yKey.wasPressedThisFrame)
        {
            Debug.Log("TEST : Événement Apparition déclenché !");

            EventBus.Publish(
                "test_apparition",
                EventCategory.Apparition | EventCategory.Entite,
                0.8f
            );
}
        
        
        // Verifie que la source des capteurs est disponible.
        if (capteur == null)
        {
            return;
        }
        

        // Récupère les dernières mesures physiologiques.
        SensorSample mesures = capteur.Lire();

        // Enrengistre les mesures dans le modèle utilisateur.
        edaActuelle = mesures.eda;
        bpmActuel = mesures.bpm;

        // Cacule l'augmentation de l'EDA par rapport au repos.
        float variationEDA = edaActuelle - edaRepos;

        // Calcule l'augmentation du BPM par rapport au repos.
        float variationBPM = bpmActuel - bpmRepos;

        // Normalise les variations entre 0 et 1.
        activationEDA = Mathf.Clamp01(variationEDA / Mathf.Max(0.001f, amplitudeReferenceEDA));
        activationBPM = Mathf.Clamp01(variationBPM / Mathf.Max(0.001f, amplitudeReferenceBPM));

        // Calcule un score global à partir des 2 indicateurs. Les poids sont normalisés pour que leur somme soit égale à 1.
        float sommePoids = poidsEDA + poidsBPM;
        if (sommePoids > 0f)
        {
            activationGlobale = Mathf.Clamp01((poidsEDA * activationEDA + poidsBPM * activationBPM) / sommePoids);
        }
        else
        {
            activationGlobale = 0f;
        }

        // Calcule un coefficient de lissage indépendant de la fréquence d'affichage du jeu. 
        float coefficientLissage = 1f - Mathf.Exp(-vitesseLissage * Time.deltaTime);

        // Fait évoluer progressivement le score lissé vers l'activation physiologique globale.
        activationLisse = Mathf.Lerp(activationLisse, activationGlobale, coefficientLissage );

        // Calcule le temps écoulé depuis le dernier évènement horrifique. Et calcule la variation d'activation depuis le dernier évènement. Aucun calcul n'est effectué si aucun évènement n'as été reçu.
        if (tempsDernierEvenenement >= 0f)
        {
            tempsDepuisEvenement = Mathf.Max(0f, Time.time - tempsDernierEvenenement);
            variationActivation = activationLisse - activationAvantEvenement;

            // Actualise le pic uniquement pendant la fenêtre d'observation.
            if (tempsDepuisEvenement <= duréeObservation)
            {
                // Conserve la plus forte augmentation observée.
                picVariationActivation = Mathf.Max(picVariationActivation, variationActivation);  
            }
        }
        else
        {
            tempsDepuisEvenement = -1f;
            variationActivation = 0f;
        }

        // Vérifie si la fenêtre d'observation est terminée. La condition n'est déclenchée qu'une seule fois grâce à observationEnCours
        if (observationEnCours && tempsDepuisEvenement >= duréeObservation)
        {
            observationEnCours = false;

            // Crée un objet contenant les mesures de la réaction.
            ReactionEvenement reaction = new ReactionEvenement();
            reaction.idEvenement = dernierEvenement;
            reaction.tempsEvenement = tempsDernierEvenenement;
            reaction.categoriesEvenement = categoriesDernierEvenement;
            reaction.activationInitiale = activationAvantEvenement;
            reaction.picActivation = picVariationActivation;
            reaction.dureeObservation = tempsDepuisEvenement;

            // Une observation commencée pendant une autre n'est pas considérée comme isolée.
            reaction.reactionIsolee = !observationPertubee;
            reaction.observationInterrompue = false;

            // Ajoute la réaction à l'historique du joueur.
            historiqueReactions.Add(reaction);

            Debug.Log("UserModel : Observation terminée pour " + dernierEvenement + " | Pic d'activation : " + picVariationActivation + " | Isolée : " + reaction.reactionIsolee);
            
            CompterReactionsExploitables();
            ActualiserProfilSensibilites();
            IdentifierCategoriePlusForteActivation();
            IdentifierCategoriePlusFaibleActivation();
        }
    }

    // Enrengistre le dernier évènement horrifique ainsi que l'état d'activation physiologique au moment de sa réception. Cette valeur sert de réference pour analyser la réaction du joueur dans les secondes qui suivent l'évènement. 
    void RecevoirEvenement(HorrorEvent evenement)
    {
        // Si une observation est encore en cours, conserve les données déjà mesurées avant de commencer l'observation du nouvel évènement.
        if (observationEnCours)
        {
            ReactionEvenement reactionInterrompue = new ReactionEvenement();
            reactionInterrompue.idEvenement = dernierEvenement;
            reactionInterrompue.tempsEvenement = tempsDernierEvenenement;
            reactionInterrompue.categoriesEvenement = categoriesDernierEvenement;
            reactionInterrompue.activationInitiale = activationAvantEvenement;
            reactionInterrompue.picActivation = picVariationActivation;
            reactionInterrompue.dureeObservation = Mathf.Max(0f, evenement.temps - tempsDernierEvenenement);

            // L'observation précédente est interrompue par le nouvel évènement et n'est donc pas isolée.
            reactionInterrompue.reactionIsolee = false;
            reactionInterrompue.observationInterrompue = true;
            historiqueReactions.Add(reactionInterrompue);
            CompterReactionsExploitables();
            ActualiserProfilSensibilites();
            IdentifierCategoriePlusForteActivation();
            IdentifierCategoriePlusFaibleActivation();
        }
        // Verifie si le nouvel évènement interrompt une observation qui n'est pas encore terminée.
        bool evenementChevauchant = observationEnCours;

        // Enrengistre le nouvel évènement.
        dernierEvenement = evenement.id;
        categoriesDernierEvenement = evenement.categories;
        tempsDernierEvenenement = evenement.temps;

        // Mémorise l'activation physiologique actuelle.
        activationAvantEvenement = activationLisse;

        // Reinitialise le pic pour observer la réaction au nouvel évènement horrfique. 
        picVariationActivation = 0f;

        // Signale si cette observation commence pendant une autre observation.
        observationPertubee = evenementChevauchant;

        // Démarre une nouvelle fenêtre d'observation pour le dernier évènement reçu.
        observationEnCours = true;
    }

    // Abonne le modèle utilisateur aux évènements horrifiques.
    void OnEnable()
    {
        EventBus.OnEvent += RecevoirEvenement;
    }

    // Désabonne le modèle pour éviter les abonements persistants.
    void OnDisable()
    {
        EventBus.OnEvent -= RecevoirEvenement;
    }

    // Parcout l'historique des réactions physiologiques et compte celles qui respectent les critères définis par la méthode EstExploitable.
    void CompterReactionsExploitables()
    {
        nombreReactionExploitables = 0;
        foreach(ReactionEvenement reaction in historiqueReactions)
        {
            if (reaction != null && reaction.EstExploitable())
            {
                nombreReactionExploitables++;
            }
        }
    }

    /* 
        Initialise le profil de sensibilité du joueur.
        Fonctionnement :
            - Vide les anciennes entrées du profils.
            - Parcourt les catégories définies dans EventCategory.
            - Crée une entrée SensibiliteCategorie pour chaque catégorie élémentaire.
            - Initialise les indicateurs à 0.
        Remarques : EventCategory utilise des flags. Nous ignorons dont les valeurs None et les éventuelles combinaisons de plusieurs catégories.
    */

    void InitialiserProfilSensibilites()
    {
        profilSensibilites.Clear();
        foreach(EventCategory categorie in Enum.GetValues(typeof(EventCategory)))
        {
            int valeur = (int)categorie;

            // Vérifie que la catégorie correspond à un seul flag.
            if (valeur == 0 || (valeur & (valeur - 1)) != 0)
            {
                continue;
            }
            SensibiliteCategorie sensibilite = new SensibiliteCategorie();

            sensibilite.categorie = categorie;
            sensibilite.nombreReactions = 0;
            sensibilite.activationMoyenne = 0f;

            profilSensibilites.Add(sensibilite);
        }
    }

     
    // Actualise le nombre de réactions exploitables pour chaque catégorie du profil utilisateur. Une réaction peut contribuer à plusieurs catégories puisque EventCategory utilise des flags.
    void ActualiserProfilSensibilites()
    {
        // Réinitialise les compteurs du profil. 
        foreach(SensibiliteCategorie sensibilite in profilSensibilites)
        {
            sensibilite.nombreReactions = 0;
            sensibilite.activationMoyenne = 0f;
        }

        // Parcourt toutes les reactions enrengistrées.
        foreach(ReactionEvenement reaction in historiqueReactions)
        {
            // Ignore les réactions absentes ou non exploitables.
            if (reaction == null || !reaction.EstExploitable())
            {
                continue;
            }

            // Vérifie chaque catégorie du profil.
            foreach(SensibiliteCategorie sensibilite in profilSensibilites)
            {
                // Vérifie si la réaction possède cette catégorie.
                if((reaction.categoriesEvenement & sensibilite.categorie) == sensibilite.categorie)
                {
                    sensibilite.nombreReactions++;
                    sensibilite.activationMoyenne += reaction.picActivation;
                }
            }
        } 

        // Calcule l'activation moyenne pour chaque catégorie.
        foreach(SensibiliteCategorie sensibilite in profilSensibilites)
        {
            // Evite une division par 0 si aucun réaction exploitable n'as été enrengistrée pour cette catégorie.
            if (sensibilite.nombreReactions > 0)
            {
                sensibilite.activationMoyenne /= sensibilite.nombreReactions;
            }
        }
    }

    // Identifie la catégorie d'évènements associée à la plus forte activation physiologique moyenne du joueur. Si plusieurs catégories possèdent exactement la même activation moyenne, la 1ère rencontrée est conservée.
    void IdentifierCategoriePlusForteActivation()
    {
        // Réinitialise les résultats.
        categoriePlusForteActivation = (EventCategory)0;
        plusForteActivationMoyenne = 0f;

        // Parcourt les catégories du profil utilisateur.
        foreach(SensibiliteCategorie sensibilite in profilSensibilites)
        {
            // Ignore les catégories qui ne possèdent pas suffisamment de réactions exploitables.
            if(sensibilite.nombreReactions < nombreMinimuReactions)
            {
                continue;
            }

            // Vérifie si cette catégorie possède une activation moyenne supérieure à la meilleure valeur actuelle.
            if (categoriePlusForteActivation == (EventCategory)0 || sensibilite.activationMoyenne > plusForteActivationMoyenne)
            {
                categoriePlusForteActivation = sensibilite.categorie;
                plusForteActivationMoyenne = sensibilite.activationMoyenne;
            }
        }
    }

    // Identifie la catégorie d'évènements associée à la plus faible activation physiologique moyenne du joueur. Le seuil minimal est défini par nombreMinimuReactions. En cas d'égalité, la 1ère catégorie rencontrée est conservée. Une faible activation ne sigifie pas nécessairement une absence de peur.
    void IdentifierCategoriePlusFaibleActivation()
    {
        // Rénitialise les résultats. 
        categoriePlusFaibleActivation = (EventCategory)0;
        plusFaibleActivationMoyenne = 0f;

        // Parcout les catégories du profil utilisateur.
        foreach(SensibiliteCategorie sensibilite in profilSensibilites)
        {
            // Ignore les catégories qui ne possèdent pas suffisamment de réactions exploitables.
            if(sensibilite.nombreReactions < nombreMinimuReactions)
            {
                continue;
            }

            // Sélectionne la 1ère catégorie admissible ou une catégorie ayant une activation moyenne plus faible.
            if(categoriePlusFaibleActivation == (EventCategory)0 || sensibilite.activationMoyenne < plusFaibleActivationMoyenne)
            {
                categoriePlusFaibleActivation = sensibilite.categorie;
                plusFaibleActivationMoyenne = sensibilite.activationMoyenne;
            }

        }
    }
}
