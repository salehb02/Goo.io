using UnityEngine;

public class CapturableSpawnPoint : MonoBehaviour
{
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.position, 0.5f);
    }
}