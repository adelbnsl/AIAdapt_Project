using System;
using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
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
    }

    // Update is called once per frame
    void Update()
    {
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
            reaction.activationInitiale = activationAvantEvenement;
            reaction.picActivation = picVariationActivation;
            reaction.dureeObservation = tempsDepuisEvenement;

            // Une observation commencée pendant une autre n'est pas considérée comme isolée.
            reaction.reactionIsolee = !observationPertubee;
            reaction.observationInterrompue = false;

            // Ajoute la réaction à l'historique du joueur.
            historiqueReactions.Add(reaction);

            Debug.Log("UserModel : Observation terminée pour " + dernierEvenement + " | Pic d'activation : " + picVariationActivation + " | Isolée : " + reaction.reactionIsolee);
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
            reactionInterrompue.activationInitiale = activationAvantEvenement;
            reactionInterrompue.picActivation = picVariationActivation;
            reactionInterrompue.dureeObservation = Mathf.Max(0f, evenement.temps - tempsDernierEvenenement);

            // L'observation précédente est interrompue par le nouvel évènement et n'est donc pas isolée.
            reactionInterrompue.reactionIsolee = false;
            reactionInterrompue.observationInterrompue = true;
            historiqueReactions.Add(reactionInterrompue);
        }
        // Verifie si le nouvel évènement interrompt une observation qui n'est pas encore terminée.
        bool evenementChevauchant = observationEnCours;

        // Enrengistre le nouvel évènement.
        dernierEvenement = evenement.id;
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
}
