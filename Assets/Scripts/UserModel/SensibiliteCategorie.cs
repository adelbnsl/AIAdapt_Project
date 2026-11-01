using System;
using NUnit.Framework;
using UnityEngine;

/* 
    SensibiliteCategorie.cs
    Rôle : Représente la sensibilité physiologique estimée du joueur à une catégorie d'évènement horrifiques.
    Fonctionnement : 
        - Identifie une catégorie d'évènements.
        - Mémorise le nombre de réactions exploitables.
        - Conserve le pic d'activation moyen observé.
        - Servira à construire progressivement le profil de sensibilité du joueur.
    Remarques :
        - Les valeurs seront calculées à partir de l'historique des réaction du UserModel.
        - Une activation physiologique élevée ne signifie pas nécessairement que le joueur ressent de la peur.
        - Cette classe n'hérite pas de MonoBehaviour.
*/

[Serializable]
public class SensibiliteCategorie
{
    // Catégorie d'évènements analysée.
    public EventCategory categorie;

    // Nombre de réactions potentiellement exploitables utilisées pour calculer cette sensibilité.
    public int nombreReactions = 0;

    // Moyenne des pics d'activation physiologique observées pour cette catégorie.
    [UnityEngine.Range(0f, 1f)]
    public float activationMoyenne = 0f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
