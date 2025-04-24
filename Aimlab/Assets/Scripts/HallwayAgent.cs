using System.Collections;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class HallwayAgent : Agent
{
    public float rotationSpeed = 150f;
    public GameObject projectilePrefab;
    public float projectileSpeed = 30f;

    public float shootCooldown = 0.5f;
    private float lastShotTime = -Mathf.Infinity;
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
        int action = actionBuffers.DiscreteActions[0];

        AddReward(-0.001f);

        if (action == 5) AddReward(0.01f);

        Vector3 toTarget = targetTransform.position - transform.position;
        float angle = Vector3.Angle(transform.forward, toTarget);
        if (angle < 15f) AddReward(0.002f);

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
                currentPitch = Mathf.Clamp(currentPitch - rotationSpeed * Time.deltaTime, -45f, 60f);
                ApplyVerticalRotation();
                break;
            case 4:
                // Rotar verticalmente hacia abajo.
                currentPitch = Mathf.Clamp(currentPitch + rotationSpeed * Time.deltaTime, -45f, 60f);
                ApplyVerticalRotation();
                break;
            case 5:
                // Disparar.
                Shoot();
                break;
            default:
                break;
        }
    }

    void ApplyVerticalRotation()
    {
        Vector3 euler = transform.localEulerAngles;
        float angleToSet = currentPitch < 0 ? 360 + currentPitch : currentPitch;
        euler.x = angleToSet;
        transform.localEulerAngles = euler;
    }

    void Shoot()
    {
        if (Time.time - lastShotTime >= shootCooldown)
        {
        Vector3 spawnPos = transform.position + transform.forward;
        GameObject projectile = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);

        Rigidbody rb = projectile.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = projectile.AddComponent<Rigidbody>();
        }
        rb.velocity = transform.forward * projectileSpeed;

        Projectile projScript = projectile.GetComponent<Projectile>();
        if (projScript != null)
        {
            projScript.shooter = this;
        }
        Destroy(projectile, 3f);

        lastShotTime = Time.time;
       }
        
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActionsOut = actionsOut.DiscreteActions;
        
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
        transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
        currentPitch = transform.localEulerAngles.x;
        if (currentPitch > 180f) currentPitch -= 360f;
    }
}
