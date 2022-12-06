using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class B : MonoBehaviour
{
    public Button button;

    public int priceB;

    public TextMeshProUGUI priceBText;

    private void onClickB()
    {
        // check the money
        if (MoneyBoard.Instance.Money < priceB)
        {
            Debug.Log("Not enough money!");
            return;
        }
        if (Board.Instance.specialTileCounters[(int)SpecialTile.Bomb].IsEmpty())
        {
            Debug.Log("No need to do B");
            return;
        }

        MoneyBoard.Instance.Money -= priceB;

        Tile tile = Board.Instance.specialTileCounters[(int)SpecialTile.Bomb].Front;

        if (tile.Type.canBeSelected == false)   // 如果没爆炸, 还能抢救
        {
            tile.Type = Board.Instance.tileTypes[Random.Range(0, Board.Instance.normalTileTypesLength)];
            tile.Type.canBeSelected = true;
            tile.button.onClick.AddListener(() => Board.Instance.Select(tile));
            Board.Instance.specialTileCounters.Remove((int)SpecialTile.Bomb);
        }

    }

    // Start is called before the first frame update
    void Start()
    {
        button.onClick.AddListener(onClickB);
        priceBText.text = "$" + priceB.ToString();
    }
}
