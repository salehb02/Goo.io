using UnityEngine;

public class CapturableObject : MonoBehaviour
{
    [Header("Global Settings")]
    public string Name;
    public GameObject venomEntrance;
    public Vector3 UIOffset;

    public Vector3 Direction { get; private set; }
    public Vector3 SmoothedDirection { get; set; }
    public PlayerData ControllingBy { get; set; }
    public GameManager GameManager { get; private set; }
    public PlayerData Target { get; set; }
    public Vector3 AIDestination { get; set; }


    private void Start()
    {
        Init();
    }

    public virtual void Init()
    {
        GameManager = FindObjectOfType<GameManager>();
    }

    public virtual void Movement(Vector3 direction, bool force = false)
    {
        Direction = direction;

        if (force)
            SmoothedDirection = direction;
    }

    public virtual void Capture(PlayerData controlBy, PlayerData.CustomShaderColors colors)
    {
        if (!Capturable())
            return;

        Movement(Vector3.zero, true);
        ControllingBy = controlBy;

        if (GameManager.SpawnedCapturables.Contains(this))
            GameManager.SpawnedCapturables.Remove(this);
    }

    public bool Capturable() => ControllingBy ? false : true;

    public virtual void LeaveObject()
    {
        ControllingBy = null;
    }
}