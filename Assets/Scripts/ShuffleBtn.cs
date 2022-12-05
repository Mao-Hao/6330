using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class ShuffleBtn : MonoBehaviour
{
    // Button
    public Button shuffleBtn;

    // shuffle times
    private int shuffleTimes = 0;

    private int shufflePrice = 500;

    [SerializeField] private TextMeshProUGUI shufflePriceText;

    void Start()
    {
        shuffleBtn.onClick.AddListener(() => Shuffle());

        shufflePriceText.text = "$" + (shuffleTimes * shufflePrice).ToString();
    }

    void Shuffle()
    {
        // check the money
        if (MoneyBoard.Instance.Money >= shufflePrice * shuffleTimes)
        {
            // if enough money, shuffle the board
            MoneyBoard.Instance.Money -= shufflePrice * shuffleTimes;
            Board.Instance.Shuffle();
            shuffleTimes++;
            shufflePriceText.text = "$" + (shufflePrice * shuffleTimes).ToString();
        }
        else
        {
            // if not enough money, show the message
            Debug.Log("Not enough money, still need $" + (shufflePrice * shuffleTimes - MoneyBoard.Instance.Money).ToString());
        }
    }
}
