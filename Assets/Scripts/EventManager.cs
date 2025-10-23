using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public struct Outcome
{
    // The texts to display. Keeps showing one line at a time until the last is reached, then shows the choices.
    public string displayText;
    public CharacterStats modifyAttributes;

    // IDs of available choices
    public List<string> choices;
}

[System.Serializable]
public class Event
{
    // The ID of the event
    public string id;

    // THe required stats to even make the first choice
    public CharacterStats requirements;

    // I got no idea what Ryan wants this to do
    public CharacterStats successAttributes;

    // If the requirements are met
    public Outcome onSuccess;

    // If below the requirements
    public Outcome onFailure;
}
[System.Serializable]
public struct EventContainer
{
    public List<Event> events;
    public EventContainer(int i = 0)
    {
        events = new List<Event>();
    }
}

public class EventManager : MonoBehaviour
{
    // Unity side references, etc
    public GameObject textObjectReference;
    public GameObject choiceButtonReference;
	public Scrollbar sliderReference;
    public ContentSizeFitter contentSizeFitter;

	public Transform textWindow;
    public Transform choicesWindow;

    public Dictionary<string, Event> eventsDictionary;

    public CharacterStats characterStats;

    // Current event/outcome details
    public string currentEventID;
    public Event currentEvent;
    public Outcome currentOutcome;

    // Stats
    public StatsEditor statsEditor;

    // THe FileStream for outputting the story.
    private FileStream storyFileStream;

    private int wordCount;

    public TextDisplayer textDisplayer;
    private bool wasStoryUpdated;

    private void Start()
	{
        LoadEvents();
        StartNewStory();
    }

    private void Update()
    {
        if (textDisplayer.textDisplaying)
        {
            contentSizeFitter.enabled = false;
        }
    }

    private void LateUpdate()
    {
        if (textDisplayer.textDisplaying)
        {
            contentSizeFitter.enabled = true;
            sliderReference.value = 0;
        }
        //if (wasStoryUpdated && !textDisplayer.textDisplaying)
        //{
        //    wasStoryUpdated = false;
        //}
    }

    // Stop speech when destroyed
    private void OnDestroy()
    {
        TextToSpeech.StopSpeech();
    }

    // Stop speech when application quits
    private void OnApplicationQuit()
    {
        TextToSpeech.StopSpeech();
    }

    public void SaveEvents()
    {
        // Make a new container
        EventContainer container = new EventContainer(0);

        // Add all the events from the dictionaries
        foreach(var kvp in eventsDictionary)
        {
            container.events.Add(kvp.Value);
        }

        // Create the json string
        string json = JsonUtility.ToJson(container, true);

        // Open file
        FileStream fileStream = new FileStream(Application.streamingAssetsPath+ "/Events.json",FileMode.OpenOrCreate);

        // Write the data!
        using (StreamWriter writer = new StreamWriter(fileStream))
        {
			writer.Write(json);
        }

        // Close the file when we're done
        fileStream.Close();
    }

    public void LoadEvents()
    {
        eventsDictionary = new Dictionary<string, Event>();

		// Open file
		FileStream fileStream = new FileStream(Application.streamingAssetsPath + "/Events.json", FileMode.OpenOrCreate);

		// Read the data!
		using (StreamReader reader = new StreamReader(fileStream))
		{
			string json = reader.ReadToEnd();

            // Get the EventContainer object
            EventContainer container = JsonUtility.FromJson<EventContainer>(json);

            // Add the events to the dictionary
            foreach(Event e in container.events)
            {
                eventsDictionary.Add(e.id, e);
            }
		}

		// Close the file when we're done
		fileStream.Close();

	}

    public void StartNewStory()
    {
        // Close the fileStream if it's open
        storyFileStream?.Close();

        // Directory sanity
        if (!Directory.Exists(Application.persistentDataPath + "/Stories"))
            Directory.CreateDirectory(Application.persistentDataPath + "/Stories");

        // Open a new FileStream
        storyFileStream = new FileStream($"{Application.persistentDataPath}/Stories/STORY.txt", FileMode.OpenOrCreate);

        // Reset the text and choice windows
        textDisplayer.ResetText();
        foreach (Transform child in choicesWindow)
        {
            Destroy(child.gameObject);
        }

        // Change to the "Root" event
        // We can do random logic here as well if we want different start states, all up to the writers ofc
        MoveToEvent("Root");

        // Reset Character Stats
        characterStats = new CharacterStats();
        statsEditor.stats = characterStats;
        statsEditor.editable = true;
    }

	// Writes lines to the file.
	public void WriteToFile(string line, bool updateWordCount = true)
    {
        // Allow blank lines to be ignored. Prevents a ton of newline spam
        if (line.Length == 0)
            return;

        // Write da ting
        using (StreamWriter writer = new StreamWriter(storyFileStream))
        {
            writer.Write(line);
        }

        // Update internal wordcount
        if (updateWordCount)
        {
            char[] delimiters = new char[] { ' ', '\r', '\n' };
            wordCount += line.Split(delimiters, StringSplitOptions.RemoveEmptyEntries).Length;
        }
    }

    public void SelectChoice(string choiceID)
    {
        MoveToEvent(choiceID);
    }

    public void MoveToEvent(string eventID)
    {
        if(!eventsDictionary.ContainsKey(eventID))
        {
            Debug.LogError($"ERROR: Event with ID {eventID} does not exist!!!");
            return;
        }
        
        // Remove all choices
		foreach (Transform child in choicesWindow)
		{
			Destroy(child.gameObject);
		}

		// Specifically for GameOver ID, we treat it as if we end the game ig, idk what else we want to do here
		if (eventID == "GameOver")
        {
            //Do things
        }

        // Change to the new event
        currentEventID = eventID;
        currentEvent = eventsDictionary[eventID];
        if (characterStats > currentEvent.requirements)
        {
            currentOutcome = currentEvent.onSuccess;
        }
        else
        {
            currentOutcome = currentEvent.onFailure;
        }

        if (statsEditor.editable)
            characterStats = statsEditor.stats;
        // Update character stats
        characterStats += currentOutcome.modifyAttributes;

        statsEditor.stats = characterStats;
        statsEditor.editable = false;
        statsEditor.UpdateValues();

        // Update the story display
        UpdateStoryDisplay();
    }

    public void UpdateStoryDisplay()
    {
        // Create a new text object and set the text, then set parent
        //GameObject newText = Instantiate(textObjectReference,textWindow);
        //newText.GetComponent<TextDisplayer>().text = currentOutcome.displayText;
        //newText.GetComponent<TextDisplayer>().eventManager = this;
        //newText.transform.SetParent(textWindow);
        //
        //textDisplayer = newText.GetComponent<TextDisplayer>();
        textDisplayer.text += currentOutcome.displayText+"\n\n";
        TextToSpeech.SpeechText(currentOutcome.displayText);
        wasStoryUpdated = true;
    }
    
    public void ShowChoices()
    {
        // If no choices, show a "Restart game?" button instead
        if(currentOutcome.choices.Count==0)
        {
			// Instantiate the choice button
			GameObject newChoiceObject = Instantiate(choiceButtonReference);

			ChoiceContainer choiceContainer = newChoiceObject.GetComponent<ChoiceContainer>();
			choiceContainer.eventManagerReference = this;
			choiceContainer.storedChoice = "_RESTART";

			newChoiceObject.transform.SetParent(choicesWindow);

			return;
        }

        // Create the choice buttons
        foreach (string choiceID in currentOutcome.choices)
        {
            // Instantiate the choice button
            GameObject newChoiceObject = Instantiate(choiceButtonReference);

            ChoiceContainer choiceContainer = newChoiceObject.GetComponent<ChoiceContainer>();
            choiceContainer.eventManagerReference = this;
            choiceContainer.storedChoice = choiceID;

            newChoiceObject.transform.SetParent(choicesWindow);
        }

    }
}