using UnityEngine;

public class CameraFollower : MonoBehaviour
{
    public Transform _target;
    public Vector3 offset;
    public float smoothness = 5f;

    private void Update()
    {
        if (!_target)
            return;

        transform.position = Vector3.Lerp(transform.position, _target.position + offset, smoothness * Time.deltaTime);
        transform.LookAt(_target.transform);
    }

    public void SetTarget(Transform target)
    {
        _target = target;
    }
}