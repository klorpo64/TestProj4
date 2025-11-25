using UnityEngine;
using System.Collections;

public class BoatSplashSpawner : MonoBehaviour
{
    public GameObject splashPrefab;
    public float spawnInterval = 0.2f; // How often splashes appear
    public Vector3 spawnOffset = new Vector3(0, 0, -1f); // Behind the boat
    public float spawnRadius = 0.5f; // Random spread around back

    void Start()
    {
        StartCoroutine(SpawnSplashes());
    }

    IEnumerator SpawnSplashes()
    {
        while (true)
        {
            Vector3 randomOffset = new Vector3(
                Random.Range(-spawnRadius, spawnRadius),
                0f,
                Random.Range(-spawnRadius, spawnRadius)
            );

            Vector3 spawnPos = transform.position + spawnOffset + randomOffset;
            Instantiate(splashPrefab, spawnPos, Quaternion.Euler(90, 0, 0));

            yield return new WaitForSeconds(spawnInterval);
        }
    }
}
