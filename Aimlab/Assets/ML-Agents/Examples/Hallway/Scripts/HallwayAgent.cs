using System.Collections;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class HallwayAgent : Agent
{
    // Parámetro configurable para la distancia máxima del raycast
public float rayDistance = 100f;

// Velocidad de rotación en grados por segundo.
public float rotationSpeed = 150f;



// Opcional: Podemos enviar alguna información de la dirección de disparo o la orientación
public override void CollectObservations(VectorSensor sensor)
{
    // Ejemplo: se puede observar la dirección hacia adelante del agente
    sensor.AddObservation(transform.forward);
}

public override void OnActionReceived(ActionBuffers actionBuffers)
{
    // Asumimos que usamos acciones discretas:
    // 0: No hacer nada, 1: Rotar izquierda, 2: Rotar derecha, 3: Disparar
    int action = actionBuffers.DiscreteActions[0];

    switch (action)
    {
        case 1:
            // Rotar a la izquierda
            transform.Rotate(Vector3.up, -rotationSpeed * Time.deltaTime);
            break;
        case 2:
            // Rotar a la derecha
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
            break;
        case 3:
            // Disparar mediante raycast
            Shoot();
            break;
        default:
            // Acción 0 o cualquier otra no definida: No hacer nada.
            break;
    }
}

/// <summary>
/// Realiza un disparo mediante un raycast desde la posición del agente hacia adelante.
/// Si el rayo impacta con un objeto etiquetado como "Target", se recompensa el disparo
/// y se puede destruir o marcar la diana.
/// </summary>
void Shoot()
{
    RaycastHit hit;
    Vector3 origin = transform.position; // Se puede ajustar si se desea que el origen sea, por ejemplo, la posición de la cámara
    Vector3 direction = transform.forward;

    // Para visualizar el raycast en la escena (durante 1 segundo)
    Debug.DrawRay(origin, direction * rayDistance, Color.red, 1f);

    if (Physics.Raycast(origin, direction, out hit, rayDistance))
    {
        // Si impacta un objeto que tenga la etiqueta "Target"
        if (hit.collider.CompareTag("Target"))
        {
            // Se otorga recompensa positiva al disparar a la diana
            AddReward(1.0f);

            // Opcional: destruir el objeto de la diana o ejecutar otra acción (por ejemplo, llamar a un método hit() en el target)
            Destroy(hit.collider.gameObject);
        }
        else
        {
            // Penalización leve si se dispara y se impacta otro objeto
            AddReward(-0.1f);
        }
    }
    else
    {
        // Penalización cuando se dispara y no se impacta ningún objeto
        AddReward(-0.05f);
    }
}

public override void Heuristic(in ActionBuffers actionsOut)
{
    var discreteActionsOut = actionsOut.DiscreteActions;

    // Utilizamos las flechas para rotar y la barra espaciadora para disparar
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
    // Opcional: Reiniciar la orientación del agente
    transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);

    // Aquí podrías reiniciar el estado de las dianas o reubicar al agente si es necesario
}

}
