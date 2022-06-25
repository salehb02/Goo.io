using UnityEngine;
using DG.Tweening;

public class GooController : MonoBehaviour
{
    public float speed = 10;
    public float jumpForce = 1;

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

    public void MoveToBody(Vector3 pos,Transform parentOnDone)
    {
        DisableColliders();

        _rigid.DOMove(pos, 0.2f).OnComplete(() => 
        {
            gameObject.SetActive(false);
            transform.SetParent(parentOnDone);
            Movement(Vector3.zero, true);
        });
    }

    public void EnableColliders()
    {
        foreach (var collider in GetComponentsInChildren<Collider>())
            collider.enabled = true;
    }

    public void DisableColliders()
    {
        foreach (var collider in GetComponentsInChildren<Collider>())
            collider.enabled = false;
    }
}