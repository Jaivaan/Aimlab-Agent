using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class SimpleHallwayAgent : Agent
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

        switch (action)
        {
            case 0:
                // Sin acción
                break;
            case 1:
                // Rotar izquierda
                transform.Rotate(Vector3.up, -rotationSpeed * Time.deltaTime);
                break;
            case 2:
                // Rotar derecha
                transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
                break;
            case 3:
                // Rotar hacia arriba
                currentPitch = Mathf.Clamp(currentPitch - rotationSpeed * Time.deltaTime, -45f, 60f);
                ApplyVerticalRotation();
                break;
            case 4:
                // Rotar hacia abajo
                currentPitch = Mathf.Clamp(currentPitch + rotationSpeed * Time.deltaTime, -45f, 60f);
                ApplyVerticalRotation();
                break;
            case 5:
                // Disparar
                Shoot();
                break;
        }
    }

    void ApplyVerticalRotation()
    {
        Vector3 e = transform.localEulerAngles;
        float angle = currentPitch < 0 ? 360 + currentPitch : currentPitch;
        e.x = angle;
        transform.localEulerAngles = e;
    }

    void Shoot()
    {
        if (Time.time - lastShotTime < shootCooldown) return;
        Vector3 spawnPos = transform.position + transform.forward;
        var proj = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);
        var rb = proj.GetComponent<Rigidbody>() ?? proj.AddComponent<Rigidbody>();
        rb.velocity = transform.forward * projectileSpeed;
        var script = proj.GetComponent<Projectile>();
        if (script != null) script.shooter = this;
        Destroy(proj, 3f);
        lastShotTime = Time.time;
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var d = actionsOut.DiscreteActions;
        if      (Input.GetKey(KeyCode.LeftArrow))  d[0] = 1;
        else if (Input.GetKey(KeyCode.RightArrow)) d[0] = 2;
        else if (Input.GetKey(KeyCode.UpArrow))    d[0] = 3;
        else if (Input.GetKey(KeyCode.DownArrow))  d[0] = 4;
        else if (Input.GetKeyDown(KeyCode.Space))  d[0] = 5;
        else                                       d[0] = 0;
    }

    public override void OnEpisodeBegin()
    {
        transform.rotation = Quaternion.Euler(0f, Random.Range(0f,360f), 0f);
        currentPitch = transform.localEulerAngles.x;
        if (currentPitch > 180f) currentPitch -= 360f;
    }
}
