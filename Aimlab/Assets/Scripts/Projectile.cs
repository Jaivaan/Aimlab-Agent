using UnityEngine;
using Unity.MLAgents;

public class Projectile : MonoBehaviour
{
    public float lifetime = 3f;
    [HideInInspector] public Agent shooter;

    //private bool hitTarget = false;

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Target"))
        {
            //hitTarget = true;
            if (shooter != null)
            {
                shooter.AddReward(5.0f);
                shooter.EndEpisode();
            }
            Destroy(collision.gameObject);
        }
       /* 
        else
        {
            if (shooter != null)
            {
                shooter.AddReward(-0.1f);
            }
       
        }*/
        Destroy(gameObject);
    }
/*
    private void OnDestroy()
    {
        if (!hitTarget && shooter != null)
        {
            shooter.AddReward(-0.05f);
        }
    }*/
}
