using UnityEngine;

public class Ragdoll : MonoBehaviour
{
    public SkinnedMeshRenderer skinnedMesh;

    public void SetColor(Color color)
    {
        if (skinnedMesh == null)
            return;

        var mat = new Material(skinnedMesh.sharedMaterial);
        mat.SetColor("_BaseColor", color);
        skinnedMesh.material = mat;
    }
}