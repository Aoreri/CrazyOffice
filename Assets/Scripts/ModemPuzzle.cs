using UnityEngine;

public class ModemPuzzle : MonoBehaviour
{
    [Header("Antennas")]
    public AntennaRotate anten1;
    public AntennaRotate anten2;


    // Optional: check if both antennas are at a "solved" angle
    public bool IsSolved(float targetY, float tolerance = 5f)
    {
        //bool a1ok = Mathf.Abs(anten1 - targetY) <= tolerance;
        //*bool a2ok = Mathf.Abs(anten2.CurrentRotY - targetY) <= tolerance;
        return false;
        //return a1ok && a2ok;
    }
}