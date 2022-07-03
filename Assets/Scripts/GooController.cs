using UnityEngine;
using DG.Tweening;
using MK.Toon;
using Pathfinding;

public class GooController : MonoBehaviour
{
    public float maxSpeed = 10;
    public float acceleration = 10;
    public float jumpForce = 1;
    public Vector3 UIOffset;

    public new Renderer renderer;
    public ParticleSystem trailParticle;
    public CapsuleCollider extraCollider;

    private Vector3 _direction;
    private Vector3 _currentDirection;
    private Rigidbody _rigid;
    private Vector3 _lastSavedPos;
    private Vector3 _initSize;

    public PlayerData ControlBy { get; set; }
    public Vector3 AIDestination { get; set; }
    private AIPath _aiPath;
    public AIPath AIPath { get { return _aiPath == null ? _aiPath = GetComponent<AIPath>() : _aiPath; } set { } }

    private void Start()
    {
        _rigid = GetComponent<Rigidbody>();

        if (AIPath)
            AIPath.enabled = false;
        _initSize = transform.localScale;
    }

    private void FixedUpdate()
    {
        if (!AIPath || AIPath.enabled == false)
        {
            _currentDirection = Vector3.Lerp(_currentDirection, _direction, Time.deltaTime * 5f);

            var force = _currentDirection * acceleration;
            var velocity = _rigid.velocity + force;
            _rigid.velocity = Vector3.ClampMagnitude(velocity, maxSpeed);

            if (_direction == Vector3.zero)
                _rigid.velocity = _rigid.velocity * 0.9f;
        }
    }

    private void Update()
    {
        if (AIPath && AIPath.enabled == true)
        {
            AIPath.destination = AIDestination;
        }

        extraCollider.transform.rotation = Quaternion.identity;
        //extraCollider.transform.localPosition = Vector3.zero;
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