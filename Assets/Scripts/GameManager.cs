using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public FixedJoystick joyStick;
    public PlayerData startPlayer;
    public List<PlayerData> players = new List<PlayerData>();
    public string[] nicknames;

    [Space(2)]
    [Header("Captuables Objects")]
    public CapturableObject[] capturablesObjects;
    public GameObject spawnPointOrigin;
    public Vector2 horizontalRandomPositionRange;
    public Vector2 verticalRandomPositionRange;
    public int capturableObjectsCount = 10;

    private IJoystickControllable _currentControllable;
    private CameraFollower _camera;
    private GameManagerPresentor _presentor;

    public PlayerData Player { get; private set; }
    public List<CapturableObject> SpawnedCapturables { get; private set; } = new List<CapturableObject>();

    private void Start()
    {
        _currentControllable = startPlayer;
        Player = startPlayer;
        _camera = FindObjectOfType<CameraFollower>();
        _presentor = GetComponent<GameManagerPresentor>();

        SpawnCapturables();
        UpdateCameraFollower();
        GetPlayers();
        ExitToGoo();
    }

    private void Update()
    {
        _currentControllable.Movement(new Vector3(joyStick.Horizontal, 0, joyStick.Vertical));

        _presentor.SetScoreText(Player.Score);
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

            players[i].SetName(nicknames[Random.Range(0, nicknames.Length)]);

            if (players[i].Enemy)
            {
                players[i].gameObject.AddComponent<AIController>();
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
        for (int i = 0; i < capturableObjectsCount; i++)
        {
            var cap = Instantiate(capturablesObjects[Random.Range(0, capturablesObjects.Length)]
                , spawnPointOrigin.transform.position + new Vector3(Random.Range(horizontalRandomPositionRange.x, horizontalRandomPositionRange.y), 0, Random.Range(verticalRandomPositionRange.x, verticalRandomPositionRange.y)), Quaternion.identity, transform);

            SpawnedCapturables.Add(cap);
        }
    }
}