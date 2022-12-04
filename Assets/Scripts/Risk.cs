using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Risk : MonoBehaviour
{
    public static Risk Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
        // RiskValues = new int[Board.Instance.tileTypesLength];
    }

    // public int[] RiskValues;
    public float RiskValue = .0f;
}
