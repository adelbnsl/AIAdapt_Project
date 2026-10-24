/*
 * Description d'un événement horrifique au moment où il se produit.
 * La structure est faite pour être immuable : on la crée, on la publie sur l'EventBus puis on ne le modifie plus.
 */

public readonly struct HorrorEvent
{
    public readonly string id;                 // ex. "son_pas", "silhouette"
    public readonly EventCategory categories;  // une ou plusieurs catégories
    public readonly float intensite;           // 0 (discret) à 1 (très intense)
    public readonly float temps;               // Time.time au déclenchement
    public readonly int boucle;                // numéro de boucle du couloir

    public HorrorEvent(string id, EventCategory categories, float intensite, float temps, int boucle)
    {
        this.id = id;
        this.categories = categories;
        this.intensite = intensite;
        this.temps = temps;
        this.boucle = boucle;
    }

    public bool A(EventCategory c) => (categories & c) != 0;
    
    public override string ToString() =>
        $"[Event] {id} | {categories} | intensité {intensite:F2} | t={temps:F1}s | boucle {boucle}";
    
}