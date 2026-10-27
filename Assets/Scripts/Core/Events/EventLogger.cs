using UnityEngine;
/*
    Écoute l'EventBus et affiche chaque événement dans la Console.
    Sera complété ultérieurement pour exporter les événements et les réactions en CSV.
*/
public class EventLogger : MonoBehaviour
{
    public bool afficherDansConsole = true;

    void OnEnable()  { EventBus.OnEvent += Enregistrer; }
    void OnDisable() { EventBus.OnEvent -= Enregistrer; }

    void Enregistrer(HorrorEvent e)
    {
        if (afficherDansConsole) Debug.Log(e.ToString());
    }
}