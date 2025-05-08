using UnityEngine;
using Unity.MLAgents;

public class Projectile : MonoBehaviour
{
    public float lifetime = 3f;
    [HideInInspector] public Agent shooter;

    private bool hitTarget = false;

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Target"))
        {
            hitTarget = true;
            if (shooter != null)
            {
                shooter.AddReward(1.0f);
                TargetSpawner.Instance.OnTargetHit();
                shooter.EndEpisode();
            }
            Destroy(collision.gameObject);
        }
        Destroy(gameObject);
    }

    void OnDestroy()
    {
        if (!hitTarget && TargetSpawner.Instance != null)
        {
            TargetSpawner.Instance.OnTargetMiss();
            if (shooter != null)
            {
                shooter.AddReward(-0.01f);
            }
        }
    }
}
