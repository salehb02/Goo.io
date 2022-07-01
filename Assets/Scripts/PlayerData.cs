using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerData : MonoBehaviour, IJoystickControllable
{
    public CharacterDetector characterDetector;
    public int maxHealth = 10;

    [Space(2)]
    [Header("Coloring")]
    public CustomShaderColors colors;

    [Space(2)]
    [Header("UI")]
    public GameObject UIObject;
    public Slider healthbar;
    public TextMeshProUGUI nameText;

    private GooController _gooController;
    private GameManager _gameManager;

    public CapturableObject CapturableObject { get; private set; }
    public bool GooMode { get; private set; }
    public int Score { get; private set; }
    public float Health { get; private set; }
    public bool Enemy { get; set; }
    public string Name { get; private set; }

    [System.Serializable]
    public class CustomShaderColors
    {
        public Color mainColor;
        public Color goochBright;
        public Color goochDark;
        public Color rimBright;
        public Color rimDark;
    }

    private void Start()
    {
        _gameManager = FindObjectOfType<GameManager>();
        _gooController = GetComponentInParent<GooController>();

        _gooController.SetColor(colors);
        _gooController.ControlBy = this;
        InitHealth();
        InitUI();
        SetToGoo();
    }

    private void Update()
    {
        transform.localScale = Vector3.one;

        if (!UIObject)
            return;

        if (GooMode && _gooController)
            UIObject.transform.position = _gooController.transform.position + _gooController.UIOffset;
        else if (CapturableObject)
            UIObject.transform.position = CapturableObject.transform.position + CapturableObject.UIOffset;
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
            CapturableObject.Movement(vector3);
    }

    public void SetToGoo()
    {
        GooMode = true;

        if (CapturableObject)
        {
            _gooController.LeaveBody();

            _gameManager.UpdateCameraFollower();
            transform.SetParent(_gooController.transform);
            transform.localPosition = Vector3.zero;

            CapturableObject.LeaveObject();
            CapturableObject = null;
            UpdateUI();
            ResetHealth();
            characterDetector.gameObject.SetActive(true);
        }
    }

    public void GetIntoCharacter(CustomCharacterController character)
    {
        CapturableObject = character;

        if (!CapturableObject.Capturable())
            return;

        GooMode = false;
        _gameManager.UpdateCameraFollower();
        transform.SetParent(CapturableObject.transform);
        transform.localPosition = Vector3.zero;

        if (!Enemy)
        {
            _gameManager.EnterToCharacter();
            UpdateUI();
        }

        CapturableObject.Capture(this, colors);
        ResetHealth();

        _gooController.MoveToBody(CapturableObject.venomEntrance.transform.position, CapturableObject.venomEntrance.transform);
        characterDetector.gameObject.SetActive(false);
    }

    private void Die()
    {
        if (!GooMode)
            SetToGoo();

        if (_gameManager.players.Contains(this))
            _gameManager.players.Remove(this);

        Destroy(UIObject.gameObject);
        Destroy(_gooController.gameObject);

        if (!Enemy)
        {
            _gameManager.ShowLosePanel();
            _gameManager.Player = null;
        }
    }

    public Transform GetCameraTarget()
    {
        if (_gooController && GooMode)
            return _gooController.transform;

        if (CapturableObject)
            return CapturableObject.transform;

        return null;
    }
}