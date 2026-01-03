using UnityEngine;

public class Interactor : MonoBehaviour
{
    [Header("Raycast")]
    public Camera cam;
    public float distance = 2.2f;
    public LayerMask interactMask = ~0; // bạn có thể set layer riêng sau

    [Header("Input")]
    public KeyCode interactKey = KeyCode.E;

    IInteractable current;
    GameObject currentGO;

    void Awake()
    {
        if (!cam) cam = GetComponentInChildren<Camera>();
        if (!cam) cam = Camera.main;
    }

    void Update()
    {
        Ray ray = new Ray(cam.transform.position, cam.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, distance, interactMask))
        {
            var go = hit.collider.gameObject;
            var interactable = go.GetComponentInParent<IInteractable>();

            if (interactable != null)
            {
                if (go != currentGO)
                {
                    ClearFocus();
                    current = interactable;
                    currentGO = go;
                    current.OnFocus();
                }

                if (Input.GetKeyDown(interactKey))
                {
                    current.Interact(this);
                }

                return;
            }
        }

        ClearFocus();
    }

    void ClearFocus()
    {
        if (current != null) current.OnUnfocus();
        current = null;
        currentGO = null;
    }
}
