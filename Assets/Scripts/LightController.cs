using UnityEngine;
using System.Collections;
/* 
    LightController.cs
    Rôle : Gère l'éclairage et le clignotement de la lampe en fonction du niveau de peur 
    du joueur.
    Le comportement de la lampe s'adapte a 3 niveaux : 
        - Peur faible (< 0.3) : clignotement fréquents.
        - Peur moyenne (0.3 à 0.6) : clignotement plus espacés.
        - Peur élevée (>= 0.7) : lumière stable pour rassurer le joueur. 
    Le script permet également de déclencher un clignotement ponctuel lors d'un évènement 
    particulier du jeu.
*/ 

public class LightController : MonoBehaviour
{
    // Référence à la lampe controlée par le script.
    public Light lampe;
    
    //Référence au gestionnaire du niveau de peur. 
    public FearManager fearManager;

    // Intensité de la lampe lorsqu'elle est allumée.
    public float intensiteNormale = 2f;

    //Intervalle de temps entre deux changements d'état de la lampe (allumé/ éteinte).
    public float intervalle = 0.5f;

    // Seuil à partir duquel est considéré comme élevée.
    public float seuilPeurElevee = 0.7f;

    // Seuil à partir duquel est considéré comme faible.
    public float seuilPeurFaible = 0.3f;

    
    // Compteur permettant de mesurer le temps écoulé depuis le dernier changement d'état de la lampe.
    private float chronometre = 0f;

    // Indique si la lampe est actuellement allumée.
    private bool lampeAllumee = true; 

    // Indique si un clignotement ponctuel est en cours. Permet d'éviter les conflits avec le cignotement normal.
    private bool clignotementEvenementiel = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // Si un clignotement évènementiel est en cours, on suspend de façon temporaire le contrôle habituel.
        if (clignotementEvenementiel)
        {
            return;
        }
        // PEUR ÉLEVÉE : La lampe reste allumée en permanence afin de réduire l'intensite de l'environnement horrifique.   
        if(fearManager.niveauPeur >= seuilPeurElevee)
        {
            lampe.intensity = intensiteNormale;
            lampeAllumee = true;
            chronometre = 0f;
            return; 
        }
        // Incrémente le chronomètre avec le temps écoulé depuis la dernière frame.
        chronometre += Time.deltaTime;

        // Alterne entre lampe allumée et eteinte. 
        if (chronometre >= intervalle)
        {
            // Alterne entre lampe allumée et eteinte.
            lampeAllumee = !lampeAllumee;

            // Applique l'intensité normale si la lampe est allumée, sinon une intensité nulle.
            lampe.intensity = lampeAllumee ? intensiteNormale : 0f;

            // Reinitialise le chronomètre. 
            chronometre = 0f;

            // PEUR FAIBLE : Les changements d'état sont fréquents avec un intervalle aléatoire entre 0.5 et 1.5s.
            if (fearManager.niveauPeur < seuilPeurFaible)
            {
                intervalle = Random.Range(0.5f,1.5f);
            }
            // PEUR MOYENNE : Les changements d'état sont plus espacées avec un intervalle aléatoire entre 2 et 4s.
            else
            {
                intervalle = Random.Range(2f, 4f);
            }
        }        
    }
    // Méthode publique permattant à un autre script de déclancher un clignotement ponctuel.
    public void DeclencherClignotment()
    {
        // Empêche le lancement de plusieurs clignotements évènementiels en même temps.
        if (!clignotementEvenementiel)
        {
            StartCoroutine(ClignotementPonctuel());
        }
    }

    // Couroutine réalisant une extinction temporaire de la lampe pendant 0.2s.
    IEnumerator ClignotementPonctuel()
    {
        // Signale qu'un clignotement évènementiel est en cours.
        clignotementEvenementiel = true;

        // Sauvegarde l'intensité actuelle de la lampe.
        float intensiteInitiale = lampe.intensity;

        // Éteint temporairement la lampe. 
        lampe.intensity = 0f;

        // Attend 0.2s seconde avant de restaurer l'éclairage.
        yield return new WaitForSeconds(0.2f);

        //Restaure l'intensité précédante.
        lampe.intensity = intensiteInitiale;

        // Autorise à nouveau le clignotement habituel.
        clignotementEvenementiel = false;
    }
}
