using System.Collections;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class HallwayAgent : Agent
{
    public float rotationSpeed = 150f;

    // Nuevo: Prefab del proyectil a disparar (puede ser una esfera con collider)
    public GameObject projectilePrefab;
    // Velocidad a la que se desplaza el proyectil
    public float projectileSpeed = 50f;

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.forward);
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        // Acciones discretas:
        // 0: No hacer nada, 1: Rotar izquierda, 2: Rotar derecha, 3: Disparar
        int action = actionBuffers.DiscreteActions[0];

        switch (action)
        {
            case 1:
                // Rotar izquierda
                transform.Rotate(Vector3.up, -rotationSpeed * Time.deltaTime);
                break;
            case 2:
                // Rotar derecha
                transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
                break;
            case 3:
                // Disparar: Se instancia un proyectil que llevará la lógica de colisiones y recompensas
                Shoot();
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// Instancia un proyectil (una esfera) en la dirección en la que el agente está mirando.
    /// El proyectil llevará una referencia al agente (shooter) para que, en el script del proyectil,
    /// se asignen las recompensas correspondientes según la colisión.
    /// </summary>
    void Shoot()
    {
        // Posición de spawn: un poco adelante del agente para evitar colisiones inmediatas
        Vector3 spawnPos = transform.position + transform.forward;
        GameObject projectile = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);

        // Se obtiene o añade el Rigidbody para mover el proyectil
        Rigidbody rb = projectile.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = projectile.AddComponent<Rigidbody>();
        }
        rb.velocity = transform.forward * projectileSpeed;

        // Asigna la referencia al agente que dispara (esto es importante para la asignación de recompensas)
        Projectile projScript = projectile.GetComponent<Projectile>();
        if (projScript != null)
        {
            projScript.shooter = this;
        }

        // Se destruye el proyectil tras cierto tiempo (opcional, para evitar saturar la escena)
        Destroy(projectile, 3f);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActionsOut = actionsOut.DiscreteActions;

        // Usamos flechas para rotar y la barra espaciadora para disparar
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            discreteActionsOut[0] = 1; // Rotar izquierda
        }
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            discreteActionsOut[0] = 2; // Rotar derecha
        }
        else if (Input.GetKeyDown(KeyCode.Space))
        {
            discreteActionsOut[0] = 3; // Disparar
        }
        else
        {
            discreteActionsOut[0] = 0; // No hacer nada
        }
    }

    public override void OnEpisodeBegin()
    {
        // Reinicia la orientación del agente al iniciar el episodio
        transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
    }
}
