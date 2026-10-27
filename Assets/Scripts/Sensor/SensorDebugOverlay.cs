using UnityEngine;
/*
    SensorDebugOverlay.cs
    Affiche à l'écran l'EDA, le BPM (valeur + courbe) et le dernier événement publié.
    F3 : afficher / masquer.
*/
public class SensorDebugOverlay : MonoBehaviour
{
    // Glisser ici l'objet portant un composant qui implémente ISensorSource.
    public MonoBehaviour sourceCapteur;
    public bool visible = true;

    private ISensorSource source;
    private const int N = 200;                 // points par courbe
    private readonly float[] bufferEda = new float[N];
    private readonly float[] bufferBpm = new float[N];
    private int tete = 0;
    private float prochainEchantillon = 0f;
    private string dernierEvenement = "(aucun)";
    private float tempsDernierEvenement = -1f;

    void OnEnable()  { EventBus.OnEvent += SurEvenement; }
    void OnDisable() { EventBus.OnEvent -= SurEvenement; }

    void Start()
    {
        source = sourceCapteur as ISensorSource;
        if (source == null) Debug.LogWarning("SensorDebugOverlay : sourceCapteur n'implémente pas ISensorSource.");
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F3)) visible = !visible;
        if (source == null || Time.time < prochainEchantillon) return;

        SensorSample s = source.Lire();
        bufferEda[tete] = s.eda;
        bufferBpm[tete] = s.bpm;
        tete = (tete + 1) % N;
        prochainEchantillon = Time.time + 0.1f;   // 10 échantillons par seconde
    }

    void SurEvenement(HorrorEvent e)
    {
        dernierEvenement = e.id + " (" + e.categories + ")";
        tempsDernierEvenement = Time.time;
    }

    void OnGUI()
    {
        if (!visible || source == null) return;

        SensorSample s = source.Lire();
        GUI.color = Color.white;
        GUI.Label(new Rect(10, 10, 420, 22), "Capteurs : " + source.Nom + "   (F3 pour masquer)");
        GUI.Label(new Rect(10, 30, 420, 22), "EDA : " + s.eda.ToString("F2") + " µS     BPM : " + s.bpm.ToString("F0"));
        string depuis = tempsDernierEvenement < 0f ? "" : "  il y a " + (Time.time - tempsDernierEvenement).ToString("F1") + " s";
        GUI.Label(new Rect(10, 50, 520, 22), "Dernier événement : " + dernierEvenement + depuis);

        Graphique(new Rect(10, 80, 400, 60), bufferEda, 1f, 6f, new Color(0.3f, 0.8f, 1f));
        Graphique(new Rect(10, 150, 400, 60), bufferBpm, 55f, 110f, new Color(1f, 0.4f, 0.4f));
        GUI.color = Color.white;
    }

    // Courbe en colonnes : le point le plus ancien est à gauche, le plus récent à droite.
    void Graphique(Rect r, float[] buffer, float min, float max, Color couleur)
    {
        GUI.color = new Color(0f, 0f, 0f, 0.5f);
        GUI.DrawTexture(r, Texture2D.whiteTexture);
        GUI.color = couleur;
        float largeur = r.width / N;
        for (int i = 0; i < N; i++)
        {
            float v = buffer[(tete + i) % N];
            float h = Mathf.InverseLerp(min, max, v) * r.height;
            GUI.DrawTexture(new Rect(r.x + i * largeur, r.yMax - h, Mathf.Max(1f, largeur), h), Texture2D.whiteTexture);
        }
    }
}