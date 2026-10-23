using UnityEngine;

public class ObjetMobileController : MonoBehaviour
{
    public FearManager fearManager;
    public Vector3 positionInitiale;
    public Vector3 positionAlternative; 
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void ChangerPosition(int numeroBoucle)
    {
        if (fearManager == null)
        {
            return;
        }
        float peur = fearManager.niveauPeur;
        float probabiliteDeplacement;
        if (peur < 0.3f)
        {
            probabiliteDeplacement = 0.8f;
        }
        else if (peur < 0.7f)
        {
            probabiliteDeplacement = 0.4f;
        }
        else
        {
            probabiliteDeplacement = 0f;
        }

        if (Random.value < probabiliteDeplacement) {
            if (numeroBoucle % 2 == 0)
            {
                transform.position = positionInitiale;
            }
            else
            {
                transform.position = positionAlternative;
            }
            Debug.Log("La chaise a changé de position à la boucle " + numeroBoucle);
        }
    }
}
