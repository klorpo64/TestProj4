using UnityEngine;

public class CarColorRandomizer : MonoBehaviour
{
    [Tooltip("List of possible materials for the car body.")]
    public Material[] bodyMaterials;

    void Start()
    {
        MeshRenderer bodyRenderer = FindCarBodyRenderer(transform);

        if (bodyRenderer != null && bodyMaterials.Length > 0)
        {
            bodyRenderer.material = bodyMaterials[Random.Range(0, bodyMaterials.Length)];
        }
        else
        {
            Debug.LogWarning("No suitable mesh found for car body color.");
        }
    }

    MeshRenderer FindCarBodyRenderer(Transform parent)
    {
        MeshRenderer[] renderers = parent.GetComponentsInChildren<MeshRenderer>();
        foreach (MeshRenderer r in renderers)
        {
            // Skip meshes with black or dark materials (usually wheels)
            Material mat = r.sharedMaterial;
            if (mat != null && mat.color.grayscale < 0.1f)
                continue;

            // Optional: Skip very small meshes
            if (r.bounds.size.magnitude < 0.5f)
                continue;

            // This is likely the car body
            return r;
        }

        return null; // fallback if nothing found
    }
}
