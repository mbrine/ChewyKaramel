using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public struct Outcome
{
    public string displayText;
    public CharacterStats modifyAttributes;
}

public class Event
{
    public string id;
    public CharacterStats requirements;
    public CharacterStats successAttributes;
    public Outcome onSuccess;
    public Outcome onFailure;
}

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
    public Dictionary<string, Event> eventsDictionary;
    public string currentEventID;
    public Event currentEvent;

    public void SaveEvents()
    {
        // Make a new container
        EventContainer container = new EventContainer(0);

        // Add all the events from the dictionary
        foreach(var kvp in eventsDictionary)
        {
            container.events.Add(kvp.Value);
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
}