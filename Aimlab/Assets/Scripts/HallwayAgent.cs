using System.Collections;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class HallwayAgent : Agent
{
    public float rotationSpeed = 150f;

    // Prefab del proyectil (una esfera) a disparar.
    public GameObject projectilePrefab;
    // Velocidad del proyectil.
    public float projectileSpeed = 50f;

    // Tiempo mínimo de espera entre disparos (en segundos).
    public float shootCooldown = 0.5f;
    // Último tiempo en que se disparó.
    private float lastShotTime = -Mathf.Infinity;

    // Variable para almacenar el ángulo vertical (pitch) actual, en grados (con signo).
    private float currentPitch;

    [Header("Referencias")]
    public Transform targetTransform;   

    [Header("Observaciones")]
    public float maxObservationDistance = 20f;
    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.forward);

        Vector3 toTarget = targetTransform.position - transform.position;
        sensor.AddObservation(toTarget.normalized);

        sensor.AddObservation(Mathf.Clamp01(toTarget.magnitude / maxObservationDistance));
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        // Se asume que el espacio de acciones discreto tiene 6 opciones:
        // 0: No hacer nada
        // 1: Rotar horizontalmente a la izquierda
        // 2: Rotar horizontalmente a la derecha
        // 3: Rotar verticalmente hacia arriba
        // 4: Rotar verticalmente hacia abajo
        // 5: Disparar (si el cooldown lo permite)
        int action = actionBuffers.DiscreteActions[0];

        AddReward(-0.001f);

        if (action == 5) AddReward(0.05f);

        Vector3 toTarget = targetTransform.position - transform.position;
        float angle = Vector3.Angle(transform.forward, toTarget);
        if (angle < 15f) AddReward(0.01f);

        switch (action)
        {
            case 0:
                // No hacer nada.
                break;
            case 1:
                // Rotar a la izquierda.
                transform.Rotate(Vector3.up, -rotationSpeed * Time.deltaTime);
                break;
            case 2:
                // Rotar a la derecha.
                transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
                break;
            case 3:
                // Rotar verticalmente hacia arriba.
                // Suponiendo que girar hacia arriba disminuye el ángulo de pitch.
                currentPitch = Mathf.Clamp(currentPitch - rotationSpeed * Time.deltaTime, -45f, 60f);
                ApplyVerticalRotation();
                break;
            case 4:
                // Rotar verticalmente hacia abajo.
                currentPitch = Mathf.Clamp(currentPitch + rotationSpeed * Time.deltaTime, -45f, 60f);
                ApplyVerticalRotation();
                break;
            case 5:
                // Disparar solo si ha pasado el cooldown.
                Shoot();
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// Aplica el ángulo vertical (pitch) al agente, manteniendo la rotación horizontal intacta.
    /// </summary>
    void ApplyVerticalRotation()
    {
        // Recupera los ángulos actuales.
        Vector3 euler = transform.localEulerAngles;
        // Convertir el pitch guardado en "currentPitch" (en rango de -45 a 60) a un ángulo en el rango 0-360
        // para asignarlo a la propiedad localEulerAngles.x.
        float angleToSet = currentPitch < 0 ? 360 + currentPitch : currentPitch;
        euler.x = angleToSet;
        transform.localEulerAngles = euler;
    }

    /// <summary>
    /// Método que instancia un proyectil en la dirección en la que el agente está mirando.
    /// El proyectil llevará la referencia al agente para asignar recompensas en base a la colisión.
    /// </summary>
    void Shoot()
    {
        if (Time.time - lastShotTime >= shootCooldown)
        {
            //Debug.Log("Disparo ejecutado en " + Time.time);
            // Posición de spawn: un poco adelante para evitar colisión con el propio agente.
        Vector3 spawnPos = transform.position + transform.forward;
        GameObject projectile = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);

        // Añadir o usar el Rigidbody para mover el proyectil.
        Rigidbody rb = projectile.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = projectile.AddComponent<Rigidbody>();
        }
        rb.velocity = transform.forward * projectileSpeed;

        // Asignar la referencia del agente al script del proyectil.
        Projectile projScript = projectile.GetComponent<Projectile>();
        if (projScript != null)
        {
            projScript.shooter = this;
        }
        // Se destruye el proyectil tras un tiempo para evitar saturar la escena.
        Destroy(projectile, 3f);

        lastShotTime = Time.time;
       }
        
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActionsOut = actionsOut.DiscreteActions;
        
        // Configura las teclas para depuración:
        // Izquierda: rotar a la izquierda (1)
        // Derecha: rotar a la derecha (2)
        // Flecha arriba: rotar hacia arriba (3)
        // Flecha abajo: rotar hacia abajo (4)
        // Barra espaciadora: disparar (5)
        // Si no se presiona nada, se envía 0 (sin acción).
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            discreteActionsOut[0] = 1;
        }
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            discreteActionsOut[0] = 2;
        }
        else if (Input.GetKey(KeyCode.UpArrow))
        {
            discreteActionsOut[0] = 3;
        }
        else if (Input.GetKey(KeyCode.DownArrow))
        {
            discreteActionsOut[0] = 4;
        }
        else if (Input.GetKeyDown(KeyCode.Space))
        {
            discreteActionsOut[0] = 5;
        }
        else
        {
            discreteActionsOut[0] = 0;
        }
    }

    public override void OnEpisodeBegin()
    {
        // Reinicia la orientación del agente y reinicia el ángulo vertical (pitch).
        transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
        // Se inicializa currentPitch a partir del ángulo local actual.
        currentPitch = transform.localEulerAngles.x;
        if (currentPitch > 180f) currentPitch -= 360f;
    }
}
