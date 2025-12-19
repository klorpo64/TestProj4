using UnityEngine;

public class CarSpawner : MonoBehaviour
{
    public GameObject[] carPrefabs;
    public GameObject carJumpZonePrefab;
    public Transform[] spawnPoints;

    public float spawnInterval = 3f;
    public int maxCars = 10;
    public float carLifetime = 10f;

    private int currentCars = 0;

    private void Start()
    {
        InvokeRepeating(nameof(SpawnCar), 1f, spawnInterval);
    }

    void SpawnCar()
    {
        if (currentCars >= maxCars) return;

        Transform point = spawnPoints[Random.Range(0, spawnPoints.Length)];
        GameObject carPrefab = carPrefabs[Random.Range(0, carPrefabs.Length)];

        GameObject car = Instantiate(carPrefab, point.position, point.rotation);

        if (carJumpZonePrefab != null)
        {
            GameObject zone = Instantiate(carJumpZonePrefab, car.transform.position, Quaternion.identity);
            zone.transform.SetParent(car.transform);
        }

        currentCars++;
        Destroy(car, carLifetime);
        Invoke(nameof(DecreaseCar), carLifetime);
    }

    void DecreaseCar()
    {
        currentCars--;
    }
}
