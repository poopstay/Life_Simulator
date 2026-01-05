using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    // === Existing (giữ để không vỡ code cũ) ===
    public bool HasMainDoorKey { get; private set; }

    public void GiveMainDoorKey()
    {
        HasMainDoorKey = true;
    }

    // === New: Vehicle key ===
    public bool HasVehicleKey { get; private set; }

    public void GiveVehicleKey()
    {
        HasVehicleKey = true;
    }
}
