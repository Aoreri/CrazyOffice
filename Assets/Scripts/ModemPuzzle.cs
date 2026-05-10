using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ModemPuzzle : Puzzle
{
    public Image lights;

    [Header("Antennas")]
    public AntennaRotate anten1;
    public AntennaRotate anten2;

    [Header("UI Transition")]
    public Image targetImage;
    public float fadeDuration = 2.0f;

    public float acceptableRange = 0.1f;

    public float designatedValue1, designatedValue2;

    void Start()
    {
        anten1.antennaDelta = UnityEngine.Random.value;
        anten2.antennaDelta = UnityEngine.Random.value;

        anten1.Setup();
        anten2.Setup();

        designatedValue1 = UnityEngine.Random.value;
        designatedValue2 = UnityEngine.Random.value;
    }

    void Update()
    {
        float diff1 = Mathf.Abs(anten1.antennaDelta - designatedValue1);
        float diff2 = Mathf.Abs(anten2.antennaDelta - designatedValue2);


        float signal1 = Mathf.InverseLerp(acceptableRange, 0, diff1);
        float signal2 = Mathf.InverseLerp(acceptableRange, 0, diff2);

        float solveDelta = (signal1 + signal2) / 2f;

        //float diff1 = Mathf.Abs(anten1.antennaDelta - designatedValue1);
        //float diff2 = Mathf.Abs(anten2.antennaDelta - designatedValue2);

        //float solveDelta = Mathf.Clamp01(1f - (diff1 + diff2) / 2f);

        lights.fillAmount = solveDelta;

        if (solveDelta >= 0.98f)
        {
            anten1.enabled = false;
            anten2.enabled = false;
            StartCoroutine(FadeImageOut());
        }

    }

    IEnumerator FadeImageOut()
    {

        float startAlpha = targetImage.color.a;
        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float newAlpha = Mathf.Lerp(startAlpha, 0f, elapsedTime / fadeDuration);

            Color c = targetImage.color;
            c.a = newAlpha;
            targetImage.color = c;

            yield return null;
        }

        Color finalColor = targetImage.color;
        finalColor.a = 0f;
        targetImage.color = finalColor;

        yield return new WaitForSeconds(2);
        EndPuzzle();
       
    }

  

    protected override void OnEndPuzzle()
    {
        
    }

    protected override void OnStartPuzzle()
    {
  
    }
}