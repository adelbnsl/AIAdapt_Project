using System;
using System.Collections.Generic;
using UnityEngine;

/*
    SimulatedSensorSource.cs
    Rôle : Simule les capteurs physiologiques (EDA et fréquence cardiaque).
    Écoute les évènements publiés sur l'EventBus et simule des signaux qui réagissent comme ceux d'un vrai joueur.
    
    Pour chaque évènement :
        1. On calcule l'amplitude de la réaction : (sensibilité du joueur simulé * intensité de l'évènement).
        2. La réaction est mise en attente pendant une latence
           (1.5s pour l'EDA, 0.5s pour le cœur, comme pour des vrais capteurs).
        3. Une fois appliquée, la réaction décroît progressivement vers la valeur de repos.
        4. Un léger bruit est ajouté pour que le signal ne soit pas parfaitement propre.
        
    Le script implémente ISensorSource : le reste du projet lit les signaux via Lire() sans savoir s'ils sont simulés ou réels. 
    Les vrais capteurs pourront remplacer cette classe sans être modifiée.
    Touches : 1 = joueur peu sensible, 2 = normal, 3 = très sensible, G = pic parasite.
*/

public class SimulatedSensorSource : MonoBehaviour, ISensorSource
{
    public string Nom => "Simulé";

    // Tonique : le niveau de fond de l'activité physiologique
    [Header("Valeurs de repos")]
    public float edaRepos = 2f;     // µS, microsiemens
    public float bpmRepos = 70f;    // bpm, battements par minutes

    [Header("Amplitude maximale d'une réaction")]
    public float gainEda = 3f;      // µS ajoutés pour une réaction de 1.0
    public float gainBpm = 25f;     // bpm ajoutés pour une réaction de 1.0

    [Header("Dynamique (secondes)")]
    public float latenceEda = 1.5f;
    public float constanteEda = 4f;     // temps de décroissance
    public float latenceBpm = 0.5f;
    public float constanteBpm = 6f;
    public float lissage = 0.5f;        // lissage de la montée

    [Header("Bruit")]
    public float bruitEda = 0.05f;
    public float bruitBpm = 1.5f;

    // ------------------------------------------------------------------------------------------
    // JOUEUR SIMULÉ : ici sensible aux sons et aux apparitions, mais peu sensible à la lumière.
    // ------------------------------------------------------------------------------------------
    [Header("Joueur simulé")]
    [Range(0f, 2f)] public float facteurSensibilite = 1f;
    [Range(0f, 1f)] public float sensibiliteSon = 0.8f;
    [Range(0f, 1f)] public float sensibiliteLumiere = 0.2f;
    [Range(0f, 1f)] public float sensibiliteObscurite = 0.3f;
    [Range(0f, 1f)] public float sensibiliteApparition = 0.9f;
    [Range(0f, 1f)] public float sensibiliteEntite = 0.5f;
    [Range(0f, 1f)] public float sensibiliteMouvement = 0.4f;
    [Range(0f, 1f)] public float sensibiliteDerriere = 0.6f;
    [Range(0f, 1f)] public float sensibiliteApproche = 0.5f;

    // Réaction en attente : elle sera appliquée à l'instant "tempsApplication" (évènement + latence).
    private struct Impulsion
    {
        public float tempsApplication;
        public float amplitude;
    }

    // Files d'attente des réactions pas encore appliquées.
    private readonly List<Impulsion> impulsionsEda = new List<Impulsion>();
    private readonly List<Impulsion> impulsionsBpm = new List<Impulsion>();

    // Phasique : Variations transitoires de l'activité physiologique associées ici à une réaction à un stimulus.
    // Elle monte quand une réaction est appliquée et redescend vers 0.
    private float phasiqueEda = 0f;  
    private float phasiqueBpm = 0f;
    
    private float edaLisse;
    private float bpmLisse;
    private SensorSample dernier;

    void OnEnable()  { EventBus.OnEvent += SurEvenement; }
    void OnDisable() { EventBus.OnEvent -= SurEvenement; }

    void Start()
    {
        edaLisse = edaRepos;
        bpmLisse = bpmRepos;
        dernier = new SensorSample(Time.time, edaLisse, bpmLisse, new Vector2(0.5f, 0.5f));
    }

    void Update()
    {
        // Temps écoulé depuis la dernière frame.
        float dt = Time.deltaTime;

        // Lit les touches du clavier
        GererTouches();

        // ÉTAPE 1 : applique les réactions dont la latence est écoulée.
        AppliquerImpulsions(impulsionsEda, ref phasiqueEda);
        AppliquerImpulsions(impulsionsBpm, ref phasiqueBpm);

        // ÉTAPE 2 : fait décroitre les réactions proressivement.
        // Chaque frame, on multiplie le phasique par exp(-dt / constante), avec dt le temps écoulé depuis la dernière frame
        // Le calcul utilise dt pour que la vitesse de décroissance ne dépende pas du nombre de fps.
        phasiqueEda *= Mathf.Exp(-dt / Mathf.Max(0.01f, constanteEda));
        phasiqueBpm *= Mathf.Exp(-dt / Mathf.Max(0.01f, constanteBpm));

        // ÉTAPE 3 : calcule le bruit. On utilise le bruit de Perlin puisqu'il renvoie une valeur entre 0 et 1 qui varie
        // doucement dans le temps (contrairement à Random.value qui sauterait à chaque frame).
        // On retire 0.5 puis on multiplie par 2 pour obtenir une valeur entre -1 et 1
        // (au lieu de 0 et 1), qu'on multiplie par l'amplitude du bruit, pour être centré sur 0.
        // Les 2ndes valeurs (0.1f et 7.3f) servent à obtenir un bruit différent pour l'EDA et pour le cœur.
        float bruitE = (Mathf.PerlinNoise(Time.time * 0.5f, 0.1f) - 0.5f) * 2f * bruitEda;
        float bruitB = (Mathf.PerlinNoise(Time.time * 0.8f, 7.3f) - 0.5f) * 2f * bruitBpm;
        
        // ÉTAPE 4 : valeur visée = repos + réaction + bruit.
        float edaCible = edaRepos + phasiqueEda + bruitE;
        float bpmCible = bpmRepos + phasiqueBpm + bruitB;

        // ÉTAPE 5 : rapproche progressivement la valeur actuelle de la valeur visée (lissage).
        // Sans cette étape, le signal ferait un saut lors d'une réaction.
        float k = 1f - Mathf.Exp(-dt / Mathf.Max(0.01f, lissage));
        edaLisse = Mathf.Lerp(edaLisse, edaCible, k);
        bpmLisse = Mathf.Lerp(bpmLisse, bpmCible, k);
        
        dernier = new SensorSample(Time.time, edaLisse, bpmLisse, new Vector2(0.5f, 0.5f));
    }

    public SensorSample Lire() => dernier;

    void SurEvenement(HorrorEvent e)
    {
        // (Sensibilité du joueur pour cet évènement * facteur global), limitée à 1, puis multipliée par l'intensité de l'évènement.
        float amplitude = Mathf.Clamp01(Sensibilite(e.categories) * facteurSensibilite) * e.intensite;
        
        // Si pas de réaction, ne rien faire
        if (amplitude <= 0f) return;
        Programmer(amplitude);
    }

    // Met une réaction en attente
    void Programmer(float amplitude)
    {
        impulsionsEda.Add(new Impulsion { tempsApplication = Time.time + latenceEda, amplitude = amplitude * gainEda });
        impulsionsBpm.Add(new Impulsion { tempsApplication = Time.time + latenceBpm, amplitude = amplitude * gainBpm });
    }

    // Parcourt la file d'attente et ajoute à "phasique" toutes les réactions arrivées à échéance.
    // La boucle va de la fin vers le début pour pouvoir supprimer des éléments sans décaler les indices.
    void AppliquerImpulsions(List<Impulsion> liste, ref float phasique)
    {
        for (int i = liste.Count - 1; i >= 0; i--)
        {
            if (Time.time >= liste[i].tempsApplication)
            {
                phasique += liste[i].amplitude;
                liste.RemoveAt(i);
            }
        }
    }

    // Calcule la sensibilité du joueur simulé pour un évènement qui peut avoir plusieurs catégories.
    // On retient la plus forte : un évènement son + lumière provoque la réaction du joueur
    // pour sa catégorie la plus sensible (ici le son).
    // ÉVENTUELLEMENT À REVOIR, méthode peut-être trop discriminante des sensibilités du joueur
    float Sensibilite(EventCategory categories)
    {
        float max = 0f;
        foreach (EventCategory c in Enum.GetValues(typeof(EventCategory)))
        {
            if (c == EventCategory.Aucune || (categories & c) == 0) continue;
            max = Mathf.Max(max, SensibilitePour(c));
        }
        return max;
    }

    float SensibilitePour(EventCategory c)
    {
        switch (c)
        {
            case EventCategory.Son:        return sensibiliteSon;
            case EventCategory.Lumiere:    return sensibiliteLumiere;
            case EventCategory.Obscurite:  return sensibiliteObscurite;
            case EventCategory.Apparition: return sensibiliteApparition;
            case EventCategory.Entite:     return sensibiliteEntite;
            case EventCategory.Mouvement:  return sensibiliteMouvement;
            case EventCategory.Derriere:   return sensibiliteDerriere;
            case EventCategory.Approche:   return sensibiliteApproche;
            default: return 0f;
        }
    }

    void GererTouches()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) facteurSensibilite = 0.3f;
        if (Input.GetKeyDown(KeyCode.Alpha2)) facteurSensibilite = 1f;
        if (Input.GetKeyDown(KeyCode.Alpha3)) facteurSensibilite = 1.6f;

        // Pic parasite : réaction sans événement du jeu (pour tester la robustesse de l'interprétation).
        if (Input.GetKeyDown(KeyCode.G)) Programmer(0.8f);
    }
}