using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameLayers : MonoBehaviour
{
    [SerializeField]
    private LayerMask solidObjectsLayer;
    [SerializeField]
    private LayerMask interactableLayer;
    [SerializeField]
    private LayerMask grassLayer;
    [SerializeField]
    private LayerMask playerLayer;

    public static GameLayers Instance { get; set; }

    public LayerMask SolidLayer {
        get => solidObjectsLayer;
    }
    public LayerMask InteractableLayer {
        get => interactableLayer;
    }
    public LayerMask GrassLayer {
        get => grassLayer;
    }
    public LayerMask PlayerLayer {
        get => playerLayer;
    }

    private void Awake() {
        Instance = this;
    }
}
