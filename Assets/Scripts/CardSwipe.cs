using System.Collections;
using TMPro;
using UnityEngine;


public class CardSwipe : Puzzle
{
    
    public CardSwipeHandler cardSwipe;
   
    public float allowedSwipeTimeMin = 0.8f;
    public float allowedSwipeTimeMax = 1.2f;

    public float textChangeTime = 1.25f;

    //Least allowed swap
    public RectTransform cardSwipeLast;


    public TMPro.TextMeshProUGUI deviceText;

    public GameObject greenLightOn, greenLightOff, redLightOn, redLightOff;


    private Coroutine replaceCoroutine;
    private Coroutine lightCoroutine;

  void Start()
    {
        cardSwipe.OnSwipeRight += HandleRightSwipe;
    }

    void HandleRightSwipe(float swipeTime)
    {
        //Debug.Log("RIGHT swipe time: " + swipeTime);
        //Debug.Log("diff: " + (cardSwipe.transform.position.x - cardSwipeLast.transform.position.x));

        float swipeCordDiff = (cardSwipe.transform.position.x - cardSwipeLast.transform.position.x);

        if (swipeCordDiff < 0)
        {
            SwitchLight(false);
            ReplaceText("INVALID READING");
            return;
        }

        if(swipeTime < allowedSwipeTimeMin)
        {
            SwitchLight(false);
            ReplaceText("TOO FAST");
            return;
        }

        if (swipeTime >= allowedSwipeTimeMax)
        {
            SwitchLight(false);
            ReplaceText("TOO SLOW");
            return;
        }

        SwitchLight(true);
        ReplaceText("LETS GO");
            return;
        

        // TODO: load next card
    }

    void SwitchLight(bool passed)
    {
        if (lightCoroutine != null)
            StopCoroutine(lightCoroutine);

        lightCoroutine = StartCoroutine(SwitchLightEnumerator(passed));
    }

    void ReplaceText(string text)
    {
        if (replaceCoroutine != null)
            StopCoroutine(replaceCoroutine);

        replaceCoroutine = StartCoroutine(ReplaceTextEnumerator(text));
    }


    
    IEnumerator ReplaceTextEnumerator(string text)
    {
        deviceText.text = text;
        yield return new WaitForSeconds(textChangeTime);
        deviceText.text = "SWIPE CARD";
    }

    
    IEnumerator SwitchLightEnumerator(bool passed)
    {
        if(passed)
        {
            greenLightOn.SetActive(true);
            greenLightOff.SetActive(false);
        } else
        {
            redLightOn.SetActive(true);
            redLightOff.SetActive(false);
        }


        yield return new WaitForSeconds(textChangeTime);

        if (passed)
        {
            EndPuzzle();
        }
        else
        {
            redLightOn.SetActive(false);
            redLightOff.SetActive(true);
        }
    }

    protected override void OnStartPuzzle()
    {
        

    }

    protected override void OnEndPuzzle()
    {
        
    }
}
