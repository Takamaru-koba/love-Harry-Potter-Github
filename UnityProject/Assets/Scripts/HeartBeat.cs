using UnityEngine;

public class Heartbeat : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public AudioSource heartbeatAudio;

    [Header("Distance Settings")]
    public float maxDetectDistance = 25f;
    public float minDetectDistance = 2f;

    [Header("Pulse Settings")]
    public float maxPulseInterval = 1.5f;  // slow pulse (far)
    public float minPulseInterval = 0.25f; // fast pulse (close)

    private float nextPulseTime = 0f;

    void Update()
    {
        if (player == null || heartbeatAudio == null) return;

        float dist = Vector3.Distance(transform.position, player.position);

        // Only pulse if player is within detection radius
        if (dist <= maxDetectDistance)
        {
            float t = Mathf.InverseLerp(maxDetectDistance, minDetectDistance, dist);
            float currentInterval = Mathf.Lerp(maxPulseInterval, minPulseInterval, t);

            // Optional: volume feedback
            heartbeatAudio.volume = Mathf.Lerp(0.1f, 1f, t);

            // Play pulse based on timing
            if (Time.time >= nextPulseTime)
            {
                heartbeatAudio.PlayOneShot(heartbeatAudio.clip);
                nextPulseTime = Time.time + currentInterval;
            }
        }
    }
}
