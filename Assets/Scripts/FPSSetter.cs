using UnityEngine;

public class FPSSetter : MonoBehaviour
{
    [SerializeField] private int targetFPS = 60;

    private void Start()
    {
        Application.targetFrameRate = targetFPS;
    }
}