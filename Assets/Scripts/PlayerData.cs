using System.Collections;
using UnityEngine;

public class PlayerData : MonoBehaviour, IJoystickControllable
{
    public Collider trigger;
    public Color gooColor;

    private GooController _gooController;
    private CustomCharacterController _characterController;
    private GameManager _gameManager;
    private Vector3 _lastGooPosition;

    private bool _gooMode;

    public int Score { get; private set; }

    private void Start()
    {
        _gameManager = FindObjectOfType<GameManager>();
        _gooController = GetComponentInParent<GooController>();

        SetToGoo();
    }

    private void Update()
    {
        trigger.transform.rotation = Quaternion.identity;
    }

    public void AddScore(int count = 1)
    {
        Score += count;
    }

    public void Movement(Vector3 vector3)
    {
        if (_gooMode)
            _gooController.Movement(vector3);
        else
            _characterController.Movement(vector3);
    }

    public void SetToGoo()
    {
        _gooMode = true;

        if(_characterController)
        {
            _gooController.transform.SetParent(null);
            _gooController.EnableColliders();
            _gooController.gameObject.SetActive(true);
            _gooController.transform.position = _lastGooPosition;
            _gameManager.UpdateCameraFollower();
            transform.SetParent(_characterController.transform);
            transform.localPosition = Vector3.zero;
            
            // Instantiate ragdoll
            var ragdoll = Instantiate(_characterController.ragdoll, _characterController.transform.position, _characterController.transform.rotation, null);
            var ragdollSkin = ragdoll.GetComponentInChildren<SkinnedMeshRenderer>();
            var ragdollMats = ragdollSkin.sharedMaterials;
            foreach (var mat in ragdollMats)
                mat.SetColor("_BaseColor", _characterController.normalColor);
            ragdollSkin.materials = ragdollMats;

            Destroy(_characterController.gameObject);
        }
    }

    public void GetIntoCharacter(CustomCharacterController character)
    {
        _characterController = character;
        _gooMode = false;
        _gameManager.UpdateCameraFollower();
        transform.SetParent(_characterController.transform);
        transform.localPosition = Vector3.zero;
        _gameManager.EnterToCharacter();
        _characterController.Capture(gooColor);

        _lastGooPosition = _gooController.transform.position;
        _gooController.MoveToBody(_characterController.gooEnterance.transform.position, _characterController.gooEnterance.transform);
    }

    public Transform GetCameraTarget()
    {
        if (_gooMode)
            return _gooController.transform;

        return _characterController.transform;
    }
}