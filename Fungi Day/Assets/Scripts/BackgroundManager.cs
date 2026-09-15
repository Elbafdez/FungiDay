using System.Collections.Generic;
using UnityEngine;

public class BackgroundManager : MonoBehaviour
{
    [Header("Prefab de fondo base")]
    [SerializeField] private GameObject forestPrefab;

    [Header("Prefabs de fondo especiales")]
    [SerializeField] private GameObject startRoutePrefab;
    [SerializeField] private GameObject fallenTreePrefab;
    [SerializeField] private GameObject flowerFieldPrefab;
    [SerializeField] private GameObject rockClusterPrefab;
    [SerializeField] private GameObject endRoutePrefab;

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
    private int forestCount = 0; // Contador de bosques generados antes del árbol caído
    private int forestsBeforeSpecial; // Número aleatorio de bosques que aparecerán antes del árbol caído

    // ======================== CONTROL DE PREFABS ESPECIALES =========================
    private List<GameObject> specialPrefabs = new List<GameObject>(); // Prefabs especiales en el orden en que deben aparecer.
    private int currentSpecialIndex = 0; // Índice del siguiente prefab especial a generar

    void Start()
    {
        // Elegimos si aparecerán 4 - 6 bosques antes del árbol caído
        forestsBeforeSpecial = Random.Range(3, 6);

        // Agregamos los prefabs especiales a la lista en el ORDEN que queremos que aparezcan
        specialPrefabs.Add(fallenTreePrefab);
        specialPrefabs.Add(flowerFieldPrefab);
        specialPrefabs.Add(rockClusterPrefab);
        specialPrefabs.Add(endRoutePrefab);

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

            if (currentSpecialIndex < specialPrefabs.Count) // Solo seguimos generando si todavía quedan prefabs especiales por colocar.
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
 
        if (routeIndex == 0) // Si es el primer índice de ruta, le corresponde el prefab de inicio
        {
            prefabToSpawn = startRoutePrefab;
        }
        else if (forestCount < forestsBeforeSpecial) // Si aún no han aparecido suficientes bosques antes del siguiente prefab especial, le corresponde un bosque
        {
            prefabToSpawn = forestPrefab;
            forestCount++;
        }
        else // Si ya han aparecido suficientes bosques, le corresponde el siguiente prefab especial en la lista
        {
            prefabToSpawn = specialPrefabs[currentSpecialIndex];
            currentSpecialIndex++;
            forestCount = 0;
 
            if (currentSpecialIndex < specialPrefabs.Count) // Si todavía quedan prefabs especiales por generar, elegimos cuántos bosques aparecerán antes del siguiente
            {
                forestsBeforeSpecial = Random.Range(4, 7);
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