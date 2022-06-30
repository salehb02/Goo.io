using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public FixedJoystick joyStick;
    public PlayerData startPlayer;
    public List<PlayerData> players = new List<PlayerData>();
    public string[] nicknames;

    [Space(2)]
    [Header("Captuables Objects")]
    public CapturableObject[] capturablesObjects;
    public bool randomGenerate;
    public bool removeLockedCapturables = false;
    public GameObject spawnPointOrigin;
    public Vector2 horizontalRandomPositionRange;
    public Vector2 verticalRandomPositionRange;
    public int capturableObjectsCount = 10;

    private IJoystickControllable _currentControllable;
    private CameraFollower _camera;
    private GameManagerPresentor _presentor;

    public PlayerData Player { get; set; }
    public List<CapturableObject> SpawnedCapturables { get; private set; } = new List<CapturableObject>();

    public const string PLAYER_NAME = "PLAYER_NAME";
    public const string LAST_LEVEL = "LAST_LEVEL";
    public const string CURRENT_PRIZE = "CURRENT_PRIZE";
    public const string PRIZE_LAST_PERCENT = "PRIZE_LAST_PERCENT"; 

    private void Awake()
    {
        LoadLatestLevel();
    }

    private void Start()
    {
        _currentControllable = startPlayer;
        Player = startPlayer;
        _camera = FindObjectOfType<CameraFollower>();
        _presentor = GetComponent<GameManagerPresentor>();
        SpawnedCapturables = FindObjectsOfType<CapturableObject>().ToList();

        SpawnCapturables();
        if(removeLockedCapturables)
        CheckPrizeCapturables();
        UpdateCameraFollower();
        GetPlayers();
        ExitToGoo();
        LoadSavedUsername();
    }

    private void Update()
    {
        _currentControllable.Movement(new Vector3(joyStick.Horizontal, 0, joyStick.Vertical));
        _presentor.SetCapturablesCount(SpawnedCapturables.Count);
        CheckEndGame();
    }

    public void StartGame()
    {
        _presentor.SetGameplayUIActivation(true);
        _presentor.SetMenuUIActivation(false);

        foreach (var player in players)
        {
            var ai = player.GetComponent<AIController>();
            if (!ai)
                continue;

            ai.enabled = true;
        }
    }

    public void UpdateCameraFollower()
    {
        _camera.SetTarget(startPlayer.GetCameraTarget());
    }

    private void GetPlayers()
    {
        players = FindObjectsOfType<PlayerData>().ToList();

        for (int i = 0; i < players.Count; i++)
        {
            players[i].Enemy = players[i] == Player ? false : true;

            //players[i].SetName(nicknames[Random.Range(0, nicknames.Length)]);

            if (players[i].Enemy)
            {
                var ai = players[i].gameObject.AddComponent<AIController>();
                ai.enabled = false;
            }
        }
    }

    public void EnterToCharacter()
    {
        _presentor.SetExitToGooActivation(true);
    }

    public void ExitToGoo()
    {
        _presentor.SetExitToGooActivation(false);
        Player.SetToGoo();
    }

    private void SpawnCapturables()
    {
        if (!randomGenerate)
            return;

        for (int i = 0; i < capturableObjectsCount; i++)
        {
            var cap = Instantiate(capturablesObjects[Random.Range(0, capturablesObjects.Length)]
                , spawnPointOrigin.transform.position + new Vector3(Random.Range(horizontalRandomPositionRange.x, horizontalRandomPositionRange.y), 0, Random.Range(verticalRandomPositionRange.x, verticalRandomPositionRange.y)), Quaternion.identity, transform);

            SpawnedCapturables.Add(cap);
        }
    }

    private void LoadSavedUsername()
    {
        _presentor.SetNameInputfield(PlayerPrefs.GetString(PLAYER_NAME));
        Player.SetName(PlayerPrefs.GetString(PLAYER_NAME));
    }

    public void SetUsername(string name)
    {
        PlayerPrefs.SetString(PLAYER_NAME, name);
        Player.SetName(name);
    }

    public void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void NextLevel()
    {
        var nextLevelIndex = SceneManager.GetActiveScene().buildIndex;

        if (nextLevelIndex < SceneManager.sceneCount - 1)
            nextLevelIndex++;
        else
            nextLevelIndex = 0;

        PlayerPrefs.SetString(LAST_LEVEL, SceneManager.GetSceneByBuildIndex(nextLevelIndex).name);
        SceneManager.LoadScene(SceneManager.GetSceneByBuildIndex(nextLevelIndex).name);
    }

    private void LoadLatestLevel()
    {
        if (PlayerPrefs.GetString(LAST_LEVEL) == SceneManager.GetActiveScene().name || PlayerPrefs.GetString(LAST_LEVEL) == "")
            return;

        SceneManager.LoadScene(PlayerPrefs.GetString(LAST_LEVEL));
    }

    public void ShowLosePanel()
    {
        _presentor.SetLosePanelActivation(true);
    }

    public void CheckEndGame()
    {
        if (Player == null)
            return;

        if (players.Count - 1 <= 0)
        {
            if (!PlayerPrefs.HasKey(CURRENT_PRIZE))
                PlayerPrefs.SetString(CURRENT_PRIZE, ControlPanel.Instance.prizes[0].id);

            _presentor.SetWinPanelActivation(true, PlayerPrefs.GetString(CURRENT_PRIZE), PlayerPrefs.GetFloat(PRIZE_LAST_PERCENT));

            PlayerPrefs.SetFloat(PRIZE_LAST_PERCENT, PlayerPrefs.GetFloat(PRIZE_LAST_PERCENT) + ControlPanel.Instance.addPercentPerLevel);

            if (PlayerPrefs.GetFloat(PRIZE_LAST_PERCENT) >= 100)
            {
                PlayerPrefs.SetFloat(PRIZE_LAST_PERCENT, 0);

                var nextPrizeIDIndex = ControlPanel.Instance.prizes.ToList().FindIndex(x => x.id == PlayerPrefs.GetString(CURRENT_PRIZE));

                if (nextPrizeIDIndex != -1)
                {
                    if (nextPrizeIDIndex < ControlPanel.Instance.prizes.Length - 1)
                        nextPrizeIDIndex++;

                    PlayerPrefs.SetString(CURRENT_PRIZE, ControlPanel.Instance.prizes[nextPrizeIDIndex].id);
                }
            }

            Player = null;
        }
    }

    public void CheckPrizeCapturables()
    {
        foreach(var prizes in ControlPanel.Instance.prizes)
        {
            foreach (var capturable in SpawnedCapturables)
            {
                if (capturable == prizes.capturable && PlayerPrefs.GetInt(prizes.id) != 1)
                {
                    Destroy(capturable.gameObject);
                }
            }
        }
    }
}