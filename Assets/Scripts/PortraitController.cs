using UnityEngine;
/* 
    PortraitController.cs
    Rôle : Gère l'apparence et les changements du portrait présent dans le couloir.
    Fonctionalités principales :
        - Changement de portrait selon le nombre de boucles.
        - Adaptation de la couloir du portrait au niveau de peur.
        - Modification de la couleur d'émission lumineuse.
        - Application d'un effet de pulsation lumineuse.
        - Retour au 1er portrait lorsque la peur est élevé.
    3 comportements sont utilisées : 
        - Peur faible (< 0.3) : teinte rougeâtre et pulsation rouge.
        - Peur moyenne (0.3 à 0.6) : teinte bleutée et pulsation bleue.
        - Peur élevée (>= 0.7) : couleur neutre, sans émission lumineuse.
    L'objectif est de rendre le portrait plus inquiétant lorsque le joueur est calme et de réduire les effets visuels lorsque son niveau de peur est trop élevée.
*/
public class PortraitController : MonoBehaviour
{
    // Tableau contenant les différentes textures pouvant être affichées sur le portrait.
    public Texture[] portraits;

    // Renderer de l'objet sur lequel le portrait est affiché. Permet de modifier sa texture et les couleurs du matériau.
    public Renderer renduPortrait;

    // Référence au gestionnaire du niveau de peur.
    public FearManager fearManager;

    // Vitesse de l'effet de pulsation lumineuse.
    public float vitessePulsation = 2f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // Vérifie que le gestionnaire de peur et le Renderer sont disponibles avant de faire les modification sur le portrait.
        if (fearManager == null || renduPortrait == null)
        {
            return;
        }

        // Récupère le niveau de peur actuel du joueur.
        float peur = fearManager.niveauPeur;

        // ADAPTATION DE LA COULEUR PRINCIPALE DU PORTRAIT.
        // PEUR FAIBLE : Applique une teinte légérement rougeâtre afin de renforcer l'ambiance inquietante.
        if (peur < 0.3f)
        {
            renduPortrait.material.SetColor(
                "_BaseColor",
                new Color(1f, 0.85f, 0.85f)
            );
        }
        
        // PEUR MOYENNE : Applique une teinte bleutée au portrait.
        else if (peur < 0.7f)
        {
            renduPortrait.material.SetColor(
                "_BaseColor",
                new Color(0.7f, 0.8f, 1f)
            );
        }
        
        // PEUR ÉLEVÉE : Restaure une couleur blanche neutre.
        else
        {
            renduPortrait.material.SetColor(
                "_BaseColor",
                Color.white
            );
        }

        // ADAPTATION DE LA COULEUR D'ÉMISSION LUMINEUSE.
        // Variable contenant la couleur de l'émission.
        Color couleurEmission;

        // PEUR FAIBLE : Utilise une émission rouge sombre.
        if (peur < 0.3f)
        {
            couleurEmission = new Color(0.25f, 0.02f, 0.02f);
        }

        // PEUR MOYENNE : Utilise une émission bleue sombre.
        else if (peur < 0.7f)
        {
            couleurEmission = new Color(0.02f, 0.04f, 0.12f);
        }

        // PEUR ÉLEVÉE : Désactive visuellement l'emission lumineur en utilisant une couleur noire.
        else 
        {
            couleurEmission = Color.black;
        }

        // GESTION DE L'EFFET DE PULSATION LUMINEUSE.
        // Intensité appliquée à la couleur d'émission. Par défaut, aucune variation n'est appliquée.
        float intensitePulsation = 1f;

        // La pulsation est active uniquement lorsque le niveau de peur est < à 0.7.
        if (peur < 0.7f)
        {
            // Récupère le temps écoulé depuis le démarrage du jeu.
            float temps = Time.time;

            // Calcule une pulsation à l'aide d'une sinusoïde. La 2nde sinusoïde introduit une légère variation dans le rythme de pulsation.
            float pulsation = Mathf.Sin(
                Time.time * vitessePulsation
                + 0.4f * Mathf.Sin(temps * 0.7f)
            );

            // Transforme la pulsation en un coefficient compris entre 0.7 et 1. Cela crée une variation lumineuse progressive.
            intensitePulsation = 0.85f + 0.15f * pulsation;

        }
        // Multiplie la couleur d'émission par l'intensité de pulsation calculée.
        couleurEmission *= intensitePulsation;

        // Active la fonctionnalité d'émission lumineuse sur le matériau du portrait.
        renduPortrait.material.EnableKeyword("_EMISSION");

        // Applique la couleur d'émission finale au matériau.
        renduPortrait.material.SetColor("_EmissionColor", couleurEmission);
    }

    // Méthode publique appelé par CouloirLoopController après chaque boucle. Sélectionne une nouvelle texture de portrait selon le nombre de boucles effectuées.
    public void ChangerPortrait(int numeroBoucle)
    {
        // Vérifie que le tableau contient des textures et que le Renderer est disponible. 
        if (portraits == null || portraits.Length == 0 || renduPortrait == null)
        {
            return;
        }

        // Calcule l'indice du portrait à afficher. Le modulo % permet de revenir au début du tableau lorsque toute les textures ont été utilisées.
        int indexPortrait = numeroBoucle % portraits.Length;

        // PEUR ÉLEVÉE : Force l'affichage du 1er portrait pour limiter les changement visuels inquétants.
        if (fearManager != null && fearManager.niveauPeur >= 0.7f)
        {
            indexPortrait = 0;
        }

        // Applique la texture sélectionnée au matériau. 
        renduPortrait.material.SetTexture("_BaseMap", portraits[indexPortrait]);
        Debug.Log("Boucle : " + numeroBoucle + " | Portrait sélectionné : " + indexPortrait);
    }
}
