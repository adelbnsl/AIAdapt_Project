using UnityEngine;

/*
    FogController.cs
    Rôle : Gère la densité du brouillard dans l'environnement en fonction du niveau de peur du
    joueur.
    Le brouillard s'adapte à 3 niveaux :
        - Peur faible (< 0.3) : Brouillard dense (0.12).
        - Peur moyenne (0.3 à 0.6) : Brouillard modéré (0.08).
        - Peur élevée (>= 0.7) : Brouillard léger (0.04).
    L'objectif est de rendre l'environnement plus visible lorsque le joueur ressent une peur 
    importante.
    Les changements de densité sont progressifs afin d'éviter des transitions visuelles trop 
    brusques.
*/

public class FogController : MonoBehaviour
{
    
    // Référence au gestionnaire du niveau de peur. Permet de récupérer la peur actuelle du joueur.
    public FearManager fearManager;

    // Update is called once per frame
    void Update()
    {
        if (fearManager == null) return;

        float peur = fearManager.niveauPeur;
        // Densité que le brouillard doit progressivement atteindre.
        float densiteCible;

        //PEUR FAIBLE : Le brouillard devient dense afin de réduire la visibilité et renforcer l'ambiance horrifique. 
        if (fearManager.niveauPeur < 0.3f) densiteCible = 0.12f;

        // PEUR MOYENNE : Le brouillard devient moins dense pour améliorer légérement la visibilité  du joueur.
        else if (fearManager.niveauPeur < 0.7f) densiteCible = 0.08f;
        
        // PEUR ÉLEVÉE : Le brouillard devient léger pour offir une meilleur visibilité et réduire la tension.
        else densiteCible = 0.04f;
        
        // Modifie progressivement la densité actuelle du brouillard vers la densité cible. Mathf.MoveTowards permet une transition fluide. La vitesse de changement est 0.02 par s. Time.deltaTime rend cette transition indépendante du nombre de fps.  
        RenderSettings.fogDensity = Mathf.MoveTowards(
            RenderSettings.fogDensity,
            densiteCible,
            0.02f * Time.deltaTime
        );
    }
}
