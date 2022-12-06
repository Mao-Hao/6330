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

        MoneyBoard.Instance.Money -= priceA;
        // TODO: 扣钱的音效?

        Tile tile = Board.Instance.specialTileCounters[(int)SpecialTile.MoneyReduction].Front;
        tile.Type = Board.Instance.tileTypes[Random.Range(0, Board.Instance.normalTileTypesLength)];
        tile.button.onClick.AddListener(() => Board.Instance.Select(tile));
        // TODO: 单个方块重新生成的音效?

        Board.Instance.specialTileCounters.Remove((int)SpecialTile.MoneyReduction);
    }

    private void Start()
    {
        button.onClick.AddListener(onClickA);
    }

}
