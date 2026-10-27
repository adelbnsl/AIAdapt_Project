using UnityEngine;
/*
    Simulation des signaux à un instant donné avant d'avoir les senseurs.
*/
public readonly struct SensorSample
{
    public readonly float temps;    // Time.time de la mesure
    public readonly float eda;      // activité électrodermale en microsiemens (µS)
    public readonly float bpm;      // fréquence cardiaque en battements par minute
    public readonly Vector2 regard; // point de regard en coordonnées (0..1, 0..1)

    public SensorSample(float temps, float eda, float bpm, Vector2 regard)
    {
        this.temps = temps;
        this.eda = eda;
        this.bpm = bpm;
        this.regard = regard;
    }
}