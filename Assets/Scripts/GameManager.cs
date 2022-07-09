using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public VariableJoystick joyStick;
    public PlayerData startPlayer;
    public bool loadLastLevel = true;
    public bool unlockAll = false;

    [Space(2)]
    [Header("Captuables Objects")]
    public CapturableObject[] capturablesObjects;
    public int capturableObjectsCount = 10;


    private IJoystickControllable _currentControllable;
    private CameraFollower _camera;
    private GameManagerPresentor _presentor;
    private bool _finish = false;

    public PlayerData Player { get; set; }
    public List<CapturableObject> SpawnedCapturables { get; private set; } = new List<CapturableObject>();
    public List<PlayerData> CurrentPlayers { get; private set; } = new List<PlayerData>();
    public CapturableSpawnPoint[] capturableSpawnPoints { get; private set; }

    public const string PLAYER_NAME = "PLAYER_NAME";
    public const string LAST_LEVEL = "LAST_LEVEL";
    public const string CURRENT_PRIZE = "CURRENT_PRIZE";
    public const string PRIZE_LAST_PERCENT = "PRIZE_LAST_PERCENT";

    private void Awake()
    {
        if (loadLastLevel)
        {
            LoadLatestLevel();
        }
        if(unlockAll)
        {
            for (int i = 0; i < ControlPanel.Instance.capturables.Length; i++)
            {
                ControlPanel.Instance.capturables[i].unlocked = true;
                   
            }

        }

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


        StartCoroutine(SpawnCapturableAtTime());
    }

    private void Update()
    {
        if (!_finish)
            _currentControllable.Movement(new Vector3(joyStick.Horizontal, 0, joyStick.Vertical));
        else
            _currentControllable.Movement(Vector3.zero);

        _presentor.SetAlivePlayersCount(CurrentPlayers.Count - 1);
        CheckEndGame();
       // SpawnCapturables();
    }

    public void StartGame()
    {
        _presentor.SetGameplayUIActivation(true);
        _presentor.SetMenuUIActivation(false);

        foreach (var player in CurrentPlayers)
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
        CurrentPlayers = FindObjectsOfType<PlayerData>().ToList();

        for (int i = 0; i < CurrentPlayers.Count; i++)
        {
            CurrentPlayers[i].Enemy = CurrentPlayers[i] == Player ? false : true;

            if (CurrentPlayers[i].Enemy)
            {
                CurrentPlayers[i].SetName(ControlPanel.Instance.nicknames[Random.Range(0, ControlPanel.Instance.nicknames.Length)]);
                var ai = CurrentPlayers[i].gameObject.GetComponent<AIController>();
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

        if (ControlPanel.Instance.removeLockedCapturables)
        {
            foreach (var prizes in ControlPanel.Instance.capturables)
            {
                if (prizes.unlocked)
                    continue;

                foreach (var capturable in usableCapturables.ToList())
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

    IEnumerator<WaitForSeconds> SpawnCapturableAtTime()
    {
        while (true)
        {
            yield return new WaitForSeconds(UnityEngine.Random.Range(0.5f,1));

            SpawnCapturables();
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

        if (nextLevelIndex < SceneManager.sceneCountInBuildSettings - 1)
            nextLevelIndex++;
        else if (nextLevelIndex == SceneManager.sceneCountInBuildSettings - 1)
            nextLevelIndex = UnityEngine.Random.Range(0, SceneManager.sceneCountInBuildSettings);
        else
            nextLevelIndex = 0;

        PlayerPrefs.SetInt(LAST_LEVEL, nextLevelIndex);
        SceneManager.LoadScene(nextLevelIndex);
    }

    private void LoadLatestLevel()
    {
        if (PlayerPrefs.GetInt(LAST_LEVEL) == SceneManager.GetActiveScene().buildIndex || PlayerPrefs.GetInt(LAST_LEVEL) == -1)
            return;

        SceneManager.LoadScene(PlayerPrefs.GetInt(LAST_LEVEL));
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

        if (CurrentPlayers.Count - 1 <= 0)
        {
            if (!PlayerPrefs.HasKey(CURRENT_PRIZE))
            {
                var firstLocked = ControlPanel.Instance.capturables.FirstOrDefault(x => x.unlocked == false);
                PlayerPrefs.SetString(CURRENT_PRIZE, firstLocked.id);
            }

            _presentor.SetWinPanelActivation(true, PlayerPrefs.GetString(CURRENT_PRIZE), PlayerPrefs.GetFloat(PRIZE_LAST_PERCENT));

            PlayerPrefs.SetFloat(PRIZE_LAST_PERCENT, PlayerPrefs.GetFloat(PRIZE_LAST_PERCENT) + ControlPanel.Instance.addPercentPerLevel);

            if (PlayerPrefs.GetFloat(PRIZE_LAST_PERCENT) >= 100)
            {
                PlayerPrefs.SetInt(CURRENT_PRIZE, 1);
                PlayerPrefs.SetFloat(PRIZE_LAST_PERCENT, 0);

                var nextPrizeIDIndex = ControlPanel.Instance.capturables.ToList().FindIndex(x => x.id == PlayerPrefs.GetString(CURRENT_PRIZE));

                if (nextPrizeIDIndex != -1)
                {
                    for (int i = nextPrizeIDIndex; i < ControlPanel.Instance.capturables.Length; i++)
                    {
                        if (ControlPanel.Instance.capturables[i].unlocked == false)
                            nextPrizeIDIndex = i;
                    }

                    PlayerPrefs.SetString(CURRENT_PRIZE, ControlPanel.Instance.capturables[nextPrizeIDIndex].id);
                }
            }

            _finish = true;
            Player = null;
        }
    }
}