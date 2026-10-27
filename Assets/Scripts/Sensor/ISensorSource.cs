/*
    Contrat que doit respecter toute source de capteurs (simulée ou réelle).
    Le reste du projet ne dépend que de cette interface : le jour où les vrais capteurs
    arrivent, on écrirera une nouvelle classe qui l'implémente, sans toucher au reste.
*/
public interface ISensorSource
{
    string Nom { get; }
    SensorSample Lire();   // toujours la dernière mesure disponible
}