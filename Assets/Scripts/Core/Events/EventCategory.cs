using System;
/*
 * Event Category.cs
 * Catégories d'un événement horrifique. Enum à flags: un événement peut en cumuler plusieurs
 * (ex : son + clignotement = Son | Lumiere | Obscurite).
 */
[Flags]
public enum EventCategory
{
    Aucune     = 0,
    Son        = 1 << 0,  // tout stimulus sonore
    Lumiere    = 1 << 1,  // variation d'éclairage (clignotement, changement de couleur...)
    Obscurite  = 1 << 2,  // perte ou baisse de visibilité (lampe éteinte, brouillard...)
    Apparition = 1 << 3,  // apparition soudaine dans le champ de vision du joueur
    Entite     = 1 << 4,  // présence d'une créature / silhouette
    Mouvement  = 1 << 5,  // objet ou décor qui bouge (chaise, porte, chute d'objet...)
    Derriere   = 1 << 6,  // stimulus placé dans le dos du joueur
    Approche   = 1 << 7   // quelque chose se rapproche du joueur
}