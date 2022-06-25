using UnityEngine;

public class GooController : MonoBehaviour, IJoystickControllable
{
    public float speed = 10;

    private Vector3 _direction;
    private Rigidbody _rigid;

    private void Start()
    {
        _rigid = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        _rigid.AddForce(_direction * speed);
    }

    public void Movement(Vector3 vector3)
    {
        _direction = vector3;
    }
}