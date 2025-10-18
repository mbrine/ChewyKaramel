using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChoiceContainer : MonoBehaviour
{
    public Choice storedChoice {
        get => _storedChoice; 
        set { 
            _storedChoice = value;
            GetComponentInChildren<TMPro.TextMeshProUGUI>().text = _storedChoice.displayText;
        } 
    }
    private Choice _storedChoice;
    public EventManager eventManagerReference;

    public void OnClick()
    {
		eventManagerReference.SelectChoice(storedChoice);
    }
}
