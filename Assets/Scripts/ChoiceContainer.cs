using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
            else if (_storedChoice == "_OPENDIRECTORY")
            {
                //Get the button
                GetComponent<Button>().interactable = true;
                GetComponentInChildren<TMPro.TextMeshProUGUI>().text = "Open story directory";
            }
            else
            {
                Event _event = null;
                if (!eventManagerReference.eventsDictionary.TryGetValue(_storedChoice, out _event))
                    UnityEngine.Debug.LogError($"Referencing nonexistent event {_storedChoice}!");
                
                //Get the button
                Button button = GetComponent<Button>();
                button.interactable = false;
                if (_event != null)
                {
                    // Check the stats, enable the button if better
                    if (eventManagerReference.characterStats > _event.requirements)
                        button.interactable = true;
                    GetComponentInChildren<TMPro.TextMeshProUGUI>().text =$"<color={ColorCodes.goldHighlight}>{eventManagerReference.FilteredText(_storedChoice)}</color>";
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
        else if (storedChoice == "_OPENDIRECTORY")
			Process.Start(Application.persistentDataPath + "/Stories");
		else
            eventManagerReference.SelectChoice(storedChoice);
    }
}
