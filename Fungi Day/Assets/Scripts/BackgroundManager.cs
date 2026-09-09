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

    private List<GameObject> activeBackgrounds = new List<GameObject>();

    private int forestCount = 0;
    private int forestsBeforeSpecial;

    void Start()
    {
        // Elegimos si aparecerán 3 o 4 bosques antes del árbol caído
        forestsBeforeSpecial = Random.Range(3, 5);

        // Creamos los fondos iniciales
        for (int i = 0; i < initialBackgrounds; i++)
        {
            SpawnNextBackground(i * backgroundWidth);
        }
    }

    // =========================
    // MOVIMIENTO HACIA DELANTE
    // =========================

    public void MoveForward()
    {
        foreach (GameObject background in activeBackgrounds)
        {
            BackgroundScroller scroller = background.GetComponent<BackgroundScroller>();

            scroller.MoveForward();
        }

        CheckForwardBackgrounds();
    }

    // =========================
    // MOVIMIENTO HACIA ATRÁS
    // =========================

    public void MoveBackward()
    {
        foreach (GameObject background in activeBackgrounds)
        {
            BackgroundScroller scroller = background.GetComponent<BackgroundScroller>();
            scroller.MoveBackward();
        }

        CheckBackwardBackgrounds();
    }

    // =========================
    // COMPROBAR FONDOS IZQUIERDA
    // =========================

    private void CheckForwardBackgrounds()  
    // Este método verifica si el primer fondo activo ha salido completamente de la pantalla hacia la izquierda. Si es así, lo destruye y genera un nuevo fondo al final de la lista.
    {
        GameObject firstBackground = activeBackgrounds[0];

        if (firstBackground.transform.position.x <= -backgroundWidth)
        {
            Destroy(firstBackground);

            activeBackgrounds.RemoveAt(0);

            float newPosition =
                activeBackgrounds[activeBackgrounds.Count - 1]
                .transform.position.x + backgroundWidth;

            SpawnNextBackground(newPosition);
        }
    }

    // =========================
    // COMPROBAR FONDOS DERECHA
    // =========================

    private void CheckBackwardBackgrounds()
    // Este método verifica si el último fondo activo ha salido completamente de la pantalla hacia la derecha. Si es así, lo destruye y genera un nuevo fondo al principio de la lista.
    {
        GameObject lastBackground =
            activeBackgrounds[activeBackgrounds.Count - 1];

        if (lastBackground.transform.position.x >= backgroundWidth)
        {
            Destroy(lastBackground);

            activeBackgrounds.RemoveAt(
                activeBackgrounds.Count - 1
            );

            float newPosition =
                activeBackgrounds[0].transform.position.x - backgroundWidth;

            SpawnNextBackgroundAtBeginning(newPosition);
        }
    }

    // =========================
    // CREAR SIGUIENTE FONDO
    // =========================

    private void SpawnNextBackground(float positionX)
    // Este método decide qué prefab de fondo crear (bosque o árbol caído) y lo instancia en la posición especificada. Luego, lo agrega a la lista de fondos activos.
    {
        GameObject prefabToSpawn;

        if (forestCount < forestsBeforeSpecial)
        {
            prefabToSpawn = forestPrefab;
            forestCount++;
        }
        else
        {
            prefabToSpawn = fallenTreePrefab;

            // Reiniciamos el ciclo
            forestCount = 0;

            // El siguiente grupo tendrá 3 o 4 bosques
            forestsBeforeSpecial = Random.Range(3, 5);
        }

        GameObject newBackground = Instantiate(
            prefabToSpawn,
            new Vector3(positionX, 0, 0),
            Quaternion.identity
        );

        activeBackgrounds.Add(newBackground);
    }

    // =========================
    // CREAR FONDO A LA IZQUIERDA
    // =========================

    private void SpawnNextBackgroundAtBeginning(float positionX)
    // Este método crea un nuevo fondo (actualmente siempre un bosque) al principio de la lista de fondos activos, en la posición especificada.
    {
        // Por ahora creamos bosque al volver hacia atrás.
        
        GameObject newBackground = Instantiate(
            forestPrefab,
            new Vector3(positionX, 0, 0),
            Quaternion.identity
        );

        activeBackgrounds.Insert(0, newBackground);
    }
}

// ========================= PROBLEMAS =========================
// 1. Cuando el jugador se mueve hacia atrás MUEVE HACIA ATRÁS, los fondos no se generan correctamente, solo se REPITE EL BOSQUE. 
//Esto se debe a que el método SpawnNextBackgroundAtBeginning() siempre instancia un bosque, sin considerar la lógica de cuántos bosques deben aparecer antes del árbol caído. Para solucionar esto, se podría implementar una lógica similar a la de SpawnNextBackground() para decidir qué prefab instanciar al moverse hacia atrás.

// 2. Por ahora el arbol caido se spawnea cada 3 o 4 bosques, y es necesario que solo se spawnee una vez, despues al implementar más prefabs especiales estes serán los siguientes en aparecer. Para solucionar esto, se podría implementar un sistema de control de prefabs especiales que determine cuál debe aparecer a continuación, en lugar de depender únicamente del conteo de bosques.