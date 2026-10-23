using UnityEngine;

public class DoorController : MonoBehaviour
{
    public Transform joueur;
    public float distanceInteraction = 2f;
    public Transform pivotPorte;
    public FearManager fearManager;
    private bool porteOuverte = false;
    public float vitesseOuverture = 60f;
    public float angleOuverture = -90f;
    private float angleCible = 0f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float distance = Vector3.Distance(
            transform.position,
            joueur.position
        );
        if (fearManager.niveauPeur < 0.3f)
        {
            vitesseOuverture = 90f;
        }
        else if (fearManager.niveauPeur < 0.7f)
        {
            vitesseOuverture = 60f; 
        }
        else
        {
            vitesseOuverture = 30f;
        }
        if (distance <= distanceInteraction && Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("Le joueur intéragit la porte.");
            if (porteOuverte == false)
            {
                angleCible = angleOuverture;
                porteOuverte = true;
            }
            else
            {
                angleCible = 0f;
                porteOuverte = false;
            }
        }
        Quaternion rotationCible = Quaternion.Euler(0f, angleCible, 0f);
        
        pivotPorte.localRotation = Quaternion.RotateTowards(
            pivotPorte.localRotation,
            rotationCible,
            vitesseOuverture * Time.deltaTime
        );

    }
    public void FermerPorte()
    {
        porteOuverte = false;
        angleCible = 0f;
        pivotPorte.localRotation = Quaternion.Euler(0f, 0f, 0f);
    }
    public void OuvrirPorte()
    {
        porteOuverte = false;
        angleCible = angleOuverture;
        pivotPorte.localRotation = Quaternion.Euler(0f, angleOuverture, 0f);
    }
}
