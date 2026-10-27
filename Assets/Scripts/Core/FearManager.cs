using UnityEngine;
/*
    FearManager.cs
    Role : Gère le niveau de peur du joueur, compris entre 0 et 1.
    Ce niveau de peur est utilisé par les autres fichier pour adapter de façon dynamique l'environnement du jeu.
    (Lumière, brouillard, sons, silhouette, porte ect...).
    Deux styles de simulation sont disponibles : 
        - Simulation automatique : la peur varie avec une sinusoïde.
        - Simulation par évènements : la peur augmente lorsqu'un évènement se produit et diminue avec le temps.
*/

public class FearManager : MonoBehaviour
{
    // Niveau de peur actuel du joueur : 0 = aucune peur, 1 = peur maximale.
    [Range(0f, 1f)]
    public float niveauPeur = 0f;

    // Active la simulation automatique de la peur. 
    public bool simulationActive = false;

    // Active la simulation basée sur les évènements du jeu.
    public bool simulationEvenements = false;

    // Vitesse à laquelle la peur diminue progressivement lorque la simulation par évènements est activée.
    public float vitesseDiminutionPeur = 0.05f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        /* MODE 1 : Simulation automatique 
        Si la simulation automatique est activée et que le mode évènements est désactivé, la peur varie continuellement entre 0 et 1 grâce a une sinusoïde. */
       if (simulationActive && !simulationEvenements)
        {
            niveauPeur = (Mathf.Sin(Time.time * 0.5f) + 1f) / 2f;
        } 
        /* MODE 2 : Simulation par évènements
        La peur diminue progressivement vers 0. Time.deltaTime permet une diminution indépendante du nombre d'images affichées par seconde. */
        else if (simulationEvenements)
        {
            niveauPeur = Mathf.MoveTowards(
                niveauPeur,
                0f,
                vitesseDiminutionPeur * Time.deltaTime
            ); 
        }
    }
    // Augmente le niveau de peur d'une quantité donné. Cette méthode est appelé par les évènements du jeu (par exemple un bruit soudain ou une silhouette). 
    public void AugmenterPeur(float quantite)
    {
        // L'augmentation est autorisé uniquement lorsque la simulation par évènemenets est activée
        if (simulationEvenements)
        {
            // Ajoute la quantité demandée au niveau actuel. Mathf.Clamp01 que la valeur soit entre 0 et 1.
            niveauPeur = Mathf.Clamp01(niveauPeur + quantite);
        }
    }
}
