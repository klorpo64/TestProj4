using UnityEngine;

public class CarSpawner : MonoBehaviour
{
    public GameObject[] carPrefabs;     // Array of different colored car prefabs
    public Transform[] spawnPoints;     // Possible spawn positions
    public float spawnInterval = 3f;    // Time between spawns
    public int maxCars = 10;            // Max cars on screen

    private int currentCars = 0;

    private void Start()
    {
        InvokeRepeating(nameof(SpawnCar), 1f, spawnInterval);
    }

    void SpawnCar()
    {
        if (currentCars >= maxCars) return;

        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];

        // Pick a random prefab
        GameObject carPrefab = carPrefabs[Random.Range(0, carPrefabs.Length)];

        GameObject car = Instantiate(carPrefab, spawnPoint.position, spawnPoint.rotation);
        currentCars++;

        // Optional: destroy after some time so the scene doesn’t get crowded
        Destroy(car, 10f);
        Invoke(nameof(DecreaseCarCount), 10f);
    }

    void DecreaseCarCount()
    {
        currentCars--;
    }
}
