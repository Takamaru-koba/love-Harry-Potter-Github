using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Turning;

namespace UnityEngine.XR.Interaction.Toolkit.Locomotion.Turning
{
    [AddComponentMenu("XR/Locomotion/Gesture Snap-Like Turn Provider")]
    public class GestureSnapLikeTurnProvider : ContinuousTurnProvider
    {
        public SingleMovementLocker lockerRef;

        [Header("Gesture-based turn state")]
        [SerializeField]
        [Tooltip("If true, this provider ignores thumbstick input and uses gesture methods instead.")]
        bool m_UseGestures = true;

        [Header("Snap-like turn settings")]
        [Tooltip("How many degrees to rotate per gesture.")]
        public float snapAngle = 45f;

        // How much rotation is left to apply this gesture (signed, in degrees)
        float m_RemainingAngle = 0f;

        // Prevents repeated snaps while the same gesture is held
        bool m_TurnLocked = false;

        #region Gesture Callbacks (called from your external script)

        public void StartTurnLeft()
        {
            if (!m_UseGestures)
                return;

            // If we are moving, ignore turn start.
            if (lockerRef != null && lockerRef.AreWeMoving())
                return;

            // Already in the middle of a turn or locked by current gesture.
            if (m_TurnLocked || !Mathf.Approximately(m_RemainingAngle, 0f))
                return;

            // Queue a left turn.
            m_RemainingAngle = -Mathf.Abs(snapAngle);
            m_TurnLocked = true;
        }

        public void StopTurnLeft()
        {
            if (!m_UseGestures)
                return;

            // Allow another snap on next gesture.
            m_TurnLocked = false;
        }

        public void StartTurnRight()
        {
            if (!m_UseGestures)
                return;

            if (lockerRef != null && lockerRef.AreWeMoving())
                return;

            if (m_TurnLocked || !Mathf.Approximately(m_RemainingAngle, 0f))
                return;

            // Queue a right turn.
            m_RemainingAngle = Mathf.Abs(snapAngle);
            m_TurnLocked = true;
        }

        public void StopTurnRight()
        {
            if (!m_UseGestures)
                return;

            m_TurnLocked = false;
        }

        #endregion

        /// <inheritdoc />
        protected override float GetTurnAmount(Vector2 input)
        {
            // If we aren't using gestures, fall back to normal thumbstick behavior.
            if (!m_UseGestures)
                return base.GetTurnAmount(input);

            // HARD LOCK: if we are moving, DO NOT TURN and cancel any queued turn.
            if (lockerRef != null && lockerRef.AreWeMoving())
            {
                m_RemainingAngle = 0f; // cancel in-progress snap
                return 0f;
            }

            // No turn queued.
            if (Mathf.Approximately(m_RemainingAngle, 0f))
                return 0f;

            // We want to apply the snapAngle over as few frames as possible.
            // turnSpeed is in degrees/sec, so per frame we can apply:
            float maxStep = turnSpeed * Time.deltaTime;

            // Clamp the step to whatever remains so we land exactly on snapAngle.
            float step = Mathf.Clamp(m_RemainingAngle, -maxStep, maxStep);

            // Reduce the remaining angle by this frame's step.
            m_RemainingAngle -= step;

            // This is the amount ContinuousTurnProvider will actually rotate this frame.
            return step;
        }
    }
}
