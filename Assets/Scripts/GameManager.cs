using UnityEngine;

public class GameManager : MonoBehaviour
{
    public FixedJoystick joyStick;
    public PlayerData startPlayer;
    public PlayerData[] players;
    public string[] nicknames;

    private IJoystickControllable _currentControllable;
    private CameraFollower _camera;
    private GameManagerPresentor _presentor;

    public PlayerData Player { get; private set; }

    private void Start()
    {
        _currentControllable = startPlayer;
        Player = startPlayer;
        _camera = FindObjectOfType<CameraFollower>();
        _presentor = GetComponent<GameManagerPresentor>();

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
        players = FindObjectsOfType<PlayerData>();

        for (int i = 0; i < players.Length; i++)
        {
            players[i].Enemy = players[i] == Player ? false : true;

            if (players[i].Enemy)
            {
                players[i].SetName(nicknames[Random.Range(0, nicknames.Length)]);
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
}