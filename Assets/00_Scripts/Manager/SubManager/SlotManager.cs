using System.Collections.Generic;
using UnityEngine;

public class SlotManager : MonoBehaviour
{
    [SerializeField] List<Slot> slots;

    private void Start()
    {
        slots[0].RollStart(0, 4);
        slots[1].RollStart(1, 5);
        slots[2].RollStart(2, 6);
    }
}
