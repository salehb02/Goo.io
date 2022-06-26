using UnityEngine;

public class CapturableObject : MonoBehaviour
{
    [Header("Global Settings")]
    public GameObject venomEntrance;
    public Vector3 UIOffset;

    public Vector3 Direction { get; private set; }
    public Vector3 SmoothedDirection { get; set; }
    public PlayerData ControllingBy { get; set; }

    public virtual void Movement(Vector3 direction, bool force = false)
    {
        Direction = direction;

        if (force)
            SmoothedDirection = direction;
    }

    public virtual void Capture(PlayerData controlBy, Color venomColor)
    {
        if (!Capturable())
            return;

        Movement(Vector3.zero, true);
        ControllingBy = controlBy;
    }

    public bool Capturable() => ControllingBy ? false : true;

    public virtual void LeaveObject()
    {
        ControllingBy = null;
    }
}