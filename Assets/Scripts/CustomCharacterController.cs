using System.Collections;
using UnityEngine;

public class CustomCharacterController : MonoBehaviour
{
    public float speed = 2f;
    public GameObject ragdoll;
    public SkinnedMeshRenderer skinnedMesh;
    public Color normalColor;
    public GameObject gooEnterance;

    [Space(2)]
    [Header("AI")]
    public GameObject[] patrolWaypoints;
    public float restInWaypointTime = 3f;
    private int _currentPointIndex;

    private Animator _animator;
    private Vector3 _direction;
    private CharacterController _controller;
    private Vector3 _currentDirection;

    private void Start()
    {
        _animator = GetComponent<Animator>();
        _controller = GetComponent<CharacterController>();

        SetSkinColor(normalColor);

        if (patrolWaypoints.Length > 0)
            StartCoroutine(AIPatrolCoroutine());
    }

    private void Update()
    {
        _currentDirection = Vector3.Lerp(_currentDirection, _direction, Time.deltaTime * 5f);

        // Move character
        _controller.SimpleMove(_currentDirection * speed);

        // Rotate transform to direction
        if (_currentDirection != Vector3.zero)
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(_currentDirection), Time.deltaTime * 10f);

        // Set animator movement speed
        _animator.SetFloat("Speed", _currentDirection.magnitude);
    }

    private IEnumerator AIPatrolCoroutine()
    {
        Movement((patrolWaypoints[_currentPointIndex].transform.position - transform.position).normalized / 2f);

        yield return new WaitUntil(() => Vector3.Distance(transform.position, patrolWaypoints[_currentPointIndex].transform.position) <= 1);

        Movement(Vector3.zero);

        if (_currentPointIndex < patrolWaypoints.Length - 1)
            _currentPointIndex++;
        else
            _currentPointIndex = 0;

        yield return new WaitForSeconds(restInWaypointTime);

        StartCoroutine(AIPatrolCoroutine());
    }

    public void Movement(Vector3 vector3, bool force = false)
    {
        _direction = vector3;

        if (force)
            _currentDirection = vector3;
    }

    public void Capture(Color color)
    {
        SetSkinColor(color);
        StopCoroutine(AIPatrolCoroutine());
        Movement(Vector3.zero, true);
    }

    private void SetSkinColor(Color color)
    {
        var mats = skinnedMesh.sharedMaterials;
        foreach (var mat in mats)
            mat.SetColor("_BaseColor", color);
        skinnedMesh.materials = mats;
    }
}