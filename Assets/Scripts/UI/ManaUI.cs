using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManaUI : MonoBehaviour
{
    public enum State
    {
        EMPTY, EXPENDED, AVAILABLE
    }
    public GameObject expendedMana;
    public GameObject availableMana;
    public void SetState(State s)
    {
        expendedMana.SetActive(s == State.EXPENDED);
        availableMana.SetActive(s == State.AVAILABLE);
    }
}
