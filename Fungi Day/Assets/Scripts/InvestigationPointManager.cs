using System.Collections.Generic;
using UnityEngine;

public class InvestigationPointManager : MonoBehaviour
{
    [Header("Prefabs de puntos de investigación")]
    [SerializeField] private List<GameObject> investigationPointPrefabs = new List<GameObject>(); // Lista de prefabs de puntos de investigación que se pueden crear.

    [Header("Separación entre puntos")]
    [SerializeField] private int minBackgroundsBetweenPoints = 1; // Número mínimo de fondos que deben generarse antes de que pueda aparecer otro punto de investigación.
    [SerializeField] private int maxBackgroundsBetweenPoints = 3; // Número máximo de fondos que deben generarse antes de que pueda aparecer otro punto de investigación.
    private int backgroundsUntilNextPoint = 0; // Contador de fondos que deben generarse antes de que pueda aparecer otro punto de investigación.

    [Header("Configuración")]
    [SerializeField] private float spawnChance = 60f; // Probabilidad de que aparezca un punto de investigación.

    private List<GameObject> activeInvestigationPoints = new List<GameObject>(); // Lista de los puntos que existen actualmente en la escena.

    //========================= INTENTAR CREAR UN PUNTO EN UN FONDO =========================
    public void TrySpawnInvestigationPoint(GameObject background) // Método para intentar crear un punto de investigación en un fondo específico
    {   
        // Primero comprobamos si el fondo tiene el componente InvestigationSpawnPoints. 
        InvestigationSpawnPoints spawnPointsComponent = background.GetComponent<InvestigationSpawnPoints>();

        // Si este fondo no tiene Spawn Points, simplemente no hacemos nada.
        if (spawnPointsComponent == null)
        {
            return;
        }

        // Obtenemos todos los Spawn Points disponibles en este fondo.
        Transform[] spawnPoints = spawnPointsComponent.GetSpawnPoints();

        // Si no hay ningún Spawn Point, tampoco podemos crear nada.
        if (spawnPoints.Length == 0)
        {
            return;
        }

        // Si aún no han pasado suficientes fondos desde el último punto de investigación no hacemos nada y decrementamos el contador.
        if (backgroundsUntilNextPoint > 0)
        {
            backgroundsUntilNextPoint--;
            return;
        }

        //--------- DECIDIR SI APARECE UN PUNTO --------- 
        float randomChance = Random.Range(0f, 100f); // Generamos un número aleatorio entre 0 y 100 para decidir si se creará un punto de investigación.

        // Si el número aleatorio es mayor que nuestra probabilidad de aparición, no creamos nada.
        if (randomChance > spawnChance)
        {
            return;
        }

        //--------- ELEGIR SPAWN POINT ALEATORIO --------- 
        int randomSpawnPointIndex = Random.Range(0, spawnPoints.Length); // Elegimos un índice aleatorio dentro del rango de los Spawn Points disponibles.
        
        Transform selectedSpawnPoint = spawnPoints[randomSpawnPointIndex]; // Obtenemos el Spawn Point seleccionado.

        //--------- ELEGIR PREFAB ALEATORIO ---------
        if (investigationPointPrefabs.Count == 0) // Verificamos si la lista de prefabs está vacía antes de intentar seleccionar uno.
        { 
            Debug.LogWarning( "No hay prefabs asignados en InvestigationPointManager." );
            return;
        } 
            
        int randomPrefabIndex = Random.Range( 0, investigationPointPrefabs.Count ); // Elegimos un índice aleatorio dentro del rango de los prefabs disponibles.
        
        GameObject prefabToSpawn = investigationPointPrefabs[randomPrefabIndex]; // Obtenemos el prefab seleccionado.

        //--------- CREAR PUNTO DE INVESTIGACIÓN ---------
        GameObject newPoint = Instantiate( prefabToSpawn, selectedSpawnPoint.position, Quaternion.identity ); // Instanciamos el nuevo punto de investigación en la posición del Spawn Point seleccionado.
        
        activeInvestigationPoints.Add(newPoint); // Lo añadimos a la lista de objetos activos.

        backgroundsUntilNextPoint = Random.Range(minBackgroundsBetweenPoints, maxBackgroundsBetweenPoints + 1); // Elegimos cuántos fondos tendrán que pasar antes de que pueda aparecer otro punto.
    }

    //========================= MOVIMIENTO HACIA DELANTE =========================
    public void MoveForward()
    {
        foreach (GameObject point in activeInvestigationPoints)
        {
            BackgroundScroller scroller = point.GetComponent<BackgroundScroller>();

            if (scroller != null) // Verificamos si el componente BackgroundScroller existe antes de llamar al método MoveForward
            { 
                scroller.MoveForward();
            }
        }
    }

    //========================= MOVIMIENTO HACIA ATRÁS =========================
    public void MoveBackward()
    {
        foreach (GameObject point in activeInvestigationPoints)
        {
            BackgroundScroller scroller = point.GetComponent<BackgroundScroller>();

            if (scroller != null)
            { 
                scroller.MoveBackward();
            }
        }
    }
}