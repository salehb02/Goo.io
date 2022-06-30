using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System.Linq;

public class GameManagerPresentor : MonoBehaviour
{
    [Header("Gameplay")]
    [SerializeField] private GameObject gameplayUI;
    [SerializeField] private TextMeshProUGUI capturablsCount;
    [SerializeField] private Button ExitToGooButton;

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
    [SerializeField] private Slider progressBar;
    [SerializeField] private TextMeshProUGUI progressPercentText;
    [SerializeField] private TextMeshProUGUI prizeName;
    [SerializeField] private Image prizeIcon;

    private GameManager _gameManager;

    private void Start()
    {
        _gameManager = GetComponent<GameManager>();

        SetGameplayUIActivation(false);
        SetMenuUIActivation(true);
        SetLosePanelActivation(false);
        SetWinPanelActivation(false);

        ExitToGooButton.onClick.AddListener(_gameManager.ExitToGoo);
        startGame.onClick.AddListener(_gameManager.StartGame);
        nameInputfield.onValueChanged.AddListener(_gameManager.SetUsername);
        restartButton.onClick.AddListener(_gameManager.RestartLevel);
        nextLevelButton.onClick.AddListener(_gameManager.NextLevel);
    }

    public void SetCapturablesCount(int count) => capturablsCount.text = count.ToString();
    public void SetExitToGooActivation(bool active) => ExitToGooButton.gameObject.SetActive(active);
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

        winUI.SetActive(true);
        winText.transform.localScale = Vector3.zero;
        nextLevelButton.gameObject.SetActive(false);

        winText.transform.DOScale(1, 1).OnComplete(() =>
        {
            nextLevelButton.gameObject.SetActive(true);
            ProgressPrize(prizeId, lastPercent, lastPercent + ControlPanel.Instance.addPercentPerLevel);
        });
    }

    public void ProgressPrize(string prizeId,float lastPercent,float newPercent)
    {
        var currentPrize = ControlPanel.Instance.prizes.Single(x=> x.id == prizeId);

        if (currentPrize != null)
        {
            prizePercentPivot.gameObject.SetActive(true);
            prizeIcon.sprite = currentPrize.icon;
            prizeName.text = currentPrize.capturable.Name;

            progressBar.value = lastPercent;
            progressBar.maxValue = 100;
            progressBar.DOValue(newPercent, 1f);
            progressPercentText.DOCounter((int)lastPercent, (int)newPercent, 1f).OnUpdate(() => progressPercentText.text += "%");
        }
        else
        {
            prizePercentPivot.gameObject.SetActive(false);
        }
    }
}