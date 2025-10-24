using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
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
    private StreamWriter writer;

    private int wordCount;

    public TextDisplayer textDisplayer;

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

	//public void SaveEvents()
    //{
    //    // Add all the events from the dictionaries
    //    foreach(var kvp in eventsDictionary)
    //    {
    //        container.events.Add(kvp.Value);
    //    }
    //
    //    // Create the json string
    //    string json = JsonUtility.ToJson(container, true);
    //
    //    // Open file
    //    FileStream fileStream = new FileStream(Application.streamingAssetsPath+ "/Events.json",FileMode.OpenOrCreate);
    //
    //    // Write the data!
    //    using (StreamWriter writer = new StreamWriter(fileStream))
    //    {
	//		writer.Write(json);
    //    }
    //
    //    // Close the file when we're done
    //    fileStream.Close();
    //}

    public void LoadEvents()
    {
        eventsDictionary = new Dictionary<string, Event>();

        //List<string> eventIDs = new List<string>();
        List<string> eventIDs = new List<string>(Directory.GetFiles(Application.streamingAssetsPath + "/Events").Where(f=>!f.EndsWith(".meta") ));
        //eventIDs.AddRange());
        
        // Open file
        foreach (var eventPath in eventIDs)
        {
            FileStream fileStream = new FileStream(eventPath, FileMode.OpenOrCreate);

            // Read the data!
            using (StreamReader reader = new StreamReader(fileStream))
            {
                string json = reader.ReadToEnd();

                // Get the EventContainer object
                Event newEvent = JsonUtility.FromJson<Event>(json);

                // Add the events to the dictionary
                if (newEvent != null)
                {
                    if (eventsDictionary.ContainsKey(newEvent.id))
                        eventsDictionary[newEvent.id] = newEvent;
                    else
                        eventsDictionary.Add(newEvent.id, newEvent);
                }
                else
                {
                    Debug.LogError("bro wtf?");
                }
            }

            // Close the file when we're done
            fileStream.Close();

        }
    }

    public void StartNewStory()
    {
        // Close the fileStream if it's open
        storyFileStream?.Close();

        // Directory sanity
        if (!Directory.Exists(Application.persistentDataPath + "/Stories"))
            Directory.CreateDirectory(Application.persistentDataPath + "/Stories");

        // Open a new FileStream
        storyFileStream = new FileStream($"{Application.persistentDataPath}/Stories/STORY_{DateTime.Now.ToString("HH-mm-ss")}.txt", FileMode.OpenOrCreate);
        writer = new StreamWriter(storyFileStream);

        // Reset the text and choice windows
        textDisplayer.ResetText();
        foreach (Transform child in choicesWindow)
        {
            Destroy(child.gameObject);
        }

        // Reload the blackboard
        BlackboardLoader.LoadBlackboard();

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
        writer.Write(line);

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
    // Applies filters to the text, such as pulling blackboard values etc.
    private string FilteredText(string text)
    {
        string output = "";
        while(text.Contains("{"))
        {
            // TMP
            break;
            output += text.Substring(0, text.IndexOf("{"));
            text = text.Substring(text.IndexOf("{"));
        }
        // TMP
        output = text;
        return output;
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
        string newText = FilteredText(currentOutcome.displayText) + "\n\n";
        textDisplayer.text += newText;
        WriteToFile(newText);
        writer.Flush();
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

            // Close the story file stream
            string wordcountText = $"\nTHE END\n\nWord Count: {wordCount.ToString()}";
            writer.Write(wordcountText);
            writer.Flush();
            storyFileStream?.Close();
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