using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class CollectableRoom_Menu_UI
{
    [HideInInspector]
    public bool menuOn = false;
    public RectTransform collectableFramesPanel, collectableRoomMenu;
}

[System.Serializable]
public class CollectableRoom_Menu_Data
{
    public bool placeMode = false;
    // public current selected collectable SrcObj
}

public class CollectableRoom_Menu : MonoBehaviour
{
    public MainGame_Controller controller;
    public LeanTweenType tweenType;
    public Button[] allAvailableButtons;

    public CollectableRoom_Menu_UI ui;
    public CollectableRoom_Menu_Data data;
    public Collectable_Frame[] frames;

    private void Start()
    {
        Center_Position();
    }

    // basic functions
    private void Button_Shield(bool activate)
    {
        for (int i = 0; i < allAvailableButtons.Length; i++)
        {
            if (activate) { allAvailableButtons[i].enabled = false; }
            else if (!activate) { allAvailableButtons[i].enabled = true; }
        }
    }
    
    private void Center_Position()
    {
        ui.collectableRoomMenu.anchoredPosition = new Vector2(0f, -125f);
    }

    private void Open()
    {
        ui.menuOn = true;
        // close all menus that are opened
        controller.Reset_All_Menu();
        // buttons available
        Button_Shield(false);
        // lean tween collectableFramesPanel
        LeanTween.move(ui.collectableFramesPanel, new Vector2(0f, 62.50972f), 0.75f).setEase(tweenType);
        // lean tween collectableRoomMenu
        LeanTween.move(ui.collectableRoomMenu, new Vector2(0f, 104.85f), 0.75f).setEase(tweenType);
        // button shield on for nextday button
        controller.defaultMenu.menuUI.nextDayButton.enabled = false;
        // turn off all farmtile status icons, button shield on for all farmtiles
        for (int i = 0; i < controller.farmTiles.Length; i++)
        {
            controller.farmTiles[i].statusIconIndicator.gameObject.SetActive(false);
            controller.farmTiles[i].button.enabled = false;
        }
    }
    public void Close()
    {
        ui.menuOn = false;
        // buttons unavailable
        Button_Shield(true);
        // lean tween collectableFramesPanel
        LeanTween.move(ui.collectableFramesPanel, new Vector2(360.04f, 62.50972f), 0.75f).setEase(tweenType);
        // lean tween collectableRoomMenu
        LeanTween.move(ui.collectableRoomMenu, new Vector2(0f, -125f), 0.75f).setEase(tweenType);
        // button shield off for nextday button
        controller.defaultMenu.menuUI.nextDayButton.enabled = true;
        // turn on all farmtile status icons, button shield off for all farmtiles
        for (int i = 0; i < controller.farmTiles.Length; i++)
        {
            controller.farmTiles[i].statusIconIndicator.gameObject.SetActive(true);
            controller.farmTiles[i].button.enabled = true;
        }
    }
    public void Open_Close()
    {
        // close if menu is open, open if menu is closed
        if (!ui.menuOn) { Open(); }
        else { Close(); }
    }

    // distinctive functions
}