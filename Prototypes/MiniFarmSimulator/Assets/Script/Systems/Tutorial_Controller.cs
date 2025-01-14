using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Tutorial_Controller : MonoBehaviour
{
    [SerializeField] private MainGame_Controller controller;

    [SerializeField] private GameObject optionsMenuTutorialScreen;

    [SerializeField] private float alphaSpeed;

    [SerializeField] private GameObject[] tutorialScreens;
    [SerializeField] private RectTransform[] screenBox;
    [SerializeField] private RectTransform collectableRoomArrow;

    private int currentScreenNum = 0;
    private int screenBoxNum = -1;

    // lean tween screen box
    private void LeanTween_ScreenBox()
    {
        float delayTime;
        float alphaSpeedSubtraction;
        
        // next day leanTween delay for nextDay animation
        if (currentScreenNum == 17)
        {
            delayTime = 4f; 
            alphaSpeedSubtraction = 0f;
        }
        else
        {
            delayTime = 0.75f;
            alphaSpeedSubtraction = 0f;
        }

        // leanTween
        LeanTween.alpha(screenBox[screenBoxNum], 0, alphaSpeed - alphaSpeedSubtraction).setOnComplete(() => { screenBox[screenBoxNum].gameObject.SetActive(false); }).setDelay(delayTime);

        // collectable room arrow movement
        if (currentScreenNum != 19) return;
        LeanTween.alpha(collectableRoomArrow, 0.6f, alphaSpeed).setDelay(delayTime * 2);
    }
    private void Reset_ScreenBox_Status()
    {
        for (int i = 0; i < screenBox.Length; i++)
        {
            screenBox[i].gameObject.SetActive(true);
            LeanTween.alpha(screenBox[i], 0.6f, 0);
        }

        LeanTween.alpha(collectableRoomArrow, 0f, 0);
    }

    // opitons menu
    public void Press_OpitonsMenu_DuringTutorial()
    {
        if (controller.optionsMenu.data.menuOn)
        {
            // turn off current guide screen 
            tutorialScreens[currentScreenNum].SetActive(false);

            // turn on options menu tutorial screen
            optionsMenuTutorialScreen.SetActive(true);
        }
        else
        {
            // turn off options menu tutorial screen
            optionsMenuTutorialScreen.SetActive(false);

            // turn on current guide screen 
            tutorialScreens[currentScreenNum].SetActive(true);
        }
    }

    // tutorial guide
    public void Skip_TutorialGuide()
    {
        controller.saveSystem.data.tutorialComplete = true;
        Destroy(gameObject);
    }

    public void Start_Guide_Screen()
    {
        // set guide screen number to first page
        currentScreenNum = 0;
        screenBoxNum = -1;
        
        // turn on current starting guide screen 
        tutorialScreens[currentScreenNum].SetActive(true);
    }
    public void Next_Guide_Screen()
    {
        // turn off current guide screen
        tutorialScreens[currentScreenNum].SetActive(false);

        // increase currentScreenNum and currentBoxNum
        currentScreenNum++;
        screenBoxNum++;

        // last guide screen check
        if (tutorialScreens.Length - currentScreenNum == 1)
        {
            controller.Reset_All_Menu();
        }

        // tutorial end check
        if (currentScreenNum >= tutorialScreens.Length)
        {
            controller.saveSystem.data.tutorialComplete = true;
            Destroy(gameObject);
            return;
        }

        // go to next guide screen
        tutorialScreens[currentScreenNum].SetActive(true);

        // screen box lean tween end check
        if (screenBoxNum >= screenBox.Length) return;
        
        // screen box lean tween
        LeanTween_ScreenBox();
    }

    public void Replay_TutorialGuide()
    {
        // turn off current last screen
        tutorialScreens[currentScreenNum].SetActive(false);

        // go back to first guide screen
        currentScreenNum = 0;
        screenBoxNum = -1;
        Reset_ScreenBox_Status();
        Next_Guide_Screen();

        // reset to day 1
        controller.timeSystem.currentInGameDay = 1;
        controller.defaultMenu.Update_UI();

        // reset the collectable that was bought at tutorial
        controller.collectableRoomMenu.Reset_AllCollectables_Data();

        // lock guide farmtile
        var guideFarmTile = controller.farmTiles[0];
        guideFarmTile.Lock_Tile();

        // reset buff menu page to 1
        controller.buffMenu.pageController.FisrtPage();
    }

    // public tutorial functions
    public void Tutorial_FarmTile_Update()
    {
        var guideFarmTile = controller.farmTiles[0];

        guideFarmTile.tileSeedStatus.dayPassed = guideFarmTile.tileSeedStatus.fullGrownDay;
        guideFarmTile.tileSeedStatus.harvestReady = true;
        guideFarmTile.tileSeedStatus.health = 3;
        guideFarmTile.tileSeedStatus.bonusPoints = 4;
    }
}
