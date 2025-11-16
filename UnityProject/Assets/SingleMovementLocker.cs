using UnityEngine;

public class SingleMovementLocker : MonoBehaviour
{
    public bool isMoving;

    public void StartMoving()
    {
        isMoving = true;
    }
    public void StopMoving()
    {
        isMoving = false;
    }

    public bool AreWeMoving()
    {
        return isMoving;
    }
}
