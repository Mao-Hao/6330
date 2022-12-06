using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class A : MonoBehaviour
{
    public Button button;

    public int priceA;

    private void onClickA()
    {
        // check the money
        if (MoneyBoard.Instance.Money < priceA)
        {
            // TODO: 不足的音效?
            Debug.Log("Not enough money!");
            return;
        }
        if (Board.Instance.specialTileCounters[(int)SpecialTile.MoneyReduction].IsEmpty())
        {
            Debug.Log("No need to do A");
            return;
        }

        Debug.Log("Hello World!");
    }

    private void Start()
    {
        button.onClick.AddListener(onClickA);
    }

}
