using System.Collections.Generic;
using UnityEngine;

/*<summary>

Ficha de datos de una ruta completa: qué fondo va al principio, cuál se repite,
qué especiales hay y en qué orden, y qué setas pueden aparecer en esta ruta.

No es un MonoBehaviour: no se pega a ningún GameObject de la escena. Se crea como
un archivo (asset) dentro del proyecto, igual que un prefab, y luego se asigna en
el Inspector de BackgroundManager e InvestigationPointManager.

Para tener una ruta 2, no hay que tocar ningún script: se crea otro archivo de este
mismo tipo (por ejemplo "Ruta2"), se rellena con otros prefabs, y se asigna en la
escena de la ruta 2 en vez de este.

</summary>*/

[CreateAssetMenu(fileName = "NuevaRuta", menuName = "Rutas/Definición de ruta")]
public class RouteDefinition : ScriptableObject
{
    [Header("Fondo de inicio (siempre el primero de la ruta, sin aleatoriedad)")]
    public GameObject startPrefab;

    [Header("Fondo base que se repite (bosque, pradera, lo que sea)")]
    public GameObject basePrefab;

    [Header("Cuántos fondos base aparecen antes de cada especial")]
    public int minBaseBeforeSpecial = 4;
    public int maxBaseBeforeSpecial = 6;

    [Header("Fondos especiales, EN EL ORDEN en que deben aparecer (el último marca el final de la ruta)")]
    public List<GameObject> specialPrefabs = new List<GameObject>();

    [Header("Setas que pueden aparecer en esta ruta (y solo estas)")]
    public List<GameObject> investigationPointPrefabs = new List<GameObject>();

    [Header("Separación entre setas (en número de fondos)")]
    public int minBackgroundsBetweenPoints = 1;
    public int maxBackgroundsBetweenPoints = 2;
}