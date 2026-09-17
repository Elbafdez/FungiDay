using System;
using System.Collections.Generic;
 
/* <summary>

Guarda, para cada índice de ruta, una decisión que ya se tomó una vez (qué prefab tocaba,
si había punto de investigación o no, etc.).

La primera vez que se pide un índice, se genera con la función "generator" que le pasemos
y se guarda. Las siguientes veces (por ejemplo, al volver hacia atrás y que el fondo se
vuelva a crear) se devuelve SIEMPRE la misma decisión, sin volver a tirar los dados.

Esto es justo el patrón que se repetía en BackgroundManager (routeHistory +
GetOrCreateBackgroundPrefab) y que ahora también necesita InvestigationPointManager.

Usa un Dictionary en vez de una List a propósito: BackgroundManager pide TODOS los índices
(0, 1, 2, 3...) sin huecos, pero InvestigationPointManager solo pide los índices de fondos
que tienen InvestigationSpawnPoints (por ejemplo, se salta el índice 0 del fondo de inicio).
Con una List, "posición en la lista" dejaba de coincidir con "índice de ruta" en cuanto
había un hueco, y las decisiones se desplazaban una posición. El Dictionary guarda cada
decisión bajo su índice real, así que funciona igual haya huecos o no.

 </summary> */
public class IndexedHistory<T>
{
    private readonly Dictionary<int, T> history = new Dictionary<int, T>(); // Crea un diccionario para almacenar las decisiones tomadas para cada índice de ruta.
 
    public T GetOrCreate(int routeIndex, Func<int, T> generator) // Devuelve la decisión ya tomada para el índice de ruta dado, o genera una nueva si no existe.
    {
        if (history.TryGetValue(routeIndex, out T value)) // Intenta obtener el valor del diccionario para el índice de ruta dado.
        {
            return value;
        }
 
        value = generator(routeIndex); // Si no existe, genera un nuevo valor usando la función generadora proporcionada.
        history[routeIndex] = value;
        return value;
    }
 
    public bool Contains(int routeIndex) => history.ContainsKey(routeIndex); // Comprueba si ya existe una decisión para el índice de ruta dado.
}
 