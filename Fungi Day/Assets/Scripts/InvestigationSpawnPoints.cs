using UnityEngine;

public class InvestigationSpawnPoints : MonoBehaviour
{
    [Header("Puntos donde pueden aparecer investigaciones")]

    [SerializeField] private Transform[] spawnPoints;


    // Este método permite que otros scripts obtengan la lista de Spawn Points.
    public Transform[] GetSpawnPoints()
    {
        return spawnPoints;
    }
}