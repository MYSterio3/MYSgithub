using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(menuName = "New Season")]
public class Season_ScrObj : ScriptableObject
{
    public Weather_ScrObj[] weatherPercentages;

    public string seasonName;

    public Sprite seasonUI;
}