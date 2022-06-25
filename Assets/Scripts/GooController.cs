using UnityEngine;

public class GooController : MonoBehaviour
{
    public float speed = 10;

    private Vector3 _direction;
    private Vector3 _currentDirection;
    private Rigidbody _rigid;

    private void Start()
    {
        _rigid = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        _currentDirection = Vector3.Lerp(_currentDirection, _direction, Time.deltaTime * 5f);
        _rigid.AddForce(_currentDirection * speed);
    }

    public void Movement(Vector3 vector3,bool force = false)
    {
        _direction = vector3;

        if (force)
            _currentDirection = vector3;
    }
}