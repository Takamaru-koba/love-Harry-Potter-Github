using UnityEngine;
using System.Collections;

public class FireballManager : MonoBehaviour
{
    
    public GameObject rh;

    public Gradient[] chargeGradients = new Gradient[4]; //Empty, 1, 2, 3
    public float[] chargeRadii = new float[4];

    public ParticleSystem ps;




    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // ps = rh.GetComponent<ParticleSystem>();
    }

    // Update is called once per frame
    void Update()
    {
        this.transform.position = rh.transform.position + new Vector3(0, 0.1f, 0.05f);
    }

    

    public void UpdateChargeLvl(int lvl)
    {
        
        var col = ps.colorOverLifetime;
        col.enabled = true;
        col.color = chargeGradients[lvl];
        var shape = ps.shape;
        shape.radius = chargeRadii[lvl];
    }

}
