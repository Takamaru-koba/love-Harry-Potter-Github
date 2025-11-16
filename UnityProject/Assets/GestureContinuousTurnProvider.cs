using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Movement;

/// <summary>
/// Continuous locomotion driven by hand poses instead of thumbsticks.
/// Call the public methods from your gesture events to start/stop movement.
/// </summary>
[AddComponentMenu("XR/Locomotion/Gesture Continuous Move Provider")]
public class GestureContinuousMoveProvider : ContinuousMoveProvider
{
    public enum HandMoveState
    {
        Still,
        MoveForward,
        MoveBackward
    }

    public SingleMovementLocker lockerRef;

    [Header("Gesture Movement Settings")]
    [Tooltip("Virtual stick magnitude for forward/backward movement (1 = full stick).")]
    [Range(0f, 1f)]
    public float gestureMagnitude = 1f;

    [Tooltip("Current gesture-driven movement state (for debugging).")]
    public HandMoveState currentState = HandMoveState.Still;

    // This acts like the thumbstick input vector that ContinuousMoveProvider expects.
    Vector2 _gestureInput = Vector2.zero;

    #region Gesture Callbacks (hook these from your pose system)

    public void BeginMoveForward()
    {
        currentState = HandMoveState.MoveForward;
        _gestureInput = new Vector2(0f, gestureMagnitude); // stick up

        if (lockerRef != null)
            lockerRef.StartMoving();
    }

    public void BeginMoveBackward()
    {
        currentState = HandMoveState.MoveBackward;
        _gestureInput = new Vector2(0f, -gestureMagnitude); // stick down

        if (lockerRef != null)
            lockerRef.StartMoving();
    }

    public void StopMoving()
    {
        currentState = HandMoveState.Still;
        _gestureInput = Vector2.zero;

        if (lockerRef != null)
            lockerRef.StopMoving();
    }

    #endregion

    protected override Vector3 ComputeDesiredMove(Vector2 ignoredInputFromBase)
    {
        // Ignore controller input; use the gesture stick instead.
        return base.ComputeDesiredMove(_gestureInput);
    }
}
