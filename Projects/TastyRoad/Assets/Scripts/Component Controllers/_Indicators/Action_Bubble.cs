using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Action_Bubble : MonoBehaviour
{
    [Header("")]
    [SerializeField] private GameObject _toggle;
    [SerializeField] private GameObject _leftBubble;
    [SerializeField] private GameObject _rightBubble;

    [Header("")]
    [SerializeField] private SpriteRenderer _leftIcon;
    public SpriteRenderer leftIcon => _leftIcon;

    [SerializeField] private SpriteRenderer _rightIcon;
    public SpriteRenderer rightIcon => _rightIcon;

    private bool _bubbleOn;
    public bool bubbleOn => _bubbleOn;


    // MonoBehaviour
    private void Start()
    {
        Toggle(false);
    }


    // Show and Hide Action Bubble
    public void Toggle(bool toggleOn)
    {
        if (toggleOn)
        {
            Update_Bubble(_leftIcon.sprite, _rightIcon.sprite);
            return;
        }

        _bubbleOn = false;
        _toggle.SetActive(false);
    }


    // Update Bubble Icon Sprite
    public void Set_Bubble(Sprite leftIcon, Sprite rightIcon)
    {
        if (_leftIcon == null) return;
        _leftIcon.sprite = leftIcon;

        if (_rightIcon == null) return;
        _rightIcon.sprite = rightIcon;
    }
    public void Set_Bubble(Food_ScrObj leftFood, Food_ScrObj rightFood)
    {
        // reposition left food icon to the center of the bubble
        _leftIcon.transform.localPosition = leftFood.centerPosition;

        // reposition right food icon to the center of the bubble
        Sprite rightFoodSprite = null;

        if (rightFood != null)
        {
            rightFoodSprite = rightFood.sprite;
            _rightIcon.transform.localPosition = rightFood.centerPosition;
        }

        //
        Set_Bubble(leftFood.sprite, rightFoodSprite);
    }

    public void Update_Bubble(Sprite leftIcon, Sprite rightIcon)
    {
        // turn off if bubble is on || no sprites are assigned
        if (_toggle.activeSelf == true || leftIcon == null)
        {
            Toggle(false);
            return;
        }

        // toggle on
        _bubbleOn = true;

        _toggle.SetActive(true);

        // left bubble toggle on
        _leftBubble.SetActive(true);
        _leftIcon.sprite = leftIcon;

        // right bubble toggle
        if (rightIcon != null)
        {
            _rightBubble.SetActive(true);
            _rightIcon.sprite = rightIcon;
        }
        else
        {
            _rightBubble.SetActive(false);
        }

        Update_BubblePosition();
    }
    public void Update_Bubble(Food_ScrObj leftFood, Food_ScrObj rightFood)
    {
        // reposition left food icon to the center of the bubble
        _leftIcon.transform.localPosition = leftFood.centerPosition;

        // reposition right food icon to the center of the bubble
        Sprite rightFoodSprite = null;

        if (rightFood != null)
        {
            rightFoodSprite = rightFood.sprite;
            _rightIcon.transform.localPosition = rightFood.centerPosition;
        }

        //
        Update_Bubble(leftFood.sprite, rightFoodSprite);
    }

    public void Empty_Bubble()
    {
        _leftIcon.sprite = null;
        _rightIcon.sprite = null;
    }

    // Update Bubble Position
    private void Update_BubblePosition()
    {
        Vector2 leftPos = _leftBubble.transform.localPosition;
        Vector2 rightPos = _rightBubble.transform.localPosition;

        if (_rightIcon.sprite == null)
        {
            _leftBubble.transform.localPosition = new Vector2(0f, leftPos.y);
        }
        else
        {
            _leftBubble.transform.localPosition = new Vector2(-0.4f, leftPos.y);
            _rightBubble.transform.localPosition = new Vector2(0.4f, rightPos.y);
        }
    }
}