using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class C : MonoBehaviour
{
    public Button button;

    public int priceC;

    public TextMeshProUGUI priceCText;

    private void onClickC()
    {
        // check the money
        if (MoneyBoard.Instance.Money < priceC)
        {
            // TODO: 不足的音效?
            Debug.Log("Not enough money!");
            return;
        }
        if (Board.Instance.specialTileCounters[(int)SpecialTile.CannotTouch].IsEmpty())
        {
            Debug.Log("No need to do C");
            return;
        }

        MoneyBoard.Instance.Money -= priceC;
        // TODO: 扣钱的音效?

        Tile tile = Board.Instance.specialTileCounters[(int)SpecialTile.CannotTouch].Front;
        tile.Type = Board.Instance.tileTypes[Random.Range(0, Board.Instance.normalTileTypesLength)];
        tile.button.onClick.AddListener(() => Board.Instance.Select(tile));
        // TODO: 单个方块重新生成的音效?

        Board.Instance.specialTileCounters.Remove((int)SpecialTile.CannotTouch);
    }

    private void Start()
    {
        button.onClick.AddListener(onClickC);
        priceCText.text = "$" + priceC.ToString();
    }

}
