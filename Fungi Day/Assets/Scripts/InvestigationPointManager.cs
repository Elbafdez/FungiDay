using System.Collections.Generic;
using UnityEngine;

public class InvestigationPointManager : MonoBehaviour
{
    [Header("Ruta")]
    [SerializeField] private RouteDefinition route; // El MISMO asset que se asigna en BackgroundManager. De aquí salen las setas permitidas en esta ruta y la separación entre ellas.
 
    private int backgroundsUntilNextPoint; // Cuántos fondos quedan por pasar antes de que se PUEDA generar el siguiente punto.
 
    /* Guarda, para cada índice de ruta, si le tocaba punto o no (y cuál). Así, si el fondo
    con ese índice se destruye y se vuelve a crear más tarde (al retroceder y volver a
    avanzar), el resultado es siempre el mismo en vez de volver a tirar los dados. */
    private readonly IndexedHistory<PointDecision> pointHistory = new IndexedHistory<PointDecision>();
 
    private class PointDecision
    {
        public bool hasPoint;
        public int prefabIndex;
        public int spawnPointIndex;
    }

    private void Awake()
    {
        /* Inicialización del contador de fondos hasta el próximo punto de investigación. 
        Hacemos esto en Awake para que se ejecute antes de que cualquier fondo intente generar un punto de investigación. */
        backgroundsUntilNextPoint = Random.Range(0, route.maxBackgroundsBetweenPoints + 1);
    }

    //========================= INTENTAR CREAR UN PUNTO EN UN FONDO =========================
    /*
    routeIndex identifica de forma única la posición en la ruta a la que pertenece "background",
    y es lo que usamos como clave del historial (la misma idea que ya usaba BackgroundManager
    para decidir qué prefab de fondo tocaba en cada posición).
    */
    public void TrySpawnInvestigationPoint(GameObject background, int routeIndex)
    // Esta función intenta crear un punto de investigación en el fondo dado, si corresponde según la decisión histórica o una nueva decisión.
    {
        InvestigationSpawnPoints spawnPointsComponent = background.GetComponent<InvestigationSpawnPoints>(); // Intentamos obtener el componente InvestigationSpawnPoints del fondo. Este componente define los puntos de spawn disponibles para los puntos de investigación.

        if (spawnPointsComponent == null) return; // Este fondo no tiene Spawn Points, no hay nada que hacer.
 
        Transform[] spawnPoints = spawnPointsComponent.GetSpawnPoints(); // Obtenemos los puntos de spawn del fondo. Esto nos da un array de Transform que representan las posiciones donde se pueden generar los puntos de investigación.

        if (spawnPoints.Length == 0) return; // Este fondo tiene el componente, pero no hay puntos de spawn definidos. No hay nada que hacer.
 
        // Consultamos el historial para ver si ya habíamos decidido antes si este fondo tenía un punto de investigación y cuál era. Si no hay decisión previa, se genera una nueva decisión.
        PointDecision decision = pointHistory.GetOrCreate(
            routeIndex,
            _ => DecideForNewIndex(spawnPoints.Length)
        );
 
        if (!decision.hasPoint) return; // Si la decisión es que no hay punto de investigación, no hacemos nada.
 
        if (route.investigationPointPrefabs.Count == 0) // Si no hay prefabs de puntos de investigación asignados, mostramos una advertencia y salimos.
        {
            Debug.LogWarning("No hay prefabs asignados en InvestigationPointManager.");
            return;
        }

        // Instanciamos el punto de investigación en la posición del spawn point seleccionado y con la rotación por defecto (Quaternion.identity).
        Transform selectedSpawnPoint = spawnPoints[decision.spawnPointIndex];
        GameObject prefabToSpawn = route.investigationPointPrefabs[decision.prefabIndex];
 
        GameObject newPoint = Instantiate(prefabToSpawn, selectedSpawnPoint.position, Quaternion.identity);
 
        newPoint.transform.SetParent(background.transform, true); // Lo hacemos hijo del fondo: así se mueve junto con él automáticamente y se destruye solo cuando el fondo se destruye.
        // El segundo parámetro "true" indica que queremos mantener la posición y rotación globales del nuevo punto de investigación, en lugar de ajustarlas a las del fondo.
    }
 
    //========================= DECIDIR SI TOCA PUNTO EN UN ÍNDICE NUEVO =========================
    private PointDecision DecideForNewIndex(int spawnPointCount)
    // Esta función decide si un fondo nuevo (que no tiene historial) tendrá un punto de investigación y cuál será.
    {
        var decision = new PointDecision { hasPoint = false }; // Inicializamos la decisión con hasPoint en false, indicando que por defecto no habrá punto de investigación.
 
        if (backgroundsUntilNextPoint > 0) // Si aún no hemos llegado al número mínimo de fondos entre puntos, decrementamos el contador y devolvemos la decisión de no tener punto.
        {
            backgroundsUntilNextPoint--;
            return decision;
        }

        // Si hemos llegado al número mínimo de fondos entre puntos, decidimos que este fondo tendrá un punto de investigación. Seleccionamos aleatoriamente un índice de spawn point y un índice de prefab de punto de investigación.
        decision.hasPoint = true;
        decision.spawnPointIndex = Random.Range(0, spawnPointCount);
        decision.prefabIndex = Random.Range(0, route.investigationPointPrefabs.Count);
 
        backgroundsUntilNextPoint = Random.Range(route.minBackgroundsBetweenPoints, route.maxBackgroundsBetweenPoints + 1); // Reiniciamos el contador de fondos hasta el próximo punto de investigación, eligiendo un número aleatorio dentro del rango especificado.
 
        return decision;
    }
}