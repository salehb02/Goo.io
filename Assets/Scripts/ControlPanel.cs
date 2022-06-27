using UnityEngine;

[CreateAssetMenu(menuName = "Current Project/Control Panel")]
public class ControlPanel : ScriptableObject
{
    public const string PLAYER_MONEY = "PLAYER_MONEY";

    public SFXBank AudioBank;

    [System.Serializable]
    public class SFXBank
    {
        public AudioClip starPickup;
    }

    #region Singleton
    private static ControlPanel instance;
    public static ControlPanel Instance
    {
        get => instance == null ? instance = Resources.Load("ControlPanel") as ControlPanel : instance;
    }
    #endregion
}