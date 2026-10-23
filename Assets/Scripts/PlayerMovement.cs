
using UnityEngine;
/* 
    PlayerMovement.cs
    Rôle : Gère les déplacements du joueur dans l'environnement à l'aide des touches du clavier.
    Fonctionnalités principales :
        - Déplacement vers l'avant et vers l'arrière.
        - Déplacement vers latéral vers la gauche et la droite.
        - Déplacement selon l'orientation actuelle du joueur.
        - Réglage de la vitesse de déplacement.
        - Utilisation du CharacterController pour gérer les déplacements et les collisions.
    Ce script fonctionne avec PlayerLook.cs :
        - PlayerLook gère l'orientation du joueur.
        - PlayerMovement gère ses déplacement.
    L'objectif est de permettre au joueur d'explorer librement le couloir dans une vue à la 1ère personne.
*/
public class PlayerMovement : MonoBehaviour
{
    // Vitesse de déplacement du joueur. La valeur par défaut est de 3 unité/s.
    public float vitesse = 3f;

    // Référence au composant CharacterController. Utilisé pour déplacer le joueur tout en tenant compte des collisions avec l'environnement.
    private CharacterController controller;

    void Start()
    {
        // Récupère le CharacterController présent sur le même GameObject que ce script.
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        // Récupère les entrées horizontales du clavier. Valeur négative : déplacement vers la gauche. Valeur positive : déplacement vers la droite.
        float horizontal = Input.GetAxis("Horizontal");

        // Récupère les entrées verticales du clavier. Valeur positive : déplacement vers l'avant. Valeur négative : déplacement vers l'arrière.
        float vertical = Input.GetAxis("Vertical");

        // Calcule la direction du joueur. transform.right correspond à l'axe droite/gauche selon l'orientation actuelle du joueur. transform.forward correspond à l'axe avant/arrière selon l'orientation actuelle du joueur. Les deux directions sont combinées pour permettre les déplacement en diagonale.
        Vector3 direction = transform.right * horizontal
                          + transform.forward * vertical;

        // Déplace le joueur dans la direction calculée. La direction est multipliée par la vitesse. Time.deltaTime permet d'adapter le déplacement au temps écoulé entre 2 images. CharacterController.Move prend en compte les collisions avec les éléments du décors. 
        controller.Move(direction * vitesse * Time.deltaTime);
    }
}
