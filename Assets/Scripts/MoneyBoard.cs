using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MoneyBoard : MonoBehaviour
{
    public static TextMeshProUGUI scoreText;

    public static int money = 0;

    public static void UpdateMoney(int _money)
    {
        money += _money;
        scoreText.text = "Money: " + money.ToString();
        Debug.Log(money);
    }

    // Start is called before the first frame Update
    void Start()
    {
        scoreText = gameObject.GetComponent<TextMeshProUGUI>();
        scoreText.text = "Money: " + money.ToString();
    }

}
