using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System.Linq;
using Pathfinding;

public class GameManagerPresentor : MonoBehaviour
{
    [Header("Gameplay")]
    [SerializeField] private GameObject gameplayUI;
    [SerializeField] private TextMeshProUGUI alivePlayersCount;
    [SerializeField] private Button exitToGoo;

    [Space(2)]
    [Header("Menu")]
    [SerializeField] private GameObject menuUI;
    [SerializeField] private TMP_InputField nameInputfield;
    [SerializeField] private Button startGame;

    [Space(2)]
    [Header("Lose Panel")]
    [SerializeField] private GameObject loseUI;
    [SerializeField] private TextMeshProUGUI loseText;
    [SerializeField] private Button restartButton;

    [Space(2)]
    [Header("Win Panel")]
    [SerializeField] private GameObject winUI;
    [SerializeField] private TextMeshProUGUI winText;
    [SerializeField] private Button nextLevelButton;
    [Space(2)]
    [SerializeField] private GameObject prizePercentPivot;
    [SerializeField] private TextMeshProUGUI progressPercentText;
    [SerializeField] private Image prizeIconMask;
    [SerializeField] private GameObject prizeSpawnPosition;

    private GameManager _gameManager;

    private void Start()
    {
        _gameManager = GetComponent<GameManager>();

        SetGameplayUIActivation(false);
        SetMenuUIActivation(true);
        SetLosePanelActivation(false);
        SetWinPanelActivation(false);

        exitToGoo.onClick.AddListener(_gameManager.ExitToGoo);
        startGame.onClick.AddListener(_gameManager.StartGame);
        nameInputfield.onValueChanged.AddListener(_gameManager.SetUsername);
        restartButton.onClick.AddListener(_gameManager.RestartLevel);
        nextLevelButton.onClick.AddListener(_gameManager.NextLevel);
    }

    public void SetAlivePlayersCount(int count) => alivePlayersCount.text = count.ToString();
    public void SetExitToGooActivation(bool active) => exitToGoo.gameObject.SetActive(active);
    public void SetNameInputfield(string name) => nameInputfield.text = name;
    public void SetGameplayUIActivation(bool active) => gameplayUI.gameObject.SetActive(active);
    public void SetMenuUIActivation(bool active) => menuUI.gameObject.SetActive(active);

    public void SetLosePanelActivation(bool active)
    {
        if (!active)
        {
            loseUI.SetActive(false);
            Time.timeScale = 1;
            return;
        }

        SetGameplayUIActivation(false);
        loseUI.SetActive(true);
        loseText.transform.localScale = Vector3.zero;
        restartButton.gameObject.SetActive(false);

        loseText.transform.DOScale(1, 1).OnComplete(() =>
        {
            restartButton.gameObject.SetActive(true);
            Time.timeScale = 0;
        });
    }

    public void SetWinPanelActivation(bool active, string prizeId = null, float lastPercent = 0)
    {
        if (!active)
        {
            winUI.SetActive(false);
            return;
        }

        SetGameplayUIActivation(false);
        winUI.SetActive(true);
        winText.transform.localScale = Vector3.zero;
        nextLevelButton.gameObject.SetActive(false);
        prizePercentPivot.gameObject.SetActive(false);

        winText.transform.DOScale(1, 1).OnComplete(() =>
        {
            nextLevelButton.gameObject.SetActive(true);
            ProgressPrize(prizeId, lastPercent, lastPercent + ControlPanel.Instance.addPercentPerLevel);
        });
    }

    public void ProgressPrize(string prizeId, float lastPercent, float newPercent)
    {
        var currentPrize = ControlPanel.Instance.capturables.Single(x => x.id == prizeId);

        if (currentPrize != null && PlayerPrefs.GetInt(currentPrize.id) == 0)
        {
            prizePercentPivot.gameObject.SetActive(true);
            progressPercentText.DOCounter((int)lastPercent, (int)newPercent, 1f).OnUpdate(() => progressPercentText.text += "%");
            prizeIconMask.fillAmount = lastPercent / 100f;
            prizeIconMask.DOFillAmount(newPercent / 100f, 1f);

            // Instance capturable
            var capt = Instantiate(currentPrize.capturable, prizeSpawnPosition.transform.position, prizeSpawnPosition.transform.rotation, prizeSpawnPosition.transform);
            capt.GetComponent<CharacterController>().enabled = false;
            capt.GetComponent<AIPath>().enabled = false;
            capt.PreviewMode(ControlPanel.Instance.defaultColor);
        }
        else
        {
            prizePercentPivot.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        //if (Input.GetKeyDown(KeyCode.Space))
        //{
        //    _gameManager.ExitToGoo();
        //}
    }
}