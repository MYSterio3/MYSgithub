using System.Collections.Generic;
using UnityEngine;

public class StationMenu_Controller : MonoBehaviour, IVehicleMenu, ISaveLoadable
{
    [Header("")]
    [SerializeField] private VehicleMenu_Controller _controller;

    [Header("")]
    [SerializeField] private ItemSlots_Controller _slotsController;
    public ItemSlots_Controller slotsController => _slotsController;

    private bool _interactionMode;
    private Station_Controller _interactStation;

    private int _targetNum;



    // UnityEngine
    private void OnEnable()
    {
        _controller.MenuOpen_Event += Update_Slots_Data;
        _controller.AssignMain_ItemSlots(_slotsController.itemSlots);

        _controller.OnSelect_Input += Select_Slot;

        _controller.OnHoldSelect_Input += Export_StationPrefab;
        _controller.OnHoldSelect_Input += Toggle_RetrieveStations;
    }

    private void OnDisable()
    {
        // save current dragging item before menu close
        Drag_Cancel();

        _controller.MenuOpen_Event -= Update_Slots_Data;

        _controller.OnSelect_Input -= Select_Slot;

        _controller.OnHoldSelect_Input -= Export_StationPrefab;
        _controller.OnHoldSelect_Input -= Toggle_RetrieveStations;
    }

    private void OnDestroy()
    {
        OnDisable();
    }



    // ISaveLoadable
    public void Save_Data()
    {
        List<ItemSlot> currentSlots = _slotsController.itemSlots;
        List<ItemSlot_Data> saveSlots = new();

        for (int i = 0; i < currentSlots.Count; i++)
        {
            saveSlots.Add(currentSlots[i].data);
        }

        ES3.Save("StationMenu_Controller/_itemSlotDatas", saveSlots);
    }

    public void Load_Data()
    {
        List<ItemSlot_Data> loadSlots = ES3.Load("StationMenu_Controller/_itemSlotDatas", new List<ItemSlot_Data>());

        _slotsController.Add_Slot(loadSlots.Count);

        for (int i = 0; i < loadSlots.Count; i++)
        {
            _slotsController.itemSlots[i].data = loadSlots[i];
        }

        // default slots amount
        if (ES3.KeyExists("StationMenu_Controller/_itemSlotDatas")) return;

        _slotsController.Add_Slot(5);
    }



    // IVehicleMenu
    public bool MenuInteraction_Active()
    {
        return _interactionMode;
    }



    /// <summary>
    /// Render sprites or amounts according to slot's current loaded data
    /// </summary>
    private void Update_Slots_Data()
    {
        List<ItemSlot> currentSlots = _slotsController.itemSlots;

        for (int i = 0; i < currentSlots.Count; i++)
        {
            currentSlots[i].Assign_Item(currentSlots[i].data.currentStation);
        }
    }



    // Check
    public int Station_Amount(Station_ScrObj station)
    {
        List<ItemSlot> currentSlots = _slotsController.itemSlots;
        int count = 0;

        for (int i = 0; i < currentSlots.Count; i++)
        {
            if (currentSlots[i].data.hasItem == false) continue;
            if (currentSlots[i].data.currentStation != station) continue;
            count++;
        }

        return count;
    }



    // Menu Control
    public void Add_StationItem(Station_ScrObj station, int amount)
    {
        List<ItemSlot> currentSlots = _slotsController.itemSlots;
        int repeatAmount = amount;

        for (int i = 0; i < currentSlots.Count; i++)
        {
            if (currentSlots[i].data.hasItem == true) continue;

            currentSlots[i].Assign_Item(station);
            repeatAmount--;

            if (repeatAmount > 0) continue;
            break;
        }
    }



    // Slot and Cursor Control
    private void Select_Slot()
    {
        if (_interactionMode) return;

        ItemSlot_Cursor cursor = _controller.cursor;

        if (cursor.data.hasItem == false)
        {
            Drag_Station();
            return;
        }

        if (cursor.currentSlot.data.hasItem)
        {
            Swap_Station();
            return;
        }

        Drop_Station();
    }

    //
    private void Drag_Station()
    {
        ItemSlot_Cursor cursor = _controller.cursor;
        ItemSlot currentSlot = cursor.currentSlot;
        Station_ScrObj slotStation = currentSlot.data.currentStation;

        cursor.Assign_Item(slotStation);
        currentSlot.Empty_ItemBox();
    }

    private void Drag_Cancel()
    {
        ItemSlot_Cursor cursor = _controller.cursor;

        if (cursor.data.hasItem == false) return;

        Add_StationItem(cursor.data.currentStation, 1);
        cursor.Empty_Item();
    }

    //
    private void Drop_Station()
    {
        ItemSlot_Cursor cursor = _controller.cursor;
        ItemSlot currentSlot = cursor.currentSlot;

        currentSlot.Assign_Item(cursor.data.currentStation);
        cursor.Empty_Item();
    }

    //
    private void Swap_Station()
    {
        ItemSlot_Cursor cursor = _controller.cursor;
        ItemSlot currentSlot = cursor.currentSlot;

        Station_ScrObj cursorStation = cursor.data.currentStation;

        cursor.Assign_Item(currentSlot.data.currentStation);
        currentSlot.Assign_Item(cursorStation);
    }



    // Station Export System
    private void Export_StationPrefab()
    {
        if (_interactionMode) return;

        ItemSlot_Cursor cursor = _controller.cursor;
        if (cursor.data.hasItem == false) return;

        _interactionMode = true;

        Vehicle_Controller vehicle = _controller.vehicleController;
        Station_ScrObj cursorStation = cursor.data.currentStation;

        if (cursorStation.unRetrievable)
        {
            Vector2 randPos = vehicle.mainController.currentLocation.Location_Random_SnapPosition();

            _interactStation = vehicle.mainController.Spawn_Station(cursorStation, randPos);
            _interactStation.movement.Load_Position();

            Complete_StationPlace();

            return;
        }

        _interactStation = vehicle.mainController.Spawn_Station(cursorStation, vehicle.stationSpawnPoint.position);

        Station_Movement movement = _interactStation.movement;

        _interactStation.Action1_Event += movement.Set_Position;

        _controller.OnOption1_Input += Place_StationPrefab;
        _controller.OnExit_Input += Cancel_Export;
    }

    private void Place_StationPrefab()
    {
        if (_interactionMode == false) return;

        Vehicle_Controller vehicle = _controller.vehicleController;

        if (_interactStation.movement.PositionSet_Available() == false)
        {
            // return back to spawn position
            _interactStation.transform.localPosition = vehicle.stationSpawnPoint.position;
            return;
        }

        Complete_StationPlace();
    }
    //
    private void Complete_StationPlace()
    {
        _controller.cursor.Empty_Item();

        _interactionMode = false;
        _interactStation = null;

        Vehicle_Controller vehicle = _controller.vehicleController;
        Main_Controller main = vehicle.mainController;

        main.Sort_CurrentStation_fromClosest(vehicle.transform);

        _controller.OnOption1_Input -= Place_StationPrefab;
        _controller.OnExit_Input -= Cancel_Export;
    }

    private void Cancel_Export()
    {
        ItemSlot_Cursor cursor = _controller.cursor;
        if (cursor.data.hasItem == false) return;

        _interactionMode = false;

        // return exported station back to current slot
        Add_StationItem(_interactStation.stationScrObj, 1);
        cursor.Empty_Item();

        // destroy current exported station
        _interactStation.Destroy_Station();
        _interactStation = null;

        _controller.OnOption1_Input -= Place_StationPrefab;
        _controller.OnExit_Input -= Cancel_Export;
    }



    // Station Retrieve System
    private void Station_TargetDirection_Control(float xInputDirection)
    {
        List<Station_Controller> retrievableStations = _controller.vehicleController.mainController.Retrievable_Stations();

        int convertedDireciton = (int)xInputDirection;
        int nextStationNum = _targetNum + convertedDireciton;

        // less than min 
        if (nextStationNum < 0)
        {
            nextStationNum = retrievableStations.Count - 1;
        }

        // more than max
        if (nextStationNum > retrievableStations.Count - 1)
        {
            nextStationNum = 0;
        }

        _interactStation.TransparentBlink_Toggle(false);
        _interactStation.movement.movementArrows.SetActive(false);

        _targetNum = nextStationNum;
        _interactStation = retrievableStations[_targetNum];

        _interactStation.TransparentBlink_Toggle(true);
        _interactStation.movement.movementArrows.SetActive(true);
    }

    private void Toggle_RetrieveStations()
    {
        if (_interactionMode) return;

        ItemSlot_Cursor cursor = _controller.cursor;
        if (cursor.data.hasItem) return;

        List<Station_Controller> retrievableStations = _controller.vehicleController.mainController.Retrievable_Stations();
        if (retrievableStations.Count <= 0) return;

        // cursor actually doesn't have item but need this for OnOption1_Input?.Invoke(); to work on VehicleMenu_Controller
        cursor.data.hasItem = true;

        _interactionMode = true;
        _controller.OnCursorControl_Input += Station_TargetDirection_Control;
        _controller.OnOption1_Input += Retrieve_StationPrefab;
        _controller.OnExit_Input += Cancel_Retrieve;

        _interactStation = retrievableStations[0];

        _interactStation.TransparentBlink_Toggle(true);
        _interactStation.movement.movementArrows.SetActive(true);
    }

    private void Retrieve_StationPrefab()
    {
        if (_interactionMode == false) return;

        ItemSlot_Cursor cursor = _controller.cursor;
        ItemSlot currentSlot = cursor.currentSlot;

        Station_ScrObj interactStation = _interactStation.stationScrObj;

        // destroy prefab for retrieve to slot
        _interactStation.Destroy_Station();

        // retrieve
        if (currentSlot.data.hasItem == false)
        {
            currentSlot.Assign_Item(_interactStation.stationScrObj);
        }

        // swap
        else
        {
            Main_Controller main = _controller.vehicleController.mainController;

            // exporting station
            Station_Controller exportStation = main.Spawn_Station(currentSlot.data.currentStation, _interactStation.transform.position);
            exportStation.movement.Load_Position();

            // retrieving station
            currentSlot.Assign_Item(interactStation);
        }

        // cursor actually doesn't have item but need this for OnOption1_Input?.Invoke(); to work on VehicleMenu_Controller
        cursor.data.hasItem = false;

        _interactionMode = false;
        _controller.OnCursorControl_Input -= Station_TargetDirection_Control;
        _controller.OnOption1_Input -= Retrieve_StationPrefab;
        _controller.OnExit_Input -= Cancel_Retrieve;

        _interactStation = null;
    }

    private void Cancel_Retrieve()
    {
        // cursor actually doesn't have item but need this for OnOption1_Input?.Invoke(); to work on VehicleMenu_Controller
        _controller.cursor.data.hasItem = false;

        _interactionMode = false;

        _controller.OnCursorControl_Input -= Station_TargetDirection_Control;
        _controller.OnOption1_Input -= Retrieve_StationPrefab;
        _controller.OnExit_Input -= Cancel_Retrieve;

        _interactStation.TransparentBlink_Toggle(false);
        _interactStation.movement.movementArrows.SetActive(false);

        _interactStation = null;
    }
}