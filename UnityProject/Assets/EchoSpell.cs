using UnityEngine;
using System.Collections;
using JetBrains.Annotations;
using System;

public class EchoSpell : MonoBehaviour
{

    public GameObject rh;
    public GameObject EchoRef;
    private bool isPolling;

    public AudioSource audioRef;
    public AudioClip audioClip;

    private float currInterval;
    public float baseInterval;
    private float currDist;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        EchoRef.SetActive(false);
        isPolling = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartIndicate()
    {
        Debug.Log("Start Polling");
        if (isPolling == false)
        {
            EchoRef.SetActive(true);
            isPolling = true;
            StartCoroutine(SendRaycast());
            StartCoroutine(BeepInterval());
        }
        

    }

    public void StopIndicate()
    {
        Debug.Log("Stop Polling");
        EchoRef.SetActive(false);
        isPolling = false;
        currDist = -1;
    }

    public IEnumerator SendRaycast()
    {
        if (isPolling)
        {
            yield return new WaitForSeconds(0.1f);
            int mask = LayerMask.GetMask("Interactable");

            if (Physics.Raycast(rh.transform.position, rh.transform.forward, out RaycastHit hit, 20f, mask))
            {
                Debug.Log($"Hit {hit.collider.name} at {hit.distance}m");
                currDist = hit.distance;
                
            } else
            {
                currDist = -1;
            }
            StartCoroutine(SendRaycast());
        }
        yield return null;
        
    }

    public void UpdateBeepInterval()
    {
        currInterval = (currDist * baseInterval) / 5.00f;
        currInterval = Math.Min(baseInterval, currInterval);
    }

    public IEnumerator BeepInterval()
    {
        if (isPolling)
        {
            if (currDist != -1)
            {
                yield return new WaitForSeconds(currInterval);
                audioRef.PlayOneShot(audioClip);
                UpdateBeepInterval();
                StartCoroutine(BeepInterval());
            }
            
        }
    }
}
