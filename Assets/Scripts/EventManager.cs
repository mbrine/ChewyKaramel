using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.VirtualTexturing;
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
    private bool debugging;

    public TextDisplayer textDisplayer;
    public GameObject characterCustomizationPanel;
    public TMPro.TextMeshProUGUI wordCountDisplay;

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
        wordCountDisplay.text = wordCount.ToString();
        if (Input.GetKey(KeyCode.LeftShift)&&Input.GetKey(KeyCode.RightShift)&&Input.GetKeyDown(KeyCode.D))
        {
            PerformDFSTest();
        }
    }

    private void LateUpdate()
    {
        if (textDisplayer.textDisplaying)
        {
            contentSizeFitter.enabled = true;
            sliderReference.value = 1-textDisplayer.textComponent.renderedHeight/textDisplayer.textComponent.preferredHeight;
        }
        else
        {
        }
    }

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
					UnityEngine.Debug.LogError("bro wtf?");
                }
            }

            // Close the file when we're done
            fileStream.Close();

        }
    }
    public void InitFileSystem()
    {
        // Open a new FileStream
        storyFileStream = new FileStream($"{Application.persistentDataPath}/Stories/STORY_{DateTime.Now.ToString("HH-mm-ss")}.txt", FileMode.OpenOrCreate);
        writer = new StreamWriter(storyFileStream);

    }
    public void StartNewStory()
    {
        // Close the fileStream if it's open
        storyFileStream?.Close();

        // Directory sanity
        if (!Directory.Exists(Application.persistentDataPath + "/Stories"))
            Directory.CreateDirectory(Application.persistentDataPath + "/Stories");

        // Reset the text and choice windows
        textDisplayer.ResetText();
        foreach (Transform child in choicesWindow)
        {
            Destroy(child.gameObject);
        }

        // Reload the blackboard
        BlackboardLoader.LoadBlackboard();

        // Enable character customization
        characterCustomizationPanel.SetActive(true);

        wordCount = 0;

        characterStats = new CharacterStats();
        statsEditor.stats = characterStats;
    }

    // Writes lines to the file.
    public void WriteToFile(string line, bool updateWordCount = true)
    {
        if (debugging)
            return;
        // Allow blank lines to be ignored. Prevents a ton of newline spam
        if (line.Length == 0)
            return;

        // Write da ting
        writer.Write(line);

        if(updateWordCount )
        UpdateWordCount(textDisplayer.text);
    }

    private void UpdateWordCount(string text)
    {
        // Update internal wordcount
        char[] delimiters = new char[] { ' ', '\r', '\n' };
        wordCount += text.Split(delimiters, StringSplitOptions.RemoveEmptyEntries).Length;
    }

	public void SelectChoice(string choiceID)
    {
        MoveToEvent(choiceID);
    }

    public void MoveToEvent(string eventID)
    {
        if(!eventsDictionary.ContainsKey(eventID))
        {
			UnityEngine.Debug.LogError($"ERROR: Event with ID {eventID} does not exist!!!");
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
            // Play safe, if the "fail" state is blank we ignore it
            if (currentEvent.onFailure.displayText != ""&&currentEvent.onFailure.displayText != "none")
                currentOutcome = currentEvent.onFailure;
            else
                currentOutcome = currentEvent.onSuccess;
        }

        if (statsEditor.editable)
            characterStats = statsEditor.stats;
        // Update character stats
        characterStats += currentOutcome.modifyAttributes;

        statsEditor.stats = characterStats;
        statsEditor.editable = false;
        //statsEditor.UpdateValues();

        // Update the story display
        UpdateStoryDisplay();
    }

    // Applies filters to the text, such as pulling blackboard values etc.
    // If debug, all blackboard text is replaced with 1 word and all _Random uses the first option
    public string FilteredText(string text, bool debug = false)
    {
        // Base case
        if (!text.Contains("{"))
            return text;

        string output = "";
        while(text.Contains("{"))
        {
            // Add to the output
            output += text.Substring(0, text.IndexOf("{"));

            // Shift the text forward to the start of the command
            text = text.Substring(text.IndexOf("{")+1);

            // Get the command string
            int depth = 0;
            int commandEndIndex = 0;
            for (; commandEndIndex < text.Length; commandEndIndex++)
            {
                if (text[commandEndIndex] == '{')
                    ++depth;
                if (text[commandEndIndex] == '}')
                {
                    if (depth == 0)
                        break;
                    --depth;
                }
            }
            string inTextCommand = text.Substring(0,commandEndIndex);

            bool commandProcessed = true;
            // If the first character is _ that means this is a command.
            if (inTextCommand[0] == '_')
            {
                string command = inTextCommand.Substring(0, inTextCommand.IndexOf(':'));
                string commandBody = inTextCommand.Substring(inTextCommand.IndexOf(':') + 1);

                switch (command)
                {
                    case "_Random":
                        {
                            List<string> options = new List<string>();
                            depth = 0;
                            for (int commandBodyIndex = 0; commandBodyIndex <= commandBody.Length; commandBodyIndex++)
                            {
                                if (commandBodyIndex == commandBody.Length)
                                {
                                    options.Add(commandBody);
                                    break;
                                }
                                if (commandBody[commandBodyIndex] == '{')
                                    ++depth;
                                if (commandBody[commandBodyIndex] == '}')
                                {
                                    if (depth == 0)
                                        break;
                                    --depth;
                                }

                                if (commandBody[commandBodyIndex] == '|' && depth == 0)
                                {
                                    options.Add(commandBody.Substring(0, commandBodyIndex));
                                    commandBody = commandBody.Substring(commandBodyIndex + 1);
                                    commandBodyIndex = 0;
                                }
                            }
                            if (debug)
                                output += options[0];
                            else
                                output += options[UnityEngine.Random.Range(0, options.Count)];
                        }
                        break;
                    // Adds value to the specified attribute
                    case "_AddAttr":
                        {
                            if (debug)
                                break;
                            string[] attributes = commandBody.Split("|");
                            foreach (string attribute in attributes)
                            {
                                string attributeName = attribute.Split("+")[0];
                                FieldInfo field = characterStats.GetType().GetField(attributeName);
                                if (field == null)
                                {
                                    UnityEngine.Debug.LogWarning($"Attempting to perform AddAttr on nonexistent stat {attributeName}!");
                                    break;
                                }
                                field.SetValue(characterStats, int.Parse(attribute.Split("+")[1]) + (int)field.GetValue(characterStats));
                            }
                        }
                        break;
                }
                commandProcessed = true;
            }
            // If not a command, we attempt to pull from the blackboard.
            else
            {

                if (Blackboard.HasObject(inTextCommand))
                {
                    commandProcessed = true;
                    output += Blackboard.GetObject(inTextCommand);
                }
                else if (debug)
                {
                    output += "BBValue";
                }
                else
                {
                    // Error here because THERES NOTHING IN THE BLACKBOARD FOR THIS.
                    UnityEngine.Debug.LogError($"YOU ARE TRYING TO ACCESS A NONEXISTENT BLACKBOARD VARIABLE {inTextCommand} FOR GOD'S SAKE");
                }
            }
            if (!commandProcessed&&!debug)
            {
                output += $"<FAILED TEXT COMMAND: {{{inTextCommand}}}>";
            }

            // Shift the text forward to the start of the command
            if(commandEndIndex<text.Length)
            text = text.Substring(commandEndIndex + 1);
        }

        output += text;
        return FilteredText(output);
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

        statsEditor.stats = characterStats;
    }
    
    public void PerformDFSTest()
    {
        debugging = true;
		// Reference the root
		Event rootEvent = eventsDictionary["Root"];
        List<string> routes = new List<string>();

		// Directory sanity
		if (!Directory.Exists(Application.persistentDataPath + "/Debug"))
			Directory.CreateDirectory(Application.persistentDataPath + "/Debug");
        if (File.Exists($"{Application.persistentDataPath}/Debug/FullDFSTest.json"))
            File.Delete($"{Application.persistentDataPath}/Debug/FullDFSTest.json");

		// Open a new FileStream
		var debugFileStream = new FileStream($"{Application.persistentDataPath}/Debug/FullDFSTest.json", FileMode.OpenOrCreate);
		var debugWriter = new StreamWriter(debugFileStream);

        // Force reload the blackboard
        BlackboardLoader.LoadBlackboard(true);

        DFS(ref routes, "Root", rootEvent);

        foreach (string route in routes)
        {
            string[] fullPath = route.Split('\n');
            wordCount = 0;
            string currentStoryText = FilteredText(rootEvent.onSuccess.displayText,true);
            foreach(string eventID in fullPath)
            {
                Event _event = eventsDictionary[eventID];
                currentStoryText += FilteredText(_event.onSuccess.displayText,true);
            }

            UpdateWordCount(currentStoryText);

            DFSWordCountResult result = new DFSWordCountResult();
            result.wordCount = wordCount;
            result.route = new List<string>(fullPath);
            debugWriter.Write(JsonUtility.ToJson(result,true)+',');
        debugWriter.Flush();
        }

        // Restart the story
        StartNewStory();

        // Open the debug folder
		Process.Start(Application.persistentDataPath + "/Debug");
        debugWriter.Flush();
		debugFileStream?.Close();
        debugging = false;
	}

    public void DFS(ref List<string> routes, string currentRoute, Event currentEvent)
    {

        if (currentEvent == null)
        {
            routes.Add(currentRoute);
            return;
        }
        // Get a list of all available choices within this event
        List<string> availableChoiceIDs = new List<string>();
        availableChoiceIDs.AddRange(currentEvent.onSuccess.choices);
        availableChoiceIDs.AddRange(currentEvent.onFailure.choices);

        if(availableChoiceIDs.Count==0)
        {
            routes.Add(currentRoute);
            return;
        }

        foreach (string choiceID in availableChoiceIDs)
        {
            if (eventsDictionary.ContainsKey(choiceID))
            {
                DFS(ref routes, currentRoute+"\n"+choiceID, eventsDictionary[choiceID]);
            }
        }
    }
	public void ShowChoices()
    {
        // Don't show the choices if we're customizing
        if (characterCustomizationPanel.activeSelf)
            return;

        // If no choices, show a "Restart game?" button instead
        if(currentOutcome.choices.Count==0)
        {
			// Instantiate the choice button
			GameObject newChoiceObject = Instantiate(choiceButtonReference);

            // Start new story
			ChoiceContainer choiceContainer = newChoiceObject.GetComponent<ChoiceContainer>();
			choiceContainer.eventManagerReference = this;
			choiceContainer.storedChoice = "_RESTART";

			newChoiceObject.transform.SetParent(choicesWindow);
            
            // Story directory
            newChoiceObject = Instantiate(choiceButtonReference);

			choiceContainer = newChoiceObject.GetComponent<ChoiceContainer>();
			choiceContainer.eventManagerReference = this;
			choiceContainer.storedChoice = "_OPENDIRECTORY";

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

    public void SkipLine()
    {
		sliderReference.value = 0;
	}
}