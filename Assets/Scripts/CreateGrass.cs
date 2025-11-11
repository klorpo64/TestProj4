using UnityEngine;

public class CreateGrass : MonoBehaviour
{
    [SerializeField]
    Vector2 bottomLeft, topRight;
    [SerializeField]
    GameObject grass;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField]
    float sizeMin, sizeMax, probability = 0.95f;
    void Start()
    {
        for (float i = bottomLeft.x; i < topRight.x; i += 0.5f) {
            for (float j = bottomLeft.y; j < topRight.y; j += 0.5f) {
                if (Random.Range(0f, 1f) > probability) {
                    GameObject obj = Instantiate(grass, transform);
                    
                    float scale = Random.Range(sizeMin, sizeMax);
                    obj.transform.localScale = Vector3.one * scale;
                    obj.transform.localScale += new Vector3(0f, scale * Random.Range(0.8f, 2f), 0f);
                    obj.transform.position = 
                            new Vector3(i, scale + 2, j);
                }
            }
        }
    }
}
