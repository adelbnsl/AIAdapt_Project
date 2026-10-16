using UnityEngine;

public class AudioController : MonoBehaviour
{
    public AudioSource audioSource;

    public FearManager fearManager;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.Play();
    }

    // Update is called once per frame
    void Update()
    {
        float volumeCible;
        if (fearManager.niveauPeur < 0.3f)
        {
            volumeCible = 0.8f;
        }
        else if (fearManager.niveauPeur < 0.7f)
        {
            volumeCible = 0.5f;
        }
        else
        {
            volumeCible = 0.2f;
        }
        audioSource.volume = Mathf.MoveTowards(
            audioSource.volume,
            volumeCible,
            0.2f * Time.deltaTime
        );
    }
}
