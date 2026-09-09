using UnityEngine;

public class ExplorationController : MonoBehaviour
{
    [SerializeField] private BackgroundManager backgroundManager;
    
    void Update()
    {
        if (Input.GetKey(KeyCode.D))
        {
            backgroundManager.MoveForward();
        }
        else if (Input.GetKey(KeyCode.A))
        {
            backgroundManager.MoveBackward();
        }
    }
}
