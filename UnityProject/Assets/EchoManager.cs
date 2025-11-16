
using UnityEngine;

public class EchoManager : MonoBehaviour
{
    public GameObject rh;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // ps = rh.GetComponent<ParticleSystem>();
    }

    // Update is called once per frame
    void Update()
    {
        this.transform.position = rh.transform.position + rh.transform.forward * 0.1f;
        this.transform.eulerAngles = rh.transform.eulerAngles;
    }
}
