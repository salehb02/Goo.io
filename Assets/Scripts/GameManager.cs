using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public VariableJoystick joyStick;
    public PlayerData startPlayer;
    public List<PlayerData> players = new List<PlayerData>();
    public string[] nicknames;

    [Space(2)]
    [Header("Captuables Objects")]
    public CapturableObject[] capturablesObjects;
    public CapturableSpawnPoint[] capturableSpawnPoints { get; private set; }
    public bool removeLockedCapturables = false;
    public int capturableObjectsCount = 10;

    private IJoystickControllable _currentControllable;
    private CameraFollower _camera;
    private GameManagerPresentor _presentor;
    private bool _finish = false;

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
        capturableSpawnPoints = FindObjectsOfType<CapturableSpawnPoint>();

        UpdateCameraFollower();
        GetPlayers();
        ExitToGoo();
        LoadSavedUsername();
    }

    private void Update()
    {
        if (!_finish)
            _currentControllable.Movement(new Vector3(joyStick.Horizontal, 0, joyStick.Vertical));
        else
            _currentControllable.Movement(Vector3.zero);

        _presentor.SetAlivePlayersCount(players.Count - 1);
        CheckEndGame();
        SpawnCapturables();
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

            if (players[i].Enemy)
            {
                players[i].SetName(nicknames[Random.Range(0, nicknames.Length)]);
                var ai = players[i].gameObject.GetComponent<AIController>();
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
        if (SpawnedCapturables.Count >= capturableObjectsCount)
            return;

        var pos = capturableSpawnPoints[Random.Range(0, capturableSpawnPoints.Length)];
        var colliders = Physics.OverlapSphere(pos.transform.position, 1f);
        var nonStaticCollidersCount = 0;

        foreach (var collider in colliders)
        {
            if (collider.gameObject.layer == LayerMask.NameToLayer("Floor") || collider.gameObject.layer == LayerMask.NameToLayer("Wall"))
                continue;

            nonStaticCollidersCount++;
        }

        if (nonStaticCollidersCount > 0)
            return;

        var usableCapturables = capturablesObjects.ToList();

        if (removeLockedCapturables)
        {
            foreach (var prizes in ControlPanel.Instance.prizes)
            {
                foreach (var capturable in usableCapturables)
                {
                    if (capturable == prizes.capturable && PlayerPrefs.GetInt(prizes.id) != 1)
                    {
                        usableCapturables.Remove(capturable);
                    }
                }
            }
        }

        var cap = Instantiate(usableCapturables[Random.Range(0, usableCapturables.Count)], pos.transform.position, pos.transform.rotation, transform);

        SpawnedCapturables.Add(cap);
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
        _finish = true;
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
                PlayerPrefs.SetInt(CURRENT_PRIZE, 1);
                PlayerPrefs.SetFloat(PRIZE_LAST_PERCENT, 0);

                var nextPrizeIDIndex = ControlPanel.Instance.prizes.ToList().FindIndex(x => x.id == PlayerPrefs.GetString(CURRENT_PRIZE));

                if (nextPrizeIDIndex != -1)
                {
                    if (nextPrizeIDIndex < ControlPanel.Instance.prizes.Length - 1)
                        nextPrizeIDIndex++;

                    PlayerPrefs.SetString(CURRENT_PRIZE, ControlPanel.Instance.prizes[nextPrizeIDIndex].id);
                }
            }

            _finish = true;
            Player = null;
        }
    }
}