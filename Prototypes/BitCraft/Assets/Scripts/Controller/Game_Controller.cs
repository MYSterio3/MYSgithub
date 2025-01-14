using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game_Controller : MonoBehaviour
{
    private Prefabs_Data _prefabsData;
    public Prefabs_Data prefabsData { get => _prefabsData; set => _prefabsData = value; }

    [Header("Main")]
    [SerializeField] private TileMap_Controller _tilemapController;
    public TileMap_Controller tilemapController { get => _tilemapController; set => _tilemapController = value; }

    [SerializeField] private Inventory_Controller _inventoryController;
    public Inventory_Controller inventoryController { get => _inventoryController; set => _inventoryController = value; }

    [SerializeField] private Interaction_Controller _interactionController;
    public Interaction_Controller interactionController { get => _interactionController; set => _interactionController = value; }

    [Header("Status Panel")]
    [SerializeField] private Time_Controller _timeController;
    public Time_Controller timeController { get => _timeController; set => _timeController = value; }

    [SerializeField] private PlayerStat_Controller _statPanel;
    public PlayerStat_Controller statPanel { get => _statPanel; set => _statPanel = value; }

    private void Awake()
    {
        if (gameObject.TryGetComponent(out Prefabs_Data prefabsData)) { _prefabsData = prefabsData; }
    }
}