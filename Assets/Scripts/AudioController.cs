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
        if (fearManager.niveauPeur < 0.3f)
        {
            audioSource.volume = 0.8f;
        }
        else if (fearManager.niveauPeur < 0.7f)
        {
            audioSource.volume = 0.5f;
        }
        else
        {
            audioSource.volume = 0.2f;
        }
    }
}
