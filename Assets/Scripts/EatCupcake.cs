using UnityEngine;

public class EatCupcake : MonoBehaviour
{
    private KratosConversationStarter conversationStarter;
    private Collider col;

    void Awake()
    {
        conversationStarter = GetComponent<KratosConversationStarter>();
        col = GetComponent<Collider>();
    }

    public void Cupcake()
    {
        // Hide the cupcake visually
        gameObject.SetActive(false);

        // Disable collider so it can't trigger interaction
        if (col != null)
            col.enabled = false;

        // Disable KratosConversationStarter to prevent interaction
        if (conversationStarter != null)
            conversationStarter.enabled = false;

        // Clear the target from any PlayerInteract currently referencing this cupcake
        Collider[] playersNearby = Physics.OverlapSphere(transform.position, 5f); // small radius to find players
        foreach (Collider c in playersNearby)
        {
            PlayerInteract pi = c.GetComponent<PlayerInteract>();
            if (pi != null)
            {
                pi.ClearTarget(conversationStarter);
            }
        }
    }
}