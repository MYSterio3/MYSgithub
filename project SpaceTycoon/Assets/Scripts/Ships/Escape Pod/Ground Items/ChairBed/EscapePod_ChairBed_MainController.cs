using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EscapePod_ChairBed_MainController : MonoBehaviour
{
    [HideInInspector]
    public bool playerDetection;

    [HideInInspector]
    public bool chairMode = true;

    public GameObject icon;
    public GameObject iconBoxCollider;
    public GameObject mainPanel;
    public GameObject chairModeButton;
    public GameObject bedModeButton;

    [HideInInspector]
    public SpriteRenderer sr;
    public Sprite bed;
    public Sprite chair;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerDetection = true;
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerDetection = false;
        }
    }
}