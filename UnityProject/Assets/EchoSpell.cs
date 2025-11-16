using UnityEngine;
using System.Collections;

public class EchoSpell : MonoBehaviour
{

    public GameObject rh;
    public GameObject EchoRef;
    private bool isPolling;
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
        }
        

    }

    public void StopIndicate()
    {
        Debug.Log("Stop Polling");
        EchoRef.SetActive(false);
        isPolling = false;
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
            }
            StartCoroutine(SendRaycast());
        }
        yield return null;
        
    }
}
