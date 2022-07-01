using UnityEngine;

[CreateAssetMenu(menuName = "Current Project/Control Panel")]
public class ControlPanel : ScriptableObject
{
    public const string PLAYER_MONEY = "PLAYER_MONEY";
    public const string RAGDOLL_LAYER = "Ragdoll";

    public SFXBank AudioBank;
    public PlayerData.CustomShaderColors defaultColor;

    [Header("Prizes")]
    public Prize[] prizes;
    public float addPercentPerLevel = 25f;
    
    public class SFXBank
    {
        public AudioClip starPickup;
    }

    [System.Serializable]
    public class Prize
    {
        public string id;
        public CapturableObject capturable;
        public Sprite icon;
    }

    #region Singleton
    private static ControlPanel instance;
    public static ControlPanel Instance
    {
        get => instance == null ? instance = Resources.Load("ControlPanel") as ControlPanel : instance;
    }
    #endregion
}