using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float step = 10f;

    private float initialY;
    private Vector3 initial;

    void Start()
    {
        initialY = transform.position.y;
        initial = transform.position;
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            Der();
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            Izq();
        }
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            Arriba();
        }
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            Abajo();
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            Der();
        }
        if (Input.GetKeyDown(KeyCode.A))
        {
            Izq();
        }
        if (Input.GetKeyDown(KeyCode.W))
        {
            Arriba();
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            Abajo();
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Reiniciar();
        }
    }

    public void Der()
    {
        Vector3 newPosition = transform.position;
        newPosition.x += step;
        transform.position = newPosition;
    }

    public void Izq()
    {
        Vector3 newPosition = transform.position;
        newPosition.x -= step;
        transform.position = newPosition;
    }

    public void Arriba()
    {
        Vector3 newPosition = transform.position;
        newPosition.z += step;
        transform.position = newPosition;
    }

    public void Abajo()
    {
        Vector3 newPosition = transform.position;
        newPosition.z -= step;
        transform.position = newPosition;
    }

    public void Reiniciar()
    {
        transform.position = initial;
        Camera cam = GetComponent<Camera>();
        cam.fieldOfView = 50f;
    }

}