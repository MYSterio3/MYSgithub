using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC_GiftSystem : MonoBehaviour
{
    [Header("")]
    [SerializeField] private NPC_Controller _controller;

    [Header("")]
    [SerializeField] private GameObject _giftCoolTimeBar;
    [SerializeField] private AmountBar _coolTimeBar;

    [Header("")]
    [SerializeField][Range(0, 100)] private float _itemDropRate;
    [SerializeField][Range(0, 100)] private float _collectCardDropRate;

    [Header("")]
    [SerializeField][Range(0, 100)] private int _dropCoolTime;
    [SerializeField][Range(0, 100)] private int _dropAmountRange;


    private Coroutine _coroutine;


    // UnityEngine
    private void Start()
    {
        _giftCoolTimeBar.SetActive(false);

        _coolTimeBar.Set_Amount(_dropCoolTime);
        _coolTimeBar.Toggle(true);

        // subscriptions
        _controller.interactable.OnHoldIInteract += ToggleBar_Duration;
        _controller.interactable.OnHoldIInteract += Gift;

        GlobalTime_Controller.TimeTik_Update += Update_CoolTime;
    }

    private void OnDestroy()
    {
        // subscriptions
        _controller.interactable.OnHoldIInteract -= ToggleBar_Duration;
        _controller.interactable.OnHoldIInteract -= Gift;

        GlobalTime_Controller.TimeTik_Update -= Update_CoolTime;
    }


    //
    private void ToggleBar_Duration()
    {
        if (_coroutine != null) return;

        _coroutine = StartCoroutine(ToggleBar_Duration_Coroutine());
    }
    private IEnumerator ToggleBar_Duration_Coroutine()
    {
        Update_CoolTimBar();
        _giftCoolTimeBar.SetActive(true);

        yield return new WaitForSeconds(2f);

        _giftCoolTimeBar.SetActive(false);

        _coroutine = null;
        yield break;
    }


    private void Update_CoolTime()
    {
        if (_coolTimeBar.currentAmount >= _dropCoolTime) return;

        _coolTimeBar.Update_Amount(1);
        Update_CoolTimBar();
    }

    private void Update_CoolTimBar()
    {
        _coolTimeBar.Toggle_BarColor(_coolTimeBar.currentAmount >= _dropCoolTime);
        _coolTimeBar.Load_Custom(_dropCoolTime, _coolTimeBar.currentAmount);
    }


    private bool Gift_Available()
    {
        // check if cool time complete
        if (_coolTimeBar.currentAmount < _dropCoolTime) return false;

        // check if food serve waiting
        if (_controller.foodIcon.hasFood) return false;

        // check if current position is claimed
        if (_controller.mainController.Position_Claimed(Main_Controller.SnapPosition(transform.position))) return false;

        FoodData_Controller playerFoodIcon = _controller.interactable.detection.player.foodIcon;

        // check if player has gift food
        if (playerFoodIcon.hasFood == false) return false;

        return true;
    }

    private void Gift()
    {
        if (Gift_Available() == false) return;

        FoodData_Controller playerFoodIcon = _controller.interactable.detection.player.foodIcon;
        Food_ScrObj playerFood = playerFoodIcon.currentData.foodScrObj;

        // remove player current food
        playerFoodIcon.Set_CurrentData(null);
        playerFoodIcon.Show_Icon();
        playerFoodIcon.Toggle_SubDataBar(true);
        playerFoodIcon.Show_Condition();

        // start cool time
        _coolTimeBar.Set_Amount(0);
        Update_CoolTimBar();

        // check drop rate
        if (Main_Controller.Percentage_Activated(_controller.characterData.generosityLevel, _itemDropRate) == false) return;

        ItemDropper dropper = _controller.itemDropper;

        // collect card drop
        if (Main_Controller.Percentage_Activated(_controller.characterData.generosityLevel, _collectCardDropRate))
        {
            dropper.Drop_CollectCard();
            return;
        }

        // food drop
        int dropAmount = Random.Range(1, _dropAmountRange + 1);
        dropper.Drop_Food(new FoodData(dropper.Weighted_RandomFood(playerFood)), dropAmount);
    }
}