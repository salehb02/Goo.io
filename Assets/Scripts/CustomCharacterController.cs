using UnityEngine;

public class CustomCharacterController : MonoBehaviour
{
    public float speed = 2f;
    public GameObject ragdoll;
    public SkinnedMeshRenderer skinnedMesh;
    public Color normalColor;

    private Animator _animator;
    private Vector3 _direction;
    private CharacterController _controller;
    private Vector3 _currentDirection;

    private void Start()
    {
        _animator = GetComponent<Animator>();
        _controller = GetComponent<CharacterController>();

        SetSkinColor(normalColor);
    }

    private void Update()
    {
        _currentDirection = Vector3.Lerp(_currentDirection, _direction, Time.deltaTime * 5f);

        // Move character
        _controller.SimpleMove(_currentDirection * speed);

        // Rotate transform to direction
        if(_currentDirection != Vector3.zero)
        transform.rotation = Quaternion.LookRotation(_currentDirection);

        // Set animator movement speed
        _animator.SetFloat("Speed", _currentDirection.magnitude);
    }

    public void Movement(Vector3 vector3)
    {
        _direction = vector3;
    }

    public void Capture(Color color)
    {
        SetSkinColor(color);
    }

    private void SetSkinColor(Color color)
    {
        var mats = skinnedMesh.sharedMaterials;
        foreach (var mat in mats)
            mat.SetColor("_BaseColor", color);
        skinnedMesh.materials = mats;
    }
}