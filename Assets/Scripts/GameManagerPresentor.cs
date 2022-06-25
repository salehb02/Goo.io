using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManagerPresentor : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private Button ExitToGooButton;

    private GameManager _gameManager;

    private void Start()
    {
        _gameManager = GetComponent<GameManager>();
        ExitToGooButton.onClick.AddListener(_gameManager.ExitToGoo);
    }

    public void SetScoreText(int score) => scoreText.text = score.ToString();
    public void SetExitToGooActivation(bool active) => ExitToGooButton.gameObject.SetActive(active);
}