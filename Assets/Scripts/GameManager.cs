using UnityEngine;

public class GameManager : MonoBehaviour
{
    public FixedJoystick joyStick;
    public GooController startControllableObject;

    private IJoystickControllable _currentControllable;
    private CameraFollower _camera;

    private void Start()
    {
        _currentControllable = startControllableObject;
        _camera = FindObjectOfType<CameraFollower>();

        _camera.SetTarget(startControllableObject.transform);
    }

    private void Update()
    {
        _currentControllable.Movement(new Vector3(joyStick.Horizontal, 0, joyStick.Vertical));
    }
}