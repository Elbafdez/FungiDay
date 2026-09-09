using UnityEngine;

public class BackgroundScroller : MonoBehaviour
{
    [SerializeField] public float speed = 5f;
    public void MoveForward()
    {
        transform.Translate(Vector3.left * speed * Time.deltaTime);
    }

    public void MoveBackward()
    {
        transform.Translate(Vector3.right * speed * Time.deltaTime);
    }
}