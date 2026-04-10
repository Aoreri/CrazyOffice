using UnityEngine;

public class ButtonScript : MonoBehaviour
{


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    GameObject player;
    public ParticleSystem particleSystemObj;
    public GameObject buttonObject;

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

            if(Input.GetKeyUp(KeyCode.E))
                particleSystemObj.Play();
        } else
        {
            buttonObject.GetComponent<Renderer>().material.color = Color.black;

        }
    }
}
