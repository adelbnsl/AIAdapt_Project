using System;
using UnityEngine;
/*
    ReactionEvenement.cs
    Rôle : Représente une réaction physiologique observée après un évènement horrifique.
    Fonctionnalités principales :
        - Identifier l'évènement responsable de l'observation.
        - Conserver l'instant de déclenchement de l'évènement.
        - Mémoriser l'activation physiologique initiale.
        - Enrengistrer le pic de variation d'activation.
        - Conserver la durée réelle d'observation.
    Utilisation : Cette classe servira à construire un historique des réaction physiologiques du joueur. Chaque occurence d'un évènement pourra posséder sa propre réaction, même si plusieurs évènements partagent le même identifiant.
    Remarques : 
        - Le pic représente une augmentation d'activation physiologique, pas une mesure directe de la peur.
        - L'association entre un évènement et une réaction reste une hypothèse lorsque plusieurs stimulis se produisent à proxmité temporelle.
 */
[Serializable]
public class ReactionEvenement
{
    // Identifiant du stimulus horrifique observé.
    public string idEvenement;

    // Istant Unity auquel l'évènement s'est produit.
    public float tempsEvenement;

    // Activation physiologique au moment de l'évènement.
    public float activationInitiale;

    // Plus forte augmentation observée pendant la fenêtre.
    public float picActivation;

    // Durée effectivement observée pendant la fenêtre.
    public float dureeObservation;

    // Indique si la réaction peut être considérée comme isolée d'autres évènements pendant son observation.
    public bool reactionIsolee;

    // Indique si la fenêtre d'observation a été interrompue par un nouvel évènement avant sa durée prévue. Une réaction interrompue doit être distinguées d'une observation arrivée normalement à son terme.
    public bool observationInterrompue;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    /* Indique si la réaction satisfait les les critères initiaux permettant de l'utiliser dans le calcul du profil utilisateur. Une réaction est considérée comme exploitable si :
            -Elle n'as pas commencé pendant une autre observation.
            -Elle n'as pas été interrompue avant la fin de sa fenêtre.
        Attention : ces critères ne garantissent pas que la réaction physiologique soit causée uniquement par cet évènement.
    */
    public bool EstExploitable()
    {
        return reactionIsolee && !observationInterrompue;
    }
}
