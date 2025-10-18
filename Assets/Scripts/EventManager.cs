using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;

public struct Outcome
{
    // The texts to display. Keeps showing one line at a time until the last is reached, then shows the choices.
    public List<string> displayText;
    public CharacterStats modifyAttributes;

    // IDs of available choices
    public List<string> choices;
}

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

public class Choice
{
    // The ID of the choice
    public string id;

    // WHat event the story moves to from this choice.
    public string targetEventID;

    // What the button says. (ie. "Kick down the door")
    public string displayText;

    // What gets written to the storyline. (ie. "You kicked the door down.")
    public string outputText;
}

public struct EventContainer
{
    public List<Event> events;
    public List<Choice> choices;
    public EventContainer(int i = 0)
    {
        events = new List<Event>();
		choices = new List<Choice>();
    }
}

public class EventManager : MonoBehaviour
{
    // Unity side references, etc
    public GameObject textObjectReference;
    public GameObject choiceButtonReference;

    public Transform textWindow;
    public Transform choicesWindow;


    public Dictionary<string, Event> eventsDictionary;
    public Dictionary<string, Choice> choicesDictionary;

    public CharacterStats characterStats;

    // Current event/outcome details
    public string currentEventID;
    public Event currentEvent;
    public Outcome currentOutcome;

    // THe FileStream for outputting the story.
    private FileStream storyFileStream;

	// This is set to 0 whenever a new event is selected an incremented when UpdateStoryDisplay() is called
    // When it hits the amount of entries in the Outcome, the choices will be displayed.
	private int outcomeLineIndex;

    private int wordCount;

	private void Start()
	{
        LoadEvents();
        StartNewStory();
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
        foreach(var kvp in choicesDictionary)
        {
            container.choices.Add(kvp.Value);
        }

        // Create the json string
        string json = JsonUtility.ToJson(container, true);

        // Open file
        FileStream fileStream = new FileStream(Application.streamingAssetsPath+ "/events.json",FileMode.OpenOrCreate);

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
		FileStream fileStream = new FileStream(Application.streamingAssetsPath + "/events.json", FileMode.OpenOrCreate);

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
        storyFileStream.Close();

        // Open a new FileStream
        storyFileStream = new FileStream(Application.persistentDataPath + "/Stories/" + DateTime.Now.ToString(), FileMode.OpenOrCreate);

		// Reset the text and choice windows
		foreach (Transform child in textWindow)
		{
			Destroy(child.gameObject);
		}
		foreach (Transform child in choicesWindow)
		{
			Destroy(child.gameObject);
		}

		// Change to the "initial" event
		// We can do random logic here as well if we want different start states, all up to the writers ofc
		MoveToEvent("init");
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

    public void SelectChoice(Choice choice)
    {
        WriteToFile(choice.outputText);
        MoveToEvent(choice.targetEventID);
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

        // Reset the line index
        outcomeLineIndex = 0;
        
        // Update the story display
        UpdateStoryDisplay();
    }

    public void UpdateStoryDisplay()
    {
        if (outcomeLineIndex == currentOutcome.displayText.Count)
        {
            // Hide the "click anywhere button"


            // Show the available options
            foreach(string choiceID in currentOutcome.choices)
            {
                if(!choicesDictionary.ContainsKey(choiceID))
                {
                    Debug.LogError($"No choice with ID {choiceID} found!");
                    continue;
                }
                Choice choice = choicesDictionary[choiceID];

                // Instantiate the choice button
                GameObject newChoiceObject = Instantiate(choiceButtonReference);

                ChoiceContainer choiceContainer = newChoiceObject.GetComponent<ChoiceContainer>();
                choiceContainer.storedChoice = choice;
                choiceContainer.eventManagerReference = this;

                newChoiceObject.transform.SetParent(choicesWindow);
			}

            ++outcomeLineIndex;
        }
        else if (outcomeLineIndex < currentOutcome.displayText.Count)
        {
            // Create a new text object and set the text, then set parent
            GameObject newText = Instantiate(textObjectReference);
            newText.GetComponent<TMPro.TextMeshProUGUI>().text = currentOutcome.displayText[outcomeLineIndex];
            newText.transform.SetParent(textWindow);

            ++outcomeLineIndex;
        }
    }    
}