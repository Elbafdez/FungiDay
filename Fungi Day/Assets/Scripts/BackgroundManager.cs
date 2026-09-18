using System.Collections.Generic;
using UnityEngine;

public class BackgroundManager : MonoBehaviour
{
    [Header("Ruta")]
    [SerializeField] private RouteDefinition route; // Aquí está TODO lo específico de esta ruta (qué fondo va primero, cuál se repite, especiales y en qué orden). Para otra ruta, se crea otro RouteDefinition y se engancha aquí — este script no cambia.

    [Header("Configuración")]
    [SerializeField] private float backgroundWidth = 18f;
    [SerializeField] private InvestigationPointManager investigationPointManager;

    [Header("Fondo inicial")]
    [SerializeField] private int initialBackgrounds = 3;

    // ======================== LISTAS DE FONDOS =========================
    private List<GameObject> activeBackgrounds = new List<GameObject>(); // Fondos activos actualmente en la escena.
    private List<int> activeBackgroundIndexes = new List<int>(); // Índice de ruta que representa cada fondo activo (mismo orden que activeBackgrounds).
    private readonly IndexedHistory<GameObject> routeHistory = new IndexedHistory<GameObject>(); // Historial de qué prefab le corresponde a cada índice de ruta.
    
    // ======================== CONTADORES =========================
    private int baseCount = 0; // Cuenta fondos "base" repetidos, sean bosques, praderas o lo que traiga la ruta.
    private int baseBeforeSpecial; // Cuántos fondos base tocan antes del siguiente especial.
 
    private int currentSpecialIndex = 0; // Índice del siguiente prefab especial a generar, dentro de route.specialPrefabs.

    void Start()
    {
        baseBeforeSpecial = Random.Range(route.minBaseBeforeSpecial, route.maxBaseBeforeSpecial + 1); // Elegimos cuántos fondos base aparecerán antes del primer especial, dentro del rango definido en la ruta.

        // Creamos los fondos iniciales y los agregamos a la lista de fondos activos
        for (int i = 0; i < initialBackgrounds; i++)
        {
            SpawnBackground(i, i * backgroundWidth);
        }
    }

    //========================= MOVIMIENTO HACIA DELANTE =========================
    public void MoveForward()
    {
        foreach (GameObject background in activeBackgrounds) // Iteramos sobre cada fondo activo
        {
            background.GetComponent<BackgroundScroller>().MoveForward(); // Llamamos al método MoveForward() del script BackgroundScroller de cada fondo activo para moverlo hacia la izquierda
        }

        CheckForwardBackgrounds(); // Verificamos si algún fondo ha salido completamente de la pantalla hacia la izquierda.
    }

    //========================= MOVIMIENTO HACIA ATRÁS =========================
    public void MoveBackward()
    {
        foreach (GameObject background in activeBackgrounds)
        {
            background.GetComponent<BackgroundScroller>().MoveBackward();
        }

        CheckBackwardBackgrounds(); // Verificamos si algún fondo ha salido completamente de la pantalla hacia la derecha.
    }

    //========================= COMPROBAR FONDOS IZQUIERDA =========================
    private void CheckForwardBackgrounds()  
    // Este método verifica si el primer fondo activo ha salido completamente de la pantalla hacia la izquierda. Si es así, lo destruye y genera un nuevo fondo al final de la lista.
    {
        // Si la ruta ya ha terminado y no queda ningún fondo activo, no hay nada que comprobar.
        if (activeBackgrounds.Count == 0) return;

        GameObject firstBackground = activeBackgrounds[0]; // Obtenemos el primer fondo activo de la lista.

        if (firstBackground.transform.position.x <= -backgroundWidth)
        {
            Destroy(firstBackground); // Destruimos el primer fondo activo.

            // Lo eliminamos de las listas de objetos activos.
            activeBackgrounds.RemoveAt(0);
            activeBackgroundIndexes.RemoveAt(0);

            if (activeBackgroundIndexes.Count == 0) return; // No queda ningún fondo de referencia para calcular el siguiente.

            // El siguiente fondo será el índice que viene después del último fondo activo.
            int lastRouteIndex = activeBackgroundIndexes[activeBackgroundIndexes.Count - 1]; // Obtenemos el índice del último fondo activo

            int nextRouteIndex = lastRouteIndex + 1; // Calculamos el índice del siguiente fondo a generar

            // Calculamos la posición física donde aparecerá el siguiente fondo, que será la posición del último fondo activo más el ancho del fondo.
            float newPosition = activeBackgrounds[activeBackgrounds.Count - 1].transform.position.x + backgroundWidth;

            if (currentSpecialIndex < route.specialPrefabs.Count) // Solo seguimos generando si todavía quedan especiales de esta ruta por colocar.
            {
                SpawnBackground(nextRouteIndex, newPosition);
            }
        }
    }

    //========================= COMPROBAR FONDOS DERECHA =========================
    private void CheckBackwardBackgrounds()
    // Este método verifica si el último fondo activo ha salido completamente de la pantalla hacia la derecha. Si es así, lo destruye y genera un nuevo fondo al principio de la lista.
    {
        if (activeBackgrounds.Count == 0) return;

        GameObject lastBackground = activeBackgrounds[activeBackgrounds.Count - 1];

        if (lastBackground.transform.position.x >= backgroundWidth)
        {
            Destroy(lastBackground); // Destruimos el último fondo activo

            activeBackgrounds.RemoveAt(activeBackgrounds.Count - 1); // Lo eliminamos de la lista de fondos activos
            activeBackgroundIndexes.RemoveAt(activeBackgroundIndexes.Count - 1); // Lo eliminamos de la lista de índices de fondos activos

            if (activeBackgroundIndexes.Count == 0) return; // No queda ningún fondo de referencia para calcular el siguiente.

            // Queremos recuperar el fondo anterior al primero que tenemos actualmente.
            int firstRouteIndex = activeBackgroundIndexes[0]; // Obtenemos el índice del primer fondo activo
            int previousRouteIndex = firstRouteIndex - 1; // Calculamos el índice del fondo anterior al primero que tenemos actualmente

            if (previousRouteIndex >= 0)  // No podemos ir más atrás del inicio de la ruta.
            {
                float newPosition = activeBackgrounds[0].transform.position.x - backgroundWidth;

                SpawnBackgroundAtBeginning(previousRouteIndex, newPosition); // Generamos el fondo anterior al principio de la lista de fondos activos
            }
        }
    }

    //========================= DECIDIR QUÉ PREFAB LE TOCA A UN ÍNDICE NUEVO =========================
    private GameObject DecidePrefabForNewIndex(int routeIndex)
    // Método que decide qué prefab le corresponde a un índice de ruta nuevo. Se llama desde routeHistory.GetOrCreate(routeIndex, DecidePrefabForNewIndex) para generar un nuevo fondo si no existe uno guardado para ese índice.
    {
        GameObject prefabToSpawn; // Variable que guardará el prefab que le corresponde a este índice de ruta
 
        if (routeIndex == 0) // Si es el primer fondo de la ruta, siempre es el fondo de inicio
        {
            prefabToSpawn = route.startPrefab;
        }
        else if (baseCount < baseBeforeSpecial) // Si todavía no hemos generado suficientes fondos base antes del siguiente especial, generamos otro fondo base
        {
            prefabToSpawn = route.basePrefab;
            baseCount++;
        }
        else // Si ya hemos generado suficientes fondos base, generamos el siguiente fondo especial en el orden definido en la ruta
        {
            prefabToSpawn = route.specialPrefabs[currentSpecialIndex];
            currentSpecialIndex++;
            baseCount = 0;
 
            if (currentSpecialIndex < route.specialPrefabs.Count) // Si todavía quedan especiales por generar, elegimos cuántos fondos base aparecerán antes del siguiente especial, dentro del rango definido en la ruta.
            {
                baseBeforeSpecial = Random.Range(route.minBaseBeforeSpecial, route.maxBaseBeforeSpecial + 1);
            }
        }
 
        return prefabToSpawn;
    }
 
    //========================= CREAR FONDO (AL FINAL) =========================
    private void SpawnBackground(int routeIndex, float positionX)
    // Método que genera un fondo en la posición especificada y lo agrega a la lista de fondos activos
    {
        GameObject prefabToSpawn = routeHistory.GetOrCreate(routeIndex, DecidePrefabForNewIndex); // Obtenemos el prefab que le corresponde a este índice de ruta, ya sea recuperándolo del historial o generándolo si no existe.
 
        GameObject newBackground = Instantiate(
            prefabToSpawn,
            new Vector3(positionX, 0, 0),
            Quaternion.identity
        );
 
        investigationPointManager.TrySpawnInvestigationPoint(newBackground, routeIndex); // Intentamos crear un punto de investigación en este fondo, si le corresponde según el índice de ruta.

        // Agregamos el nuevo fondo a la lista de fondos y índices activos.
        activeBackgrounds.Add(newBackground);
        activeBackgroundIndexes.Add(routeIndex);
    }

    //========================= CREAR FONDO (AL PRINCIPIO) =========================
    private void SpawnBackgroundAtBeginning(int routeIndex, float positionX)
    // Método que genera un fondo en la posición especificada y lo agrega al principio de la lista de fondos activos
    {
        GameObject prefabToSpawn = routeHistory.GetOrCreate(routeIndex, DecidePrefabForNewIndex); // Obtenemos el prefab que le corresponde a este índice de ruta, ya sea recuperándolo del historial o generándolo si no existe.
 
        GameObject newBackground = Instantiate(
            prefabToSpawn,
            new Vector3(positionX, 0, 0),
            Quaternion.identity
        );
 
        //Al retroceder, el fondo reaparece con su punto de investigación (si tiene).
        investigationPointManager.TrySpawnInvestigationPoint(newBackground, routeIndex);
 
        activeBackgrounds.Insert(0, newBackground);
        activeBackgroundIndexes.Insert(0, routeIndex);
    }
}