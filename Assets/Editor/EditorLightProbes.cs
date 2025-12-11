using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public class SetLightProbes : EditorWindow
{
    [MenuItem("Tools/Lighting/Set All To Light Probes (Scene)")]
    public static void SetAllInScene()
    {
        // Finds all MeshRenderers that are *active or inactive* in the scene
        var renderers = Object.FindObjectsByType<MeshRenderer>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None
        );

        int count = 0;
        foreach (var r in renderers)
        {
            r.lightProbeUsage = LightProbeUsage.BlendProbes;
            r.receiveGI = ReceiveGI.LightProbes;
            r.reflectionProbeUsage = ReflectionProbeUsage.BlendProbes;

            // Mark scene dirty so it saves
            EditorUtility.SetDirty(r);
            count++;
        }

        Debug.Log($"Updated {count} MeshRenderers to Light Probes.");
    }
}