using UnityEngine;

public class PlayerData : MonoBehaviour, IJoystickControllable
{
    public Collider trigger;
    private GooController _gooController;
    private CustomCharacterController _characterController;
    private GameManager _gameManager;

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
            _gooController.gameObject.SetActive(true);
            transform.SetParent(_characterController.transform);
            transform.localPosition = Vector3.zero;
            Instantiate(_characterController.ragdoll, _characterController.transform.position, _characterController.transform.rotation, null);
            Destroy(_characterController.gameObject);
            _gameManager.UpdateCameraFollower();
        }
    }

    public void GetIntoCharacter(CustomCharacterController character)
    {
        _characterController = character;
        _gooController.gameObject.SetActive(false);
        _gooController.transform.SetParent(_characterController.transform);
        transform.SetParent(_characterController.transform);
        transform.localPosition = Vector3.zero;
        _gooMode = false;
        _gameManager.UpdateCameraFollower();
        _gameManager.EnterToCharacter();
        _gooController.Movement(Vector3.zero, true);
    }

    public Transform GetCameraTarget()
    {
        if (_gooMode)
            return _gooController.transform;

        return _characterController.transform;
    }
}