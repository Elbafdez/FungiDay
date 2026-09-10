using System.Collections.Generic;
using UnityEngine;

public class BackgroundManager : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject forestPrefab;
    [SerializeField] private GameObject fallenTreePrefab;

    [Header("Configuración")]
    [SerializeField] private float backgroundWidth = 18f;

    [Header("Fondos iniciales")]
    [SerializeField] private int initialBackgrounds = 3;

    // ======================== LISTAS DE FONDOS =========================
    private List<GameObject> activeBackgrounds = new List<GameObject>(); // Lista de fondos activos actualmente en la escena, se podran destruir al salir de la pantalla y se generaran nuevos fondos al final de la lista.
    private List<GameObject> routeHistory = new List<GameObject>(); // Lista de prefabs que han sido generados, para poder repetir la misma ruta al volver hacia atrás, no se borra al destruir los fondos, solo se agregan nuevos prefabs a la lista.
    private List<int> activeBackgroundIndexes = new List<int>(); // Lista de índices de los prefabs que han sido generados, para saber qué prefab se generó en cada posición y poder repetir la misma ruta al volver hacia atrás, no se borra al destruir los fondos, solo se agregan nuevos índices a la lista.
    
    // ======================== CONTADORES =========================
    private int forestCount = 0; // Contador de bosques generados antes del árbol caído
    private int forestsBeforeSpecial; // Número aleatorio de bosques que aparecerán antes del árbol caído
    // =============================================================
    void Start()
    {
        // Elegimos si aparecerán 4 - 6 bosques antes del árbol caído
        forestsBeforeSpecial = Random.Range(4, 7);

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
            BackgroundScroller scroller = background.GetComponent<BackgroundScroller>(); // Obtenemos el script BackgroundScroller del fondo

            scroller.MoveForward(); // Llamamos al método MoveForward() del script BackgroundScroller para mover el fondo hacia la izquierda
        }

        CheckForwardBackgrounds(); // Verificamos si algún fondo ha salido completamente de la pantalla hacia la izquierda
    }

    //========================= MOVIMIENTO HACIA ATRÁS =========================
    public void MoveBackward()
    {
        foreach (GameObject background in activeBackgrounds)
        {
            BackgroundScroller scroller = background.GetComponent<BackgroundScroller>();
            scroller.MoveBackward();
        }

        CheckBackwardBackgrounds(); // Verificamos si algún fondo ha salido completamente de la pantalla hacia la izquierda
    }

    //========================= COMPROBAR FONDOS IZQUIERDA =========================
    private void CheckForwardBackgrounds()  
    // Este método verifica si el primer fondo activo ha salido completamente de la pantalla hacia la izquierda. Si es así, lo destruye y genera un nuevo fondo al final de la lista.
    {
        GameObject firstBackground = activeBackgrounds[0];

        if (firstBackground.transform.position.x <= -backgroundWidth)
        {
            int firstRouteIndex = activeBackgroundIndexes[0]; // Guardamos el índice del primer fondo activo antes de destruirlo

            Destroy(firstBackground);

            // Lo eliminamos de las listas de objetos activos.
            activeBackgrounds.RemoveAt(0);
            activeBackgroundIndexes.RemoveAt(0);

            // El siguiente fondo será el índice que viene después del último fondo activo.
            int lastRouteIndex = activeBackgroundIndexes[activeBackgroundIndexes.Count - 1]; // Obtenemos el índice del último fondo activo

            int nextRouteIndex = lastRouteIndex + 1; // Calculamos el índice del siguiente fondo a generar

            // Calculamos la posición física donde aparecerá el siguiente fondo, que será la posición del último fondo activo más el ancho del fondo.
            float newPosition = activeBackgrounds[activeBackgrounds.Count - 1].transform.position.x + backgroundWidth;

            SpawnBackground(nextRouteIndex, newPosition); // Generamos el siguiente fondo en la posición calculada
            //Si este índice ya existe en routeHistory, recuperará el prefab guardado. Si todavía no existe, generará uno nuevo y lo guardará en routeHistory.
        }
    }

    //========================= COMPROBAR FONDOS DERECHA =========================
    private void CheckBackwardBackgrounds()
    // Este método verifica si el último fondo activo ha salido completamente de la pantalla hacia la derecha. Si es así, lo destruye y genera un nuevo fondo al principio de la lista.
    {
        GameObject lastBackground = activeBackgrounds[activeBackgrounds.Count - 1];

        if (lastBackground.transform.position.x >= backgroundWidth)
        {
            // Guardamos el índice del último fondo antes de eliminarlo.
            int lastRouteIndex = activeBackgroundIndexes[activeBackgroundIndexes.Count - 1];

            Destroy(lastBackground); // Destruimos el último fondo activo

            activeBackgrounds.RemoveAt(activeBackgrounds.Count - 1); // Lo eliminamos de la lista de fondos activos
            activeBackgroundIndexes.RemoveAt(activeBackgroundIndexes.Count - 1); // Lo eliminamos de la lista de índices de fondos activos

            // Queremos recuperar el fondo anterior al primero que tenemos actualmente.
            int firstRouteIndex = activeBackgroundIndexes[0]; // Obtenemos el índice del primer fondo activo
            int previousRouteIndex = firstRouteIndex - 1; // Calculamos el índice del fondo anterior al primero que tenemos actualmente

            //No podemos ir más atrás del inicio de la ruta. Si previousRouteIndex es menor que 0, significa que hemos llegado al principio.
            if (previousRouteIndex >= 0)
            {
                float newPosition = activeBackgrounds[0].transform.position.x - backgroundWidth;

                SpawnBackgroundAtBeginning(previousRouteIndex, newPosition); // Generamos el fondo anterior al principio de la lista de fondos activos
            }
        }
    }

    //========================= OBTENER O CREAR EL PREFAB CORRECTO =========================
    // Este método obtiene el prefab correspondiente al índice de ruta especificado. Si el índice ya existe en routeHistory, devuelve el prefab guardado. Si no, genera un nuevo prefab y lo guarda en routeHistory.
    private GameObject GetOrCreateBackgroundPrefab(int routeIndex)
    {
        // -----------------------------------------------------
        // CASO 1:
        // Este punto de la ruta ya existe. Simplemente recuperamos el prefab que recordamos.
        // -----------------------------------------------------

        if (routeIndex < routeHistory.Count)
        {
            return routeHistory[routeIndex];
        }

        // -----------------------------------------------------
        // CASO 2:
        // Estamos avanzando a una zona nueva de la ruta. Tenemos que decidir qué prefab toca y guardarlo.
        // -----------------------------------------------------

        GameObject prefabToSpawn;


        if (forestCount < forestsBeforeSpecial) // Si aún no hemos alcanzado el número de bosques antes del árbol caído
        {
            prefabToSpawn = forestPrefab;

            forestCount++;
        }
        else // Si hemos alcanzado el número de bosques, instanciamos el árbol caído
        {
            prefabToSpawn = fallenTreePrefab;

            forestCount = 0;

            forestsBeforeSpecial = Random.Range(4, 7); // El siguiente grupo tendrá 4 - 6 bosques
        }

        // Guardamos el prefab en el historial permanente de la ruta.

        routeHistory.Add(prefabToSpawn);


        return prefabToSpawn; // Devolvemos el prefab que acabamos de decidir
    }

    //========================= CREAR FONDO =========================
    private void SpawnBackground(int routeIndex,float positionX)
    {
        // Primero obtenemos qué prefab corresponde a esta posición concreta de la ruta. (Nos lo dice el método GetOrCreateBackgroundPrefab())

        GameObject prefabToSpawn = GetOrCreateBackgroundPrefab(routeIndex);


        // Instanciamos ese prefab.

        GameObject newBackground = Instantiate(
            prefabToSpawn,
            new Vector3(positionX, 0, 0), // La posición en X se calcula según la posición del último fondo activo más el ancho del fondo.
            Quaternion.identity
        );


        // Lo añadimos a la lista de fondos activos.
        activeBackgrounds.Add(newBackground);


        // También guardamos qué índice de la ruta representa.
        activeBackgroundIndexes.Add(routeIndex);
    }

    //========================= CREAR FONDO A LA IZQUIERDA =========================
    private void SpawnBackgroundAtBeginning(int routeIndex, float positionX)
    {
        // Consultamos el historial de la ruta para ver qué prefab corresponde a este índice de ruta. Si no existe, lo creamos y lo guardamos.
        GameObject prefabToSpawn = GetOrCreateBackgroundPrefab(routeIndex);

        // Instanciamos ese prefab.
        GameObject newBackground = Instantiate(
            prefabToSpawn,
            new Vector3(positionX, 0, 0),
            Quaternion.identity
        );


        // Como este fondo aparece antes de todos los demás, lo añadimos al principio de las listas.

        activeBackgrounds.Insert(0, newBackground);

        activeBackgroundIndexes.Insert(0, routeIndex);
    }
}

// ========================= PROBLEMAS =========================
// 1. Cuando el jugador se mueve hacia atrás MUEVE HACIA ATRÁS, los fondos no se generan correctamente, solo se REPITE EL BOSQUE. 
//Esto se debe a que el método SpawnNextBackgroundAtBeginning() siempre instancia un bosque, sin considerar la lógica de cuántos bosques deben aparecer antes del árbol caído. Para solucionar esto, se podría implementar una lógica similar a la de SpawnNextBackground() para decidir qué prefab instanciar al moverse hacia atrás.

// 2. Por ahora el arbol caido se spawnea cada 3 o 4 bosques, y es necesario que solo se spawnee una vez, despues al implementar más prefabs especiales estes serán los siguientes en aparecer. Para solucionar esto, se podría implementar un sistema de control de prefabs especiales que determine cuál debe aparecer a continuación, en lugar de depender únicamente del conteo de bosques.