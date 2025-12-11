using System.Collections;
using UnityEngine;

public class RockSpawner : MonoBehaviour
{
    [SerializeField]
    GameObject rock;
    [SerializeField]
    float timeBetweenMin = 8f, timeBetweenMax = 15f;
    [SerializeField]
    float lengthExist = 60f, randomForceStrength = 5f;

    public bool alive = true;
    void Start()
    {
        StartCoroutine(SpawnRock());
        SpawnerController.OnComplete += Die;
    }

    public void Die()
    {
        alive = false;
    }

    IEnumerator SpawnRock()
    {
        while(alive)
        {
            yield return new WaitForSeconds(Random.Range(timeBetweenMin, timeBetweenMax));
            StartCoroutine(KillRock(Instantiate(rock, transform.position, transform.rotation)));
        }
    }
    IEnumerator KillRock(GameObject rock)
    {
        rock.GetComponent<Rigidbody>().AddForce(new Vector3(
            Random.Range(-1f, 1f),
            Random.Range(-1f, 1f),
            Random.Range(-1f, 1f)
        ) * randomForceStrength, ForceMode.Impulse);
        yield return new WaitForSeconds(lengthExist);
        Destroy(rock);
    }
}
