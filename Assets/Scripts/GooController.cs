using UnityEngine;
using DG.Tweening;
using MK.Toon;

public class GooController : MonoBehaviour
{
    public float maxSpeed = 10;
    public float acceleration = 10;
    public float jumpForce = 1;
    public Vector3 UIOffset;

    public Renderer renderer;
    public ParticleSystem trailParticle;

    private Vector3 _direction;
    private Vector3 _currentDirection;
    private Rigidbody _rigid;
    private Vector3 _lastSavedPos;
    private Vector3 _initSize;

    private void Start()
    {
        _rigid = GetComponent<Rigidbody>();
        _initSize = transform.localScale;
    }

    private void FixedUpdate()
    {
        _currentDirection = Vector3.Lerp(_currentDirection, _direction, Time.deltaTime * 5f);
        if (_rigid.velocity.magnitude < maxSpeed)
            _rigid.AddForce(_currentDirection * acceleration);
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

        _rigid.DOMove(pos, 0.7f).OnComplete(() =>
        {
            gameObject.SetActive(false);
            Movement(Vector3.zero, true);
        });

        transform.DOScale(Vector3.zero, 0.7f);
    }

    public void LeaveBody()
    {
        transform.DOScale(_initSize, 0);
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

    public void SetColor(PlayerData.CustomShaderColors colors)
    {
        var mat = new Material(renderer.sharedMaterial);
        Properties.albedoColor.SetValue(mat, colors.mainColor);
        Properties.goochBrightColor.SetValue(mat, colors.goochBright);
        Properties.goochDarkColor.SetValue(mat, colors.goochDark);
        Properties.rimBrightColor.SetValue(mat, colors.rimBright);
        Properties.rimDarkColor.SetValue(mat, colors.rimDark);
        renderer.material = mat;

        var particleRenderer = trailParticle.GetComponent<Renderer>();
        var particleMat = new Material(particleRenderer.sharedMaterial);
        particleMat.SetColor("_BaseColor", colors.mainColor);
        particleRenderer.material = particleMat;
    }
}