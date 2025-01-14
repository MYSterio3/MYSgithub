using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class Shop_Menu_Data
{
    public bool menuOn;
    public Collectable_ScrObj lastWinCollectable;
}

[System.Serializable]
public class Shop_Menu_UI
{
    public RectTransform shopMenuPanel;
    public Animator anim, fireWorkAnim;
    public GameObject rollAnimFrame, stopAnimFrame, fireWork;
    public Image stopFrame, stopCollectableImage;
    public Button gachaButton;
    public Text gachaPriceText;
}

public class Shop_Menu : MonoBehaviour
{
    public MainGame_Controller controller;
    public LeanTweenType tweenType;
    public Button[] allAvailableButtons;

    public Shop_Menu_Data data;
    public Shop_Menu_UI ui;

    private void Start()
    {
        Center_Position();
        Set_GachaPrice_Text();
    }

    // basic ui functions
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
        ui.shopMenuPanel.anchoredPosition = new Vector2(-392.56f, 300f);
    }
    private void Open()
    {
        data.menuOn = true;
        Button_Shield(false);
        LeanTween.move(ui.shopMenuPanel, new Vector2(-107.44f, 300f), 0.75f).setEase(tweenType);

        // deactivate placemode if it is on and unselect all collectable buttons
        if (controller.collectableRoomMenu.data.placeMode)
        {
            controller.collectableRoomMenu.AllFrame_PlaceMode_Off();
            controller.collectableRoomMenu.AllButton_UnSelect();
        }
    }
    public void Close()
    {
        data.menuOn = false;
        Button_Shield(true);
        LeanTween.move(ui.shopMenuPanel, new Vector2(-392.56f, 300f), 0.75f).setEase(tweenType);
    }
    public void Open_Close()
    {
        if (data.menuOn)
        {
            Close();
        }
        else
        {
            Open();
        }
    }

    private void Set_GachaPrice_Text()
    {
        var x = controller.gachaSystem.gachaPrice;
        ui.gachaPriceText.text = "$ " + x.ToString();
    }

    // stop and activate casino roll frame 
    public void Activate_Roll_Frame()
    {
        ui.anim.SetBool("gachaPressed", false);
        ui.rollAnimFrame.SetActive(true);
        ui.stopAnimFrame.SetActive(false);
        ui.gachaButton.enabled = true;
        Reset_Fireworks();
    }

    private void Stop_Roll_Collectable_Set()
    {
        var x = controller.collectableRoomMenu.allCollectableTierData;
        for (int i = 0; i < x.Length; i++)
        {
            if (data.lastWinCollectable.colorLevel == x[i].colorLevel)
            {
                ui.stopFrame.sprite = x[i].colorButtonFrameSprite;
                ui.stopCollectableImage.sprite = data.lastWinCollectable.sprite;
                break;
            }
        }
    }
    public void Stop_Roll_Frame()
    {
        ui.rollAnimFrame.SetActive(false);
        ui.stopAnimFrame.SetActive(true);
        ui.gachaButton.enabled = false;

        Stop_Roll_Collectable_Set();
        Activate_Fireworks();
    }

    // fireworks
    private void Activate_Fireworks()
    {
        var x = controller.collectableRoomMenu.allCollectableTierData;
        for (int i = 0; i < x.Length; i++)
        {
            if (data.lastWinCollectable.colorLevel == x[i].colorLevel)
            {
                ui.fireWorkAnim.runtimeAnimatorController = x[i].fireworks;
                break;
            }
        }

        ui.fireWork.SetActive(true);
        ui.fireWorkAnim.SetBool("activate", true);
    }
    private void Reset_Fireworks()
    {
        ui.fireWork.SetActive(false);
        ui.fireWorkAnim.SetBool("activate", false);
    }
}
