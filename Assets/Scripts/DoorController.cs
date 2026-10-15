using UnityEngine;

public class DoorController : MonoBehaviour
{
    public Transform joueur;
    public float distanceInteraction = 2f;
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
        if (distance <= distanceInteraction)
        {
            Debug.Log("Le joueur est proche de la porte.");
        }

    }
}
