using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Turning;

namespace UnityEngine.XR.Interaction.Toolkit.Locomotion.Turning
{
    [AddComponentMenu("XR/Locomotion/Gesture Continuous Turn Provider")]
    public class GestureContinuousTurnProvider : ContinuousTurnProvider
    {
        public SingleMovementLocker lockerRef;

        [Header("Gesture-based turn state")]
        [SerializeField]
        [Tooltip("If true, this provider ignores thumbstick input and uses gesture flags instead.")]
        bool m_UseGestures = true;

        bool m_IsTurningLeft;
        bool m_IsTurningRight;

        #region Gesture Callbacks

        public void StartTurnLeft()
        {
            // If we are moving, ignore turn start.
            if (lockerRef != null && lockerRef.AreWeMoving())
                return;

            m_IsTurningLeft = true;
            m_IsTurningRight = false;
        }

        public void StopTurnLeft()
        {
            m_IsTurningLeft = false;
        }

        public void StartTurnRight()
        {
            // If we are moving, ignore turn start.
            if (lockerRef != null && lockerRef.AreWeMoving())
                return;

            m_IsTurningRight = true;
            m_IsTurningLeft = false;
        }

        public void StopTurnRight()
        {
            m_IsTurningRight = false;
        }

        public enum GestureTurnDirection
        {
            None,
            Left,
            Right
        }

        public void SetGestureTurnDirection(GestureTurnDirection direction)
        {
            // Honor the movement lock here too.
            if (lockerRef != null && lockerRef.AreWeMoving())
            {
                m_IsTurningLeft = false;
                m_IsTurningRight = false;
                return;
            }

            m_IsTurningLeft = direction == GestureTurnDirection.Left;
            m_IsTurningRight = direction == GestureTurnDirection.Right;
        }

        #endregion

        /// <inheritdoc />
        protected override float GetTurnAmount(Vector2 input)
        {
            if (!m_UseGestures)
                return base.GetTurnAmount(input);

            // HARD LOCK: if we are moving, DO NOT TURN.
            if (lockerRef != null && lockerRef.AreWeMoving())
            {
                m_IsTurningLeft = false;
                m_IsTurningRight = false;
                return 0f;
            }

            float direction = 0f;

            if (m_IsTurningLeft && !m_IsTurningRight)
                direction = -1f; // left
            else if (m_IsTurningRight && !m_IsTurningLeft)
                direction = 1f;  // right
            else
                direction = 0f;

            if (Mathf.Approximately(direction, 0f))
                return 0f;

            return direction * turnSpeed * Time.deltaTime;
        }
    }
}
