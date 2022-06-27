using UnityEngine;

public class Star : MonoBehaviour
{
    public float rotationSpeed = 1;
    public float sinusMovementSpeed = 1;
    public float sinusMaxMovement = 1f;

    private Vector3 _initPositon;

    private void Start()
    {
        _initPositon = transform.position;
    }

    private void Update()
    {
        transform.Rotate(Vector3.up * rotationSpeed);
        transform.position = _initPositon + (Vector3.up * Mathf.Sin(sinusMovementSpeed * Time.time) * sinusMaxMovement);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Venom") || other.CompareTag("Character"))
        {
            var player = other.GetComponentInChildren<PlayerData>();

            if (player)
            {
                player.AddScore();
                AudioManager.Instance.StarPickupSFX();
                Destroy(gameObject);
            }
        }
    }
}