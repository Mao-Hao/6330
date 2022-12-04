using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Risk : MonoBehaviour
{
    public static Risk Instance { get; private set; }
    public int RiskValue = 0;

    private void Awake()
    {
        Instance = this;
    }

}
