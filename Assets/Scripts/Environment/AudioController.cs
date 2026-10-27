using UnityEngine;
/*
    AudioController.cs
    Rôle : Gère le volume de l'ambiance sonore du jeu en fonction du niveau de peur du 
    joueur.
    Le volume s'adapte à 3 niveaux :
        - Peur faible (< 0.3) : Volume élevé (0.8).
        - Peur moyenne (0.3 à 0.6) : Volume modérée (0.5).
        - Peur élevé (>= 0.7) : Volume faible (0.2).
    
    L'objectif est de renforcer l'ambiance horrifique lorsque le joueur est calme et de 
    réduire l'intensité sonore lorsque son niveau de peur devient trop important.
    Les changements de volume sont progressif afin d'éviter des variations sonore trop 
    brusques.
*/

public class AudioController : MonoBehaviour
{
    // Composant AudioSource utilisé pour diffuser l'ambiance sonore du jeu.
    public AudioSource audioSource;

    // Référence au gestionnaire de niveau de peur. Permet de récupérer la peur actuelle du joueur.
    public FearManager fearManager;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Récupère le composant AudioSource présent sur le même GameObject.
        audioSource = GetComponent<AudioSource>();

        // Lance la lecture de l'ambiance sonore. 
        audioSource.Play();
    }

    // Update is called once per frame
    void Update()
    {
        // Volume que l'ambiance sonoore doit progressivement atteindre.
        float volumeCible;

        // PEUR FAIBLE : Le volume est élevé afin de renforcer l'immersion et l'ambiance horrifique.
        if (fearManager.niveauPeur < 0.3f)
        {
            volumeCible = 0.8f;
        }
        // PEUR MOYENNE : Le volume est réduit pour diminuer légérement l'intensité de l'ambiance.
        else if (fearManager.niveauPeur < 0.7f)
        {
            volumeCible = 0.5f;
        }
        // PEUR ÉLEVÉE : Le volume devient faible afin de limiter la tension sonore ressentie par le joueur.
        else
        {
            volumeCible = 0.2f;
        }
        //Modifie progressivement le volume actuel vers le volume cible. Mathf.MoveTowards permet une transition fluide. La vitesse de changement est de 0.2 par s. Time.deltaTime rend cette transition indépendante du nombre de fps. 
        audioSource.volume = Mathf.MoveTowards(
            audioSource.volume,
            volumeCible,
            0.2f * Time.deltaTime
        );
    }
}
