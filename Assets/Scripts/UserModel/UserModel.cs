using System;
using NUnit.Framework;
using UnityEngine;
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
        activationEDA = Mathf.Clamp01(variationEDA / 3f);
        activationBPM = Mathf.Clamp01(variationBPM / 25f);

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
    }
}
