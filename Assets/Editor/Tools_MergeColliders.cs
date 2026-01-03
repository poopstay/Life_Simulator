// Assets/Editor/Tools_MergeColliders.cs
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public static class Tools_MergeColliders
{
    [MenuItem("Tools/Colliders/Merge Child Meshes To ONE MeshCollider")]
    private static void MergeToSingleMeshCollider()
    {
        var root = Selection.activeGameObject;
        if (root == null)
        {
            Debug.LogWarning("Hãy chọn GameObject cha cần gộp collider.");
            return;
        }

        // 1) Thu thập tất cả MeshFilter con (bao gồm cả root)
        var meshFilters = root.GetComponentsInChildren<MeshFilter>(true);
        var combine = new List<CombineInstance>(meshFilters.Length);

        foreach (var mf in meshFilters)
        {
            if (mf.sharedMesh == null) continue;

            // Bỏ qua các mesh bị tắt renderer nếu bạn muốn (tuỳ chọn)
            // var r = mf.GetComponent<Renderer>();
            // if (r != null && !r.enabled) continue;

            var ci = new CombineInstance
            {
                mesh = mf.sharedMesh,
                // Đưa từ local của child -> local của root
                transform = root.transform.worldToLocalMatrix * mf.transform.localToWorldMatrix
            };
            combine.Add(ci);
        }

        if (combine.Count == 0)
        {
            Debug.LogWarning("Không tìm thấy MeshFilter nào để gộp.");
            return;
        }

        Undo.RegisterFullObjectHierarchyUndo(root, "Merge Colliders");

        // 2) Xoá toàn bộ collider cũ trong hierarchy (trừ root nếu muốn)
        var oldColliders = root.GetComponentsInChildren<Collider>(true);
        foreach (var c in oldColliders)
        {
            Undo.DestroyObjectImmediate(c);
        }

        // 3) Tạo mesh gộp và gắn MeshCollider vào root
        var merged = new Mesh();
        merged.name = $"{root.name}_MergedColliderMesh";
        merged.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32; // an toàn cho nhiều vertex

        merged.CombineMeshes(combine.ToArray(), true, true, false);
        merged.RecalculateBounds();

        var mc = Undo.AddComponent<MeshCollider>(root);
        mc.sharedMesh = merged;
        mc.convex = false; // Nếu cần convex (dùng cho Rigidbody/Trigger), bật true nhưng mesh phải "đơn giản"

        // (Tuỳ chọn) Nếu bạn cần trigger:
        // mc.isTrigger = true;

        Debug.Log($"✅ Done: Đã gộp {combine.Count} mesh thành 1 MeshCollider trên '{root.name}'.");
    }
}
