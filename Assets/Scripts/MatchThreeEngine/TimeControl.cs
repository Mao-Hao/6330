using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using DG.Tweening;
using TMPro;

public class TimeControl : MonoBehaviour
{
    public GameObject timeText; //????
    // Start is called before the first frame update
    public int totalTime;  //?????
    void Start()
    {
        StartCoroutine(TimeCounter());
    }

    // Update is called once per frame
    IEnumerator TimeCounter()
    {
        while(totalTime>0)
        {
            timeText.GetComponent<TMP_Text>().text = totalTime.ToString();
           
            yield return new WaitForSeconds(1);
            totalTime--;
            if (totalTime<=5)
            {
                timeText.GetComponent<TMP_Text>().color = Color.red;
              //  StartCoroutine(MoveText(totalTime));
                
            }
            if(totalTime==0)
            {
                timeText.GetComponent<TMP_Text>().text = "game over!";
            }
        }
    }

    IEnumerable MoveText(int time)
    {
        while(time>=0)
        {
            time--;
            timeText.transform.DOScale(2, 1).From();
            timeText.GetComponent<TMP_Text>().DOFade(0, 1).From();
            yield return new WaitForSeconds(1);
        }
    }


    void Update()
    {

    }

    
}
