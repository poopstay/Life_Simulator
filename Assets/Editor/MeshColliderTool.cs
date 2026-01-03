using UnityEngine;
using UnityEditor;

public class MeshColliderTool
{
    [MenuItem("Tools/Colliders/Reset All MeshColliders")]
    static void ResetAllMeshColliders()
    {
        // Lấy toàn bộ GameObject trong scene
        GameObject[] allObjects = Object.FindObjectsOfType<GameObject>();

        int removed = 0;
        int added = 0;

        foreach (GameObject go in allObjects)
        {
            // Bỏ qua object bị disable
            if (!go.activeInHierarchy) continue;

            // Chỉ xử lý object có mesh
            MeshFilter mf = go.GetComponent<MeshFilter>();
            if (mf == null || mf.sharedMesh == null) continue;

            // ❌ Xóa tất cả MeshCollider cũ
            MeshCollider[] oldColliders = go.GetComponents<MeshCollider>();
            foreach (var col in oldColliders)
            {
                Object.DestroyImmediate(col);
                removed++;
            }

            // ✅ Add MeshCollider mới
            MeshCollider newCol = go.AddComponent<MeshCollider>();
            newCol.sharedMesh = mf.sharedMesh;
            newCol.convex = false;

            //added++;
        }

        Debug.Log($"Reset MeshCollider xong ✔  Removed: {removed}, Added: {added}");
    }
}
