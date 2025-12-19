using System;
using System.Collections.Generic;
using UnityEngine;

public static class SpawnerController
{
    public static event Action OnComplete;

    public static void Completed()
    {
        OnComplete?.Invoke();
    }
}
