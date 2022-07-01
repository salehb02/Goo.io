using UnityEngine;

public class DestroyOnBecomeInvisible : MonoBehaviour
{
    public GameObject parent;

    private void OnBecameInvisible()
    {
        if (!parent)
            return;

        Destroy(parent.gameObject);
    }
}