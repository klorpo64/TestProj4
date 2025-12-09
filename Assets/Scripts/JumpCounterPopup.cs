using UnityEngine;
using TMPro;

public class JumpCounterPopup : MonoBehaviour
{
    public TextMeshProUGUI counterText;
    public float lifeTime = 1f;
    private Transform cam;

    public void Initialize(int number)
    {
        counterText.text = number.ToString();
    }

    private void Start()
    {
        cam = Camera.main.transform;
        Destroy(gameObject, lifeTime);
    }

    private void LateUpdate()
    {
        if (cam != null)
        {
            // Billboard
            Vector3 forward = transform.position - cam.position;
            transform.rotation = Quaternion.LookRotation(forward);
        }
    }
}