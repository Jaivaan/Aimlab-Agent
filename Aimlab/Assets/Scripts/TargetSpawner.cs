using UnityEngine;

public class TargetSpawner : MonoBehaviour
{
    // Prefab del objetivo a instanciar (asegúrate de que sea una esfera y tenga un Collider)
    public GameObject targetPrefab;

    // Centro y tamaño del área donde se generarán los objetivos.
    // Por ejemplo, spawnAreaCenter puede ser la posición del centro de la zona de spawn
    // y spawnAreaSize define el tamaño del rectángulo (o caja) en el que se ubicarán los objetivos.
    public Vector3 spawnAreaCenter = Vector3.zero;
    public Vector3 spawnAreaSize = new Vector3(10f, 1f, 10f);

    // Tiempo (en segundos) de espera para spawnear un nuevo objetivo una vez que se destruye el anterior
    public float spawnDelay = 0.5f;

    // Referencia al objetivo actual en la escena.
    private GameObject currentTarget;

    void Start()
    {
        // Spawnea el primer objetivo al iniciar la escena
        SpawnTarget();
    }

    void Update()
    {
        // Si el objetivo fue destruido y no se tiene ya una invocación pendiente, se programa el spawn del siguiente
        if (currentTarget == null && !IsInvoking("SpawnTarget"))
        {
            Invoke("SpawnTarget", spawnDelay);
        }
    }

    /// <summary>
    /// Instancia un nuevo objetivo en una posición aleatoria dentro del área definida.
    /// Además, se asegura de que el objeto tenga la etiqueta "Target" (para que el disparo del agente lo reconozca).
    /// </summary>
    void SpawnTarget()
    {
        Vector3 spawnPosition = GetRandomPosition();
        currentTarget = Instantiate(targetPrefab, spawnPosition, Quaternion.identity);
        currentTarget.tag = "Target"; // Es importante que coincida con la comprobación del agente.
    }

    /// <summary>
    /// Calcula una posición aleatoria dentro de la zona delimitada.
    /// Se asume que se quiere variar en los ejes X y Z, manteniendo el Y fijo.
    /// </summary>
    /// <returns>Una posición en el espacio para spawnear el objetivo.</returns>
    Vector3 GetRandomPosition()
    {
        float randomX = Random.Range(-spawnAreaSize.x / 2, spawnAreaSize.x / 2);
        float randomZ = Random.Range(-spawnAreaSize.z / 2, spawnAreaSize.z / 2);
        return spawnAreaCenter + new Vector3(randomX, 0f, randomZ);
    }

    // Dibuja en el editor una representación del área de spawn para facilitar su configuración
    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(spawnAreaCenter, spawnAreaSize);
    }
}
