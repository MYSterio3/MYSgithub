using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MailBox : MonoBehaviour, IInteractable
{
    private SpriteRenderer _sr;

    private Main_Controller _mainController;

    [SerializeField] private Detection_Controller _detection;
    [SerializeField] private AmountBar _amountBar;

    [Header("")]
    [SerializeField] private Sprite[] _sprites;

    [Header("")]
    [SerializeField] private GameObject _collectCard;

    private Coroutine _coroutine;


    // UnityEngine
    private void Awake()
    {
        Load_Data();

        _sr = gameObject.GetComponent<SpriteRenderer>();
        _mainController = GameObject.FindGameObjectWithTag("MainController").GetComponent<Main_Controller>();
    }

    private void Start()
    {
        Update_Sprite();
        _amountBar.Load();

        GlobalTime_Controller.TimeTik_Update += Drop_CollectCard;
    }

    private void OnDestroy()
    {
        Save_Data();

        GlobalTime_Controller.TimeTik_Update -= Drop_CollectCard;
    }


    // ISaveLoadable
    private void Save_Data()
    {
        ES3.Save("MailBox/_amountBar.currentAmount", _amountBar.currentAmount);
    }

    private void Load_Data()
    {
        _amountBar.Set_Amount(ES3.Load("MailBox/_amountBar.currentAmount", _amountBar.currentAmount));
    }


    // IInteractable
    public void Interact()
    {
        Insert_Nugget();
    }

    public void UnInteract()
    {
        
    }


    // Sprite Control
    private void Update_Sprite()
    {
        if (_amountBar.currentAmount <= 0)
        {
            _sr.sprite = _sprites[0];
            return;
        }

        _sr.sprite = _sprites[1];
    }


    // NPC Control
    private NPC_Controller Spawn_MailNPC()
    {
        // get current location outer position
        Location_Controller currentLocation = _mainController.currentLocation;
        Vector2 spawnPoint = currentLocation.OuterLocation_Position(1);

        // spawn
        GameObject spawnNPC = _mainController.Spawn_Character(2, spawnPoint);
        NPC_Controller npc = spawnNPC.GetComponent<NPC_Controller>();

        _mainController.UnTrack_CurrentCharacter(spawnNPC);

        return npc;
    }


    // Interact Functions
    private Vector2 Available_SpawnPoint()
    {
        List<Vector2> spawnPoints = new();

        spawnPoints.Add(new Vector2(transform.position.x + 1, transform.position.y));
        spawnPoints.Add(new Vector2(transform.position.x - 1, transform.position.y));

        for (int i = 0; i < spawnPoints.Count; i++)
        {
            if (_mainController.Position_Claimed(spawnPoints[i]) == true) continue;
            return spawnPoints[i];
        }

        return transform.position;
    }


    private void Insert_Nugget()
    {
        // check if nugget amount is max
        if (_amountBar.currentAmount >= _amountBar.amountBarSprite.Length - 1) return;

        FoodData_Controller playerFood = _detection.player.foodIcon;

        if (playerFood.hasFood == false) return;
        if (playerFood.currentData.foodScrObj != _mainController.dataController.goldenNugget) return;

        playerFood.Set_CurrentData(null);
        playerFood.Show_Icon();

        // increase amount
        _amountBar.Update_Amount(1);
        _amountBar.Load();

        Update_Sprite();
    }

    private void Drop_CollectCard()
    {
        if (_coroutine != null) return;
        if (_amountBar.currentAmount <= 0) return;
        
        _coroutine = StartCoroutine(Drop_CollectCard_Coroutine());
    }
    private IEnumerator Drop_CollectCard_Coroutine()
    {
        NPC_Movement movement = Spawn_MailNPC().movement;

        // move to current mail box
        movement.Assign_TargetPosition(transform.position);
        while (movement.At_TargetPosition() == false) yield return null;

        // check if spawn point is empty
        if (Available_SpawnPoint() != (Vector2)transform.position)
        {
            // spawn collect card
            GameObject collectCard = Instantiate(_collectCard, Available_SpawnPoint(), Quaternion.identity);
            collectCard.transform.SetParent(_mainController.otherFile);

            // decrease amount
            _amountBar.Update_Amount(-1);
            _amountBar.Load();

            Update_Sprite();
        }

        // leave
        movement.Leave(0f);
        while (movement.At_TargetPosition() == false) yield return null;

        _coroutine = null;
        yield break;
    }
}