using System.Diagnostics;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    #region Singleton
    public static AudioManager Instance;

    private void Awake()
    {
        transform.SetParent(null);

        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    #endregion

    private bool logCallers = false;

    private bool _isInitialized = false;

    private AudioSource _sfxADS;

    private void OnEnable()
    {
        if (!_isInitialized)
            InitializeAudioSources();
    }

    private void InitializeAudioSources()
    {
        _sfxADS = new GameObject("SFX").AddComponent<AudioSource>();
        _sfxADS.transform.SetParent(transform);
        _sfxADS.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
        _sfxADS.loop = false;
        _sfxADS.playOnAwake = false;
        _sfxADS.spatialBlend = 0;
        _sfxADS.Stop();

        _isInitialized = true;
    }

    public void StarPickupSFX() => CustomPlayOneShot(ControlPanel.Instance.AudioBank.starPickup);
    private void CustomPlayOneShot(AudioClip audioClip,float volume= 1)
    {
        if (!audioClip)
            return;

        if (logCallers)
            UnityEngine.Debug.Log(new StackFrame(1).GetMethod().Name);

        _sfxADS?.PlayOneShot(audioClip, volume);
    }
}