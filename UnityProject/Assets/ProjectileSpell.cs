using JetBrains.Annotations;
using UnityEngine;
using System.Collections;
using System;

public class ProjectileSpell : MonoBehaviour
{
    public GameObject ReloadFireballRef;
    public FireballManager fireballManagerRef;
    private int chargeLvl;
    private bool isReloading;

    public Transform projSpawn;
    public GameObject R_wrist_ref;
    public GameObject projPrefab;

    public float speed = 1000f;
    private bool hasFired;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ReloadFireballRef.SetActive(false);
        chargeLvl = 0;
        isReloading = false;
        fireballManagerRef = ReloadFireballRef.GetComponent<FireballManager>();
        hasFired = false;
    }

    // // Update is called once per frame
    // void Update()
    // {
        
    // }
    public void Point()
    {
        Debug.Log("point registered");
        if (chargeLvl > 0 && hasFired == false)
        {
            hasFired = true;
            projSpawn = R_wrist_ref.transform;
            GameObject proj = Instantiate(projPrefab, projSpawn);
            proj.transform.SetParent(null);

            proj.GetComponent<ProjectileFireball>().Setup(chargeLvl);
            proj.GetComponent<Rigidbody>().linearVelocity = projSpawn.forward * speed;
            chargeLvl -= 1;
            fireballManagerRef.UpdateChargeLvl(chargeLvl);
        }
    }
    public void StopPoint()
    {
        hasFired = false;
    }

    public void Prime()
    {
        Debug.Log("Prime registered");
    }
    
    public void Reload()
    {
        ReloadFireballRef.SetActive(true);
        if (isReloading == false)
        {
            isReloading = true;
            StartCoroutine(ReloadDuration());
        }
        
    }

    public void StopReload()
    {
        ReloadFireballRef.SetActive(false);
        isReloading = false;
    }

    public IEnumerator ReloadDuration()
    {
        if (isReloading)
        {
            yield return new WaitForSeconds(0.8f);
            chargeLvl = Math.Min(chargeLvl+ 1, 3);
            fireballManagerRef.UpdateChargeLvl(chargeLvl);
            StartCoroutine(ReloadDuration());
        }
        yield return null;
        

    }
}
