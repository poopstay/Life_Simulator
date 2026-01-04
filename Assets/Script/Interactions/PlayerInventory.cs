using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public bool HasMainDoorKey { get; private set; }

    public void GiveMainDoorKey()
    {
        HasMainDoorKey = true;
    }
}
