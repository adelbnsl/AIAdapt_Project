using System;
using UnityEngine;

/*
 * Point central par lequel les actors annoncent qu'un événement vient de se produire.
 * Les modules intéressés (logger, profil, estimateur...) s'abonnent à OnEvent.
 * Classe statique : pas besoin de l'ajouter à aucune scène.
*/

public static class EventBus
{
    // Déclenché à chaque événement publié
    public static event Action<HorrorEvent> OnEvent;
    
    // Numéro de boucle actuel, màj par CouloirLoopController.
    public static int CurrentLoop = 0;

    public static void Publish(HorrorEvent e)
    {
        OnEvent?.Invoke(e);
    }

    public static void Publish(string id, EventCategory categories, float intensite) 
    {
        Publish(new HorrorEvent(id, categories, Mathf.Clamp01(intensite), Time.time, CurrentLoop));
    }
    
    // Réinitialise l'état statique à chaque lancement.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void Reinitialiser()
    {
        OnEvent = null;
        CurrentLoop = 0;
    }
    
}