using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CableMatch : MonoBehaviour
{
    public GameObject[] hubs = new GameObject[4];
    public GameObject[] connects = new GameObject[4];
    public Color[] colors = new Color[4];


    void Start()
    {
        AssignRandomUniqueColors(hubs);
        AssignRandomUniqueColors(connects);


        for (int i = 0; i < hubs.Length; i++)
        {
            hubs[i].GetComponent<WireDrag>().enabled = true;
        }

    }
    void AssignRandomUniqueColors(GameObject[] objects)
    {
        
        List<Color> availableColors = new List<Color>(colors);

        for (int i = 0; i < hubs.Length; i++)
        {
          
            int randomIndex = Random.Range(0, availableColors.Count);
            Color selectedColor = availableColors[randomIndex];

            if (objects[i] != null)
            {
                objects[i].GetComponent<Image>().color = selectedColor;
            }


            availableColors.RemoveAt(randomIndex);
        }
    }
}
