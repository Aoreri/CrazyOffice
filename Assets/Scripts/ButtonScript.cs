using UnityEngine;

public class ButtonScript : MonoBehaviour
{


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    GameObject player;
    public ParticleSystem particleSystemObj;
    public GameObject buttonObject;
    public GameObject mazeCanvas;

    void Start()
    {
        player = GameObject.FindWithTag("Player");
    }

    // Update is called once per frame
    void Update()
    {
        if((player.transform.position - gameObject.transform.position).magnitude <= 2.75)
        {
            buttonObject.GetComponent<Renderer>().material.color = Color.red;

            if (Input.GetKeyUp(KeyCode.E))
            {
                PuzzleManager.StartPuzzle("Maze");
            }

            if (Input.GetKeyUp(KeyCode.T))
            {
                PuzzleManager.StartPuzzle("Modem");
            }

            if (Input.GetKeyUp(KeyCode.Y))
            {
                PuzzleManager.StartPuzzle("Card");
            }

            if (Input.GetKeyUp(KeyCode.R))
                particleSystemObj.Play();
        } else
        {
            buttonObject.GetComponent<Renderer>().material.color = Color.black;

        }
    }
}
