using UnityEngine;

public class TargetSpawner : MonoBehaviour
{
    public GameObject targetPrefab;

    public Vector3 spawnAreaCenter = Vector3.zero;
    public Vector3 spawnAreaSize = new Vector3(10f, 1f, 10f);
    public float spawnDelay = 0.5f;

    private GameObject currentTarget;

    void Start()
    {
        SpawnTarget();
    }

   void Update()
    {
        if (currentTarget == null && !IsInvoking("SpawnTarget"))
        {
            Invoke("SpawnTarget", spawnDelay);
        }
    }

    void SpawnTarget()
    {
        Vector3 center = transform.position;
        //Vector3 spawnPosition = GetRandomPosition();
        currentTarget = Instantiate(targetPrefab, spawnAreaCenter, Quaternion.identity);
        currentTarget.tag = "Target";
    }

    Vector3 GetRandomPosition()
    {
        float randomX = Random.Range(-spawnAreaSize.x / 2, spawnAreaSize.x / 2);
        float randomZ = Random.Range(-spawnAreaSize.z / 2, spawnAreaSize.z / 2);
        return spawnAreaCenter + new Vector3(randomX, 0f, randomZ);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(spawnAreaCenter, spawnAreaSize);
    }
}
