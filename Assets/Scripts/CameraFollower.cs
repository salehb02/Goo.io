using UnityEngine;

public class CameraFollower : MonoBehaviour
{
    public Transform _target;
    public Vector3 offset;
    public float smoothness = 5f;
    public float lookAtSmoothness = 10f;

    private void LateUpdate()
    {
        if (!_target)
            return;

        transform.position = Vector3.Lerp(transform.position, _target.position + offset, smoothness * Time.deltaTime);

        var targetDirection = _target.transform.position - transform.position;
        transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(targetDirection), Time.deltaTime * lookAtSmoothness);
    }

    public void SetTarget(Transform target)
    {
        _target = target;
    }
}