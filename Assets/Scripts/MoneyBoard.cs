using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public sealed class MoneyBoard : MonoBehaviour
{
    public static MoneyBoard Instance { get; private set; }

    private void Awake() 
    {
        moneyText = gameObject.GetComponent<TextMeshProUGUI>();
        moneyText.text = "Money: " + money.ToString();
        Instance = this;
    }

    private int money = 0;

    public int Money 
    { 
        get => money; 
        set 
        {
            money = value;
            moneyText.text = "Money: " + money.ToString();
        } 
    }

    [SerializeField] private TextMeshProUGUI moneyText;

}
