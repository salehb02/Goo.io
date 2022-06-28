using UnityEngine;
using DG.Tweening;

public class GooController : MonoBehaviour
{
    public float speed = 10;
    public float jumpForce = 1;
    public Vector3 UIOffset;

    public Renderer renderer;
    public ParticleSystem trailParticle;

    private Vector3 _direction;
    private Vector3 _currentDirection;
    private Rigidbody _rigid;
    private Vector3 _lastSavedPos;

    private void Start()
    {
        _rigid = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        _currentDirection = Vector3.Lerp(_currentDirection, _direction, Time.deltaTime * 5f);
        _rigid.AddForce(_currentDirection * speed);
    }

    public void Movement(Vector3 vector3, bool force = false)
    {
        _direction = vector3;

        if (force)
            _currentDirection = vector3;
    }

    public void MoveToBody(Vector3 pos, Transform parentOnDone)
    {
        DisableColliders();
        transform.SetParent(parentOnDone);
        SaveLastPositon(transform.localPosition);

        _rigid.DOMove(pos, 0.2f).OnComplete(() =>
        {
            gameObject.SetActive(false);
            Movement(Vector3.zero, true);
        });
    }

    public void LeaveBody()
    {
        LoadLastPosition();
        transform.SetParent(null);
        gameObject.SetActive(true);
        EnableColliders();
    }

    private void SaveLastPositon(Vector3 pos)
    {
        _lastSavedPos = pos;
    }

    public void LoadLastPosition()
    {
        transform.localPosition = _lastSavedPos;
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

    public void SetColor(Color color)
    {
        var mat = new Material(renderer.sharedMaterial);
        mat.SetColor("_BaseColor", color);
        renderer.material = mat;

        var particleRenderer = trailParticle.GetComponent<Renderer>();
        var particleMat = new Material(particleRenderer.sharedMaterial);
        particleMat.SetColor("_BaseColor", color);
        particleRenderer.material = particleMat;
    }
}