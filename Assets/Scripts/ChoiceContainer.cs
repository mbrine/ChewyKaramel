using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChoiceContainer : MonoBehaviour
{
    public string storedChoice
    {
        get => _storedChoice;
        set
        {
            _storedChoice = value;

            // If the button is used to restart the game, we change a few things about how it's handled.
            if (_storedChoice == "_RESTART")
            {
                //Get the button
                GetComponent<Button>().interactable = true;
                GetComponentInChildren<TMPro.TextMeshProUGUI>().text = "Start again?";
            }
            else
            {
                Event _event = null;
                if (!eventManagerReference.eventsDictionary.TryGetValue(_storedChoice, out _event))
                    Debug.LogError($"Referencing nonexistent event {_storedChoice}!");

                //Get the button
                string eventName = "<MISSING STRING TABLE ENTRY>";
                Button button = GetComponent<Button>();
                button.interactable = false;
                if (_event != null)
                {
                    // Check the stats, enable the button if better
                    if (eventManagerReference.characterStats > _event.requirements)
                        button.interactable = true;
                    GetComponentInChildren<TMPro.TextMeshProUGUI>().text = _storedChoice;
                }
                else
                {
                    GetComponentInChildren<TMPro.TextMeshProUGUI>().text = "<MISSING STRING TABLE ENTRY>";
                }
            }
        }
    }
    private string _storedChoice;
    public EventManager eventManagerReference;

    public void OnClick()
    {
        if (storedChoice == "_RESTART")
            eventManagerReference.StartNewStory();
        else
            eventManagerReference.SelectChoice(storedChoice);
    }
}
