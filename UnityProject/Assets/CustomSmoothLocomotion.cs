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

    [Header("Gesture Movement Settings")]
    [Tooltip("Virtual stick magnitude for forward/backward movement (1 = full stick).")]
    [Range(0f, 1f)]
    public float gestureMagnitude = 1f;

    [Tooltip("Current gesture-driven movement state (for debugging).")]
    public HandMoveState currentState = HandMoveState.Still;

    // This acts like the thumbstick input vector that ContinuousMoveProvider expects.
    Vector2 _gestureInput = Vector2.zero;

    #region Gesture Callbacks (hook these from your pose system)

    /// <summary>
    /// Call when the 'move forward' pose is detected / held.
    /// </summary>
    public void BeginMoveForward()
    {
        currentState = HandMoveState.MoveForward;
        _gestureInput = new Vector2(0f, gestureMagnitude); // stick up
    }

    /// <summary>
    /// Call when the 'move backward' pose is detected / held.
    /// </summary>
    public void BeginMoveBackward()
    {
        currentState = HandMoveState.MoveBackward;
        _gestureInput = new Vector2(0f, -gestureMagnitude); // stick down
    }

    /// <summary>
    /// Call when you want to stop movement (neutral pose).
    /// </summary>
    public void StopMoving()
    {
        currentState = HandMoveState.Still;
        _gestureInput = Vector2.zero;
    }

    #endregion

    /// <summary>
    /// Override ContinuousMoveProvider's move computation to use our
    /// gesture-driven virtual stick instead of controller input.
    /// </summary>
    protected override Vector3 ComputeDesiredMove(Vector2 ignoredInputFromBase)
    {
        // Completely ignore the thumbstick input that ContinuousMoveProvider would read,
        // and instead use our gesture vector that gets updated by the callbacks above.
        return base.ComputeDesiredMove(_gestureInput);
    }

    // protected override void OnDisable()
    // {
    //     base.OnDisable();
    //     StopMoving(); // safety reset
    // }
}
