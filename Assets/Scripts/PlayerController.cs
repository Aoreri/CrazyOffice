using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; private set; }

    void Awake()
    {
        Instance = this;
    }
}