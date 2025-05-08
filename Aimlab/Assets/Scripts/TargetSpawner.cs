using UnityEngine;

public class TargetSpawner : MonoBehaviour
{
    public GameObject targetPrefab;
    public Transform[] spawnPoints; 

    public float spawnDelay = 0.5f;

    private GameObject currentTarget;
    public int consecutiveHits = 0;
    public int unlockedSpawns = 1;
    public static TargetSpawner Instance;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        SpawnTarget();
    }

    void Update()
    {
        if (currentTarget == null && !IsInvoking(nameof(SpawnTarget)))
        {
            Invoke(nameof(SpawnTarget), spawnDelay);
        }
    }

    public void OnTargetHit()
    {
        consecutiveHits++;
        if (consecutiveHits % 10 == 0 && unlockedSpawns < spawnPoints.Length)
        {
            unlockedSpawns++;
        }
    }

    public void OnTargetMiss()
    {
        consecutiveHits = 0;
    }

    void SpawnTarget()
    {

        int idx = Random.Range(0, unlockedSpawns);
        Transform spawn = spawnPoints[idx];

        currentTarget = Instantiate(targetPrefab, spawn.position, Quaternion.identity);
        currentTarget.tag = "Target";
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        if (spawnPoints != null)
        {
            foreach (var p in spawnPoints)
            {
                if (p != null) Gizmos.DrawWireSphere(p.position, 0.3f);
            }
        }
    }
}
