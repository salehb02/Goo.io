using UnityEngine;

public class Ragdoll : MonoBehaviour
{
    public SkinnedMeshRenderer skinnedMesh;

    public void SetColor(Color color)
    {
        if (skinnedMesh == null)
            return;

        var ragdollMats = skinnedMesh.sharedMaterials;
        foreach (var mat in ragdollMats)
            mat.SetColor("_BaseColor", color);
    }
}