using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GasPump : MonoBehaviour
{
    [Header("")]
    [SerializeField] private IInteractable_Controller _interactable;
    [SerializeField] private Detection_Controller _detection;

    [Header("")]
    [SerializeField] private Clock_Timer _fillClock;
    [SerializeField] private AmountBar _amountBar;

    [Header("")]
    [SerializeField] private Station_ScrObj _oilDrum;
    [SerializeField] private GameObject _spawnPoint;

    [Header("")]
    [SerializeField][Range(0, 100)] private int _price;
    [SerializeField][Range(0, 100)] private int _fillTime;

    private Coroutine _coroutine;


    // UnityEngine
    private void Start()
    {
        _amountBar.Set_MaxAmount(_price);
        _amountBar.Set_Amount(ES3.Load("GasPump/_amountBar.currentAmount", _amountBar.currentAmount));
        _amountBar.Load();

        Load_Data();

        AmountBar_Toggle();
        SpawnPoint_Toggle();

        // Subscriptions
        _detection.EnterEvent += AmountBar_Toggle;
        _detection.ExitEvent += AmountBar_Toggle;

        _detection.EnterEvent += SpawnPoint_Toggle;
        _detection.ExitEvent += SpawnPoint_Toggle;

        _interactable.OnInteract += Trigger_Dialog;
        _interactable.OnInteract += Insert;
    }

    private void OnDestroy()
    {
        Reset_OnFill();
        Save_Data();

        // Subscriptions
        _detection.EnterEvent -= AmountBar_Toggle;
        _detection.ExitEvent -= AmountBar_Toggle;

        _detection.EnterEvent -= SpawnPoint_Toggle;
        _detection.ExitEvent -= SpawnPoint_Toggle;

        _interactable.OnInteract -= Trigger_Dialog;
        _interactable.OnInteract -= Insert;
    }


    // ISaveLoadable
    private void Save_Data()
    {
        ES3.Save("GasPump/_amountBar.currentAmount", _amountBar.currentAmount);
    }

    private void Load_Data()
    {
        if (_amountBar.Is_MaxAmount() == false) return;

        Spawn_OilDrum();
    }


    // Indication Updates
    private void Trigger_Dialog()
    {
        DialogTrigger trigger = gameObject.GetComponent<DialogTrigger>();

        if (_coroutine != null)
        {
            trigger.Update_Dialog(0);
            return;
        }

        if (Fill_Available() == false)
        {
            trigger.Update_Dialog(1);
            return;
        }

        if (Insert_Available()) return;

        string dialogString = "Insert " + _price + " <sprite=56> to export <sprite=61>";
        DialogData dialog = new(trigger.defaultData.icon, dialogString);

        trigger.Update_Dialog(dialog);
    }


    private void AmountBar_Toggle()
    {
        bool toggle = _coroutine != null || _detection.player != null;

        _amountBar.transform.parent.gameObject.SetActive(toggle);

        if (toggle == false) return;

        _amountBar.Toggle(true);
    }

    private void SpawnPoint_Toggle()
    {
        _spawnPoint.SetActive(_detection.player != null);
    }


    // Functions
    private bool Fill_Available()
    {
        bool insertActive = _amountBar.maxAmount - _amountBar.currentAmount <= 1;
        bool spawnPositionClaimed = Main_Controller.instance.Position_Claimed(_spawnPoint.transform.position);

        if (insertActive && spawnPositionClaimed) return false;

        return true;
    }

    private bool Insert_Available()
    {
        if (_coroutine != null) return false;

        Food_ScrObj nugget = Main_Controller.instance.dataController.goldenNugget;

        // if player has nugget
        if (_detection.player.foodIcon.Is_SameFood(nugget) == false) return false;

        if (_amountBar.Is_MaxAmount()) return false;

        return true;
    }


    private void Insert()
    {
        if (Insert_Available() == false) return;
        if (Fill_Available() == false) return;

        // player update
        FoodData_Controller playerIcon = _detection.player.foodIcon;

        playerIcon.Set_CurrentData(null);

        playerIcon.Show_Icon();
        playerIcon.Toggle_SubDataBar(true);
        playerIcon.Show_Condition();

        // this pump update
        _amountBar.Toggle_BarColor(false);
        _amountBar.Update_Amount(1);
        _amountBar.Load();

        // sfx
        Audio_Controller.instance.Play_OneShot(gameObject, 1);

        if (_amountBar.Is_MaxAmount() == false) return;

        // oil drum spawn update
        Fill_OilDrum();
    }


    private void Spawn_OilDrum()
    {
        Main_Controller main = Main_Controller.instance;
        Station_Controller stationController = main.Spawn_Station(_oilDrum, _spawnPoint.transform.position);

        stationController.movement.Load_Position();

        _amountBar.Toggle_BarColor(false);
        _amountBar.Set_Amount(0);
        _amountBar.Load();

        Audio_Controller.instance.Play_OneShot(gameObject, 0);
    }

    private void Fill_OilDrum()
    {
        if (_coroutine != null) return;
        _coroutine = StartCoroutine(Fill_OilDrum_Coroutine());

        gameObject.GetComponent<DialogTrigger>().Update_Dialog(0);
    }
    private IEnumerator Fill_OilDrum_Coroutine()
    {
        // claim position before spawn
        Main_Controller.instance.Claim_Position(_spawnPoint.transform.position);

        _amountBar.Toggle_BarColor(true);
        _amountBar.Set_Amount(0);

        AmountBar_Toggle();
        _fillClock.Toggle_RunAnimation(true);

        for (int i = 0; i < _fillTime; i++)
        {
            _amountBar.Load();

            yield return new WaitForSeconds(1);

            _amountBar.Update_Amount(1);
        }

        Spawn_OilDrum();

        _fillClock.Toggle_RunAnimation(false);

        _coroutine = null;
        AmountBar_Toggle();

        yield break;
    }

    private void Reset_OnFill()
    {
        Main_Controller main = Main_Controller.instance;

        if (_coroutine == null) return;
        if (main.Position_Claimed(_spawnPoint.transform.position) == false) return;

        _amountBar.Set_Amount(_price);

        StopCoroutine(_coroutine);
        _coroutine = null;

        main.UnClaim_Position(_spawnPoint.transform.position);
    }
}
