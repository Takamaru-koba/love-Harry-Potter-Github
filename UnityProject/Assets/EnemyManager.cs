using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public int activeEnemyIdx;
    public GameObject spawn2Ref;
    public GameObject spawn3Ref;
    public GameObject enemyPrefab;

    public GameObject DoorRef;
    public GameObject PlayerRef;
    public GameObject CameraRef;

    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        activeEnemyIdx = 1;
        //Spawn enemy at location of manager
        GameObject a = Instantiate(enemyPrefab, this.transform);
        a.GetComponent<EnemyCamouflageFixedFull_V3>().player = PlayerRef.transform;
        a.GetComponent<EnemyCamouflageFixedFull_V3>().playerCamera = CameraRef.GetComponent<Camera>();
        a.transform.SetParent(null);
    }

    // // Update is called once per frame
    // void Update()
    // {
        
    // }

    public void RegisterEnemyDeath()
    {
        activeEnemyIdx += 1;

        if (activeEnemyIdx == 2)
        {
            //Spawn Enemy at Spawn2
            GameObject a = Instantiate(enemyPrefab, spawn2Ref.transform);
            a.GetComponent<EnemyCamouflageFixedFull_V3>().player = PlayerRef.transform;
            a.GetComponent<EnemyCamouflageFixedFull_V3>().playerCamera = CameraRef.GetComponent<Camera>();
            a.transform.SetParent(null);

        } else if (activeEnemyIdx == 3)
        {
            //Spawn Enemy at Spawn 3
            GameObject a =Instantiate(enemyPrefab, spawn3Ref.transform);
            a.GetComponent<EnemyCamouflageFixedFull_V3>().player = PlayerRef.transform;
            a.GetComponent<EnemyCamouflageFixedFull_V3>().playerCamera = CameraRef.GetComponent<Camera>();
            a.transform.SetParent(null);
        } else
        {
            //Oopen Na Noor
            DoorRef.SetActive(false);
        }
    }
}
