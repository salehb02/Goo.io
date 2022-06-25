using UnityEngine;

public class CharacterDetector : MonoBehaviour
{
    private PlayerData _playerData;

    private void Start()
    {
        _playerData = GetComponentInParent<PlayerData>();
    }

    private void OnTriggerEnter(Collider other)
    {
        var controller = other.GetComponent<CustomCharacterController>();

        if (controller)
        {
            _playerData.GetIntoCharacter(controller);
        }
    }
}