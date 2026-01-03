using UnityEngine;

public class OutlineHighlighter : MonoBehaviour
{
    [SerializeField] private Renderer targetRenderer;
    [SerializeField] private Material outlineMaterial;
    [SerializeField, Range(1.001f, 1.08f)] private float scale = 1.3f;

    private GameObject outlineObj;

    private void Awake()
    {
        if (!targetRenderer) targetRenderer = GetComponentInChildren<Renderer>();
        Build();
        SetHighlighted(false);
    }

    private void Build()
    {
        if (!targetRenderer || !outlineMaterial) return;

        outlineObj = new GameObject($"{targetRenderer.name}_Outline");
        outlineObj.transform.SetParent(targetRenderer.transform, false);
        outlineObj.transform.localScale = Vector3.one * scale;

        var mf = targetRenderer.GetComponent<MeshFilter>();
        var mr = targetRenderer.GetComponent<MeshRenderer>();
        var smr = targetRenderer as SkinnedMeshRenderer;

        if (mf && mr)
        {
            var outMF = outlineObj.AddComponent<MeshFilter>();
            outMF.sharedMesh = mf.sharedMesh;

            var outMR = outlineObj.AddComponent<MeshRenderer>();
            outMR.sharedMaterial = outlineMaterial;
            outMR.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            outMR.receiveShadows = false;
        }
        else if (smr)
        {
            var outSMR = outlineObj.AddComponent<SkinnedMeshRenderer>();
            outSMR.sharedMesh = smr.sharedMesh;
            outSMR.rootBone = smr.rootBone;
            outSMR.bones = smr.bones;
            outSMR.sharedMaterial = outlineMaterial;
            outSMR.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            outSMR.receiveShadows = false;
        }
        else
        {
            Destroy(outlineObj);
            outlineObj = null;
        }
    }

    public void SetHighlighted(bool on)
    {
        if (outlineObj) outlineObj.SetActive(on);
		
		Debug.Log($"[Outline] {name} -> {(on ? "ON" : "OFF")}  outlineObj={(outlineObj ? outlineObj.name : "NULL")}");
		if (outlineObj) outlineObj.SetActive(on);
    }
}
