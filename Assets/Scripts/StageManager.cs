
using System.Collections;
using UnityEngine;

public class StageManager : MonoBehaviour
{
    public GameObject player;
    public Camera currCamera;

    public GameObject door;

    public Transform startPosition;
    public Transform spawnPosition;

    public Quest q;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player.transform.position = new Vector3(spawnPosition.position.x, player.transform.position.y, spawnPosition.position.z);

        player.GetComponent<CapsuleCollider>().enabled = false;
        player.GetComponent<PlayerMovement>().disableMovement = true;
        currCamera.GetComponent<CameraFollow>().enabled = false;
        StartCoroutine(startAnimation());
    }

    private IEnumerator startAnimation()
    {
        yield return new WaitForSeconds(0.8f);
        door.GetComponent<DoorScript>().ToggleDoor(true);
        yield return new WaitForSeconds(0.7f);

        player.GetComponent<PlayerMovement>().moveInput = new Vector2(0, -1);
        yield return new WaitForSeconds(0.42f);
        currCamera.GetComponent<CameraFollow>().enabled = true;
        yield return new WaitForSeconds(1f);
        player.GetComponent<PlayerMovement>().moveInput = new Vector2(0, 0);
        yield return new WaitForSeconds(0.5f);

        player.transform.position = new Vector3(startPosition.position.x, player.transform.position.y, startPosition.position.z);


        player.GetComponent<PlayerMovement>().disableMovement = false;
        player.GetComponent<CapsuleCollider>().enabled = true;

        TimeManager.Instance.StartTimer();
        yield return new WaitForSeconds(0.5f);


        QuestManager.Instance.StartQuest(q);

    }
}
