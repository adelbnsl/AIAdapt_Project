using UnityEngine;
/* 
    PlayerLook.cs
    Rôle : Gère l'orientation de la caméra et du joueur à l'aide des mouvements de la souris.
    Fonctionnalités principales :
        - Rotation horizontales du personnage (gauche/droite).
        - Rotation verticale de la caméra (haut/bas).
        - Réglage de la sensibilité de la souris.
        - Limitation de la rotation verticale entre -90 et 90 degrés.
        - Verrouillage et masquage du curseur pendant le jeu.
    L'objectif est de permettre au joueur d'explorer librement le couloir dans une vue à la 1ère personne, tout en conservant des mouvements de caméra naturels.
*/


public class PlayerLook : MonoBehaviour
{
    // Sensibilité de la souris. Détermine la vitesse de rotation de la caméra et du personnage selon les mouvement de la souris.
    public float sensibilite = 2f;

    // Référence au corps du joueur. Utilisée pour effectuer la rotation horizontale de l'ensemble du personnage.
    public Transform playerBody;

    // Stock l'angle de rotation verticale de la caméra. Initialisé à 0 degré.
    private float rotationY;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Verouille le curseur au centre de la fenêtre du jeu. Cela évite qu'il sorte de l'écran pendant les mouvement.
        Cursor.lockState = CursorLockMode.Locked;

        // Masque le curseur pour améliorer l'immersion dans la vue à la 1ère personne.
        Cursor.visible = false;
    }

    // Update is called once per frame
    void Update()
    {
        // Récupère le mouvement horizontal de la souris. La sensibilité ajuste l'amplitude de la rotation. Time.deltaTime est appliqué au calcul.
        float sourisX = Input.GetAxis("Mouse X") * sensibilite;

        // Récupère le mouvement verticale de la souris.
        float sourisY = Input.GetAxis("Mouse Y") * sensibilite;

        // Met à jour l'angle verticale de la caméra. La soustraction permet d'orienter la caméra vers le haut quand la souris monte.
        rotationY -= sourisY;

        // Limite la rotation verticale entre -90 et 90 degrés. Empêche le joueur de faire tourner la caméra complètement sur elle-même verticalement.
        rotationY = Mathf.Clamp(rotationY, -90f, 90f);

        // Applique la rotation verticale uniquement à la caméra autour de son axe local Y.
        transform.localRotation = Quaternion.Euler(rotationY, 0f, 0f);

        // Applique la rotation horizontale au corps du joueur autour de l'axe verticale Y. Le personnage et sa caméra tournent ainsi ensemble.
        playerBody.Rotate(Vector3.up * sourisX);
    }
}
