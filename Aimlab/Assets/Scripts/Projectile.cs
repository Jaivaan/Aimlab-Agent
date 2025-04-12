using UnityEngine;
using Unity.MLAgents;  // Para poder usar el tipo Agent

public class Projectile : MonoBehaviour
{
    // Tiempo de vida del proyectil en segundos
    public float lifetime = 3f;
    // Referencia al agente que disparó (se asigna al instanciar el proyectil)
    [HideInInspector] public Agent shooter;

    // Bandera para indicar si el proyectil impactó un target
    private bool hitTarget = false;

    private void Start()
    {
        // Destruir el proyectil tras 'lifetime' segundos si no ha colisionado
        Destroy(gameObject, lifetime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Si impacta un objeto etiquetado como "Target"
        if (collision.gameObject.CompareTag("Target"))
        {
            hitTarget = true;
            if (shooter != null)
            {
                shooter.AddReward(1.0f);
            }
            // Destruye el target al impactarlo (puedes agregar efectos extra si lo deseas)
            Destroy(collision.gameObject);
        }
        else
        {
            // Penaliza si el proyectil colisiona con cualquier otro objeto
            if (shooter != null)
            {
                shooter.AddReward(-0.1f);
            }
        }
        // Destruye el proyectil al colisionar
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        // Si el proyectil se destruye por expiración sin haber impactado nada,
        // se aplica una penalización leve
        if (!hitTarget && shooter != null)
        {
            shooter.AddReward(-0.05f);
        }
    }
}
