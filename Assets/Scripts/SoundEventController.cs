using UnityEngine;

public class SoundEventController : MonoBehaviour
{
    private AudioSource audioSource;
    public FearManager fearManager;
    public AudioSource audioSourceDerriere;
    public AudioClip sonPas;
    public AudioClip sonChuchotement;

    public LightController lightController;
    [Range(0f, 1f)]
    public float augmentationPeur = 0.2f;

    public bool declenchementAutomatique = false;

    public float delaiMinimum = 5f;
    public float delaiMaximum = 10f;
    public float delaiMinimumPeurMoyenne = 12f;
    public float delaiMaximumPeurMoyenne = 20f;
    
    private float chronometre = 0f;
    private float prochainDeclanchement;
    private int niveauPeurPrecedent = -1;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        prochainDeclanchement = CalculerDelai();
    }

    // Update is called once per frame
    void Update()
    {
        int categorieActuelle = ObtenirCategoriePeur();
        if (categorieActuelle != niveauPeurPrecedent)
        {
            niveauPeurPrecedent = categorieActuelle;
            prochainDeclanchement = CalculerDelai();
        }
        if (declenchementAutomatique && fearManager.niveauPeur < 0.7){
            chronometre += Time.deltaTime;
            if (chronometre >= prochainDeclanchement)
                {
                    JouerEvenementSonore();
                    fearManager.AugmenterPeur(augmentationPeur);

                    chronometre = 0f;
                    prochainDeclanchement = CalculerDelai();
                }
        }
    }
    float CalculerDelai()
    {
        if (fearManager.niveauPeur < 0.3f)
        {
            return Random.Range(delaiMinimum, delaiMaximum);
        }
        else
        {
            return Random.Range(delaiMinimumPeurMoyenne, delaiMaximumPeurMoyenne);
        }
    }
    int ObtenirCategoriePeur()
    {
        if (fearManager.niveauPeur < 0.3f)
        {
            return 0;
        }
        else if (fearManager.niveauPeur < 0.7f)
        {
            return 1;
        }
        else
        {
            return 2;
        }
    }
    void PositionnerSonDerriereJoueur()
    {
        if (audioSourceDerriere == null)
        {
            return;
        }
        float postitionX = Random.Range(-2f, 2f);
        float positionZ = Random.Range(-4f, -1.5f);
        audioSourceDerriere.transform.localPosition = new Vector3(
            postitionX,
            0f,
            positionZ
        );
    }
    void JouerEvenementSonore()
    {
        int evenement = Random.Range(0, 3);
        if (evenement == 0)
        {
            audioSource.Play();
            lightController.DeclencherClignotment();
        }
        else if (evenement == 1)
        {
            if (sonPas != null && audioSourceDerriere != null)
            {
                PositionnerSonDerriereJoueur();
                audioSourceDerriere.PlayOneShot(sonPas);
            }
        }
        else
        {
            if (sonChuchotement != null && audioSourceDerriere != null)
            {
                PositionnerSonDerriereJoueur();
                audioSourceDerriere.PlayOneShot(sonChuchotement);
            }
        }
    }
}
