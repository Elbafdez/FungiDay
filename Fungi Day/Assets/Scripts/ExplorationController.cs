using UnityEngine;
using UnityEngine.InputSystem;

public class ExplorationController : MonoBehaviour
{
    [SerializeField] private BackgroundManager backgroundManager;
    

    void Update()
    {
        // Movimiento hacia delante con D o flecha derecha 
        if (Keyboard.current.dKey.isPressed) 
            { 
                backgroundManager.MoveForward(); 
            } 
        // Movimiento hacia atrás con A o flecha izquierda 
        else if (Keyboard.current.aKey.isPressed) 
        { 
            backgroundManager.MoveBackward(); 
        } 
    }
}
