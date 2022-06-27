using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerData : MonoBehaviour, IJoystickControllable
{
    public Color gooColor;
    public CharacterDetector characterDetector;
    public int maxHealth = 10;

    [Space(2)]
    [Header("UI")]
    public GameObject UIObject;
    public Slider healthbar;
    public TextMeshProUGUI nameText;

    private GooController _gooController;
    private CapturableObject _capturableObject;
    private GameManager _gameManager;

    public bool GooMode { get; private set; }
    public int Score { get; private set; }
    public float Health { get; private set; }
    public bool Enemy { get; set; }
    public string Name { get; private set; }

    private void Start()
    {
        _gameManager = FindObjectOfType<GameManager>();
        _gooController = GetComponentInParent<GooController>();

        InitHealth();
        InitUI();
        SetToGoo();
    }

    private void Update()
    {
        if (GooMode && _gooController)
            UIObject.transform.position = _gooController.transform.position + _gooController.UIOffset;
        else if (_capturableObject)
            UIObject.transform.position = _capturableObject.transform.position + _capturableObject.UIOffset;
    }

    private void InitHealth()
    {
        Health = maxHealth;
        healthbar.maxValue = maxHealth;
    }

    private void InitUI()
    {
        UIObject.transform.SetParent(null);
        UpdateUI();
    }

    public void AddScore(int count = 1)
    {
        Score += count;
    }

    public bool Damage(float amount = 1)
    {
        Health -= amount;
        UpdateUI();

        if (Health <= 0)
        {
            Die();
            return true;
        }

        return false;
    }

    private void ResetHealth()
    {
        Health = maxHealth;
        UpdateUI();
    }

    public void UpdateUI()
    {
        healthbar.value = Health;
        nameText.text = Name;
    }

    public void SetName(string name)
    {
        Name = name;
        UpdateUI();
    }

    public void Movement(Vector3 vector3)
    {
        if (GooMode)
            _gooController.Movement(vector3);
        else
            _capturableObject.Movement(vector3);
    }

    public void SetToGoo()
    {
        GooMode = true;

        if (_capturableObject)
        {
            _gooController.LeaveBody();

            _gameManager.UpdateCameraFollower();
            transform.SetParent(_gooController.transform);
            transform.localPosition = Vector3.zero;

            _capturableObject.LeaveObject();
            _capturableObject = null;
            UpdateUI();
            ResetHealth();
            characterDetector.gameObject.SetActive(true);
        }
    }

    public void GetIntoCharacter(CustomCharacterController character)
    {
        _capturableObject = character;

        if (!_capturableObject.Capturable())
            return;

        GooMode = false;
        _gameManager.UpdateCameraFollower();
        transform.SetParent(_capturableObject.transform);
        transform.localPosition = Vector3.zero;
        _gameManager.EnterToCharacter();
        _capturableObject.Capture(this, gooColor);
        ResetHealth();

        _gooController.MoveToBody(_capturableObject.venomEntrance.transform.position, _capturableObject.venomEntrance.transform);
        UpdateUI();
        characterDetector.gameObject.SetActive(false);
    }

    private void Die()
    {
        if (!GooMode)
            SetToGoo();

        Destroy(UIObject.gameObject);
        Destroy(_gooController.gameObject);

        if (!Enemy)
        {
            // TODO: some player stuff
        }
    }

    public Transform GetCameraTarget()
    {
        if (GooMode)
            return _gooController.transform;

        return _capturableObject.transform;
    }
}