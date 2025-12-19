using UnityEngine;

public interface IInteractable
{
    void Interact(PlayerInteract player);
    Transform GetTransform(); // for rotation
}