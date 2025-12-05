using UnityEngine;

public class CreateArrowMarker : MonoBehaviour
{
    [ContextMenu("Create Arrow Prefab")]
    void CreateArrow()
    {
        // Create parent
        GameObject arrow = new GameObject("TaskMarker_Arrow");

        // Create cone (arrow head)
        GameObject cone = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        cone.name = "ArrowHead";
        cone.transform.SetParent(arrow.transform);
        cone.transform.localPosition = new Vector3(0, -0.3f, 0);
        cone.transform.localScale = new Vector3(0.5f, 0.3f, 0.5f);
        cone.transform.localRotation = Quaternion.Euler(180, 0, 0);

        // Remove collider
        DestroyImmediate(cone.GetComponent<Collider>());

        // Create stem
        GameObject stem = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        stem.name = "ArrowStem";
        stem.transform.SetParent(arrow.transform);
        stem.transform.localPosition = new Vector3(0, 0.2f, 0);
        stem.transform.localScale = new Vector3(0.15f, 0.3f, 0.15f);

        // Remove collider
        DestroyImmediate(stem.GetComponent<Collider>());

        // Add animation
        arrow.AddComponent<TaskMarkerAnimation>();

        Debug.Log("Arrow marker created! Save as prefab.");
    }
}