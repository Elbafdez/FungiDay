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
 Se asume que los índices se piden en orden creciente (0, 1, 2...), igual que en el código
 original.

 </summary> */
public class IndexedHistory<T>
{
    private readonly List<T> history = new List<T>(); // Lista que guarda, para cada índice de ruta, la decisión que se tomó (qué prefab tocaba, si había punto de investigación o no, etc.).
 
    public T GetOrCreate(int routeIndex, Func<int, T> generator) // Método que devuelve la decisión guardada para un índice de ruta, o genera una nueva si no existe.
    {
        if (routeIndex < history.Count) // Si ya existe una decisión guardada para este índice de ruta, la devolvemos.
        {
            return history[routeIndex];
        }
 
        T value = generator(routeIndex); // Si no existe, generamos una nueva decisión usando la función "generator" que nos pasaron .
        history.Add(value);
        return value;
    }
 
    public bool Contains(int routeIndex) => routeIndex < history.Count; // Método que devuelve true si ya existe una decisión guardada para un índice de ruta, false si no.
}
 