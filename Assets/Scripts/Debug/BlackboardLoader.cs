using UnityEngine;
using System.IO;
using NUnit.Framework;
using System.Collections.Generic;


public static class BlackboardLoader
{
    [System.Serializable]
    struct BlackboardObject
    {
        public string[] Blackboard;
    }
    private static string _filePath = $"{Application.streamingAssetsPath}/Blackboard.json";

    private static Dictionary<string, int> preRandomIndexes;
    public static void LoadBlackboard(bool debug = false)
    {
        // Wipe the blackboard
        Blackboard.Wipe();

        // Get the file
        FileStream fileStream = new FileStream(_filePath, FileMode.OpenOrCreate);

        preRandomIndexes = new Dictionary<string, int>();

        // Read the data!
        using (StreamReader reader = new StreamReader(fileStream))
        {
            string json = reader.ReadToEnd();

            // Get the BlackboardObject
            BlackboardObject blackboardObject = JsonUtility.FromJson<BlackboardObject>(json);

            foreach (string entry in blackboardObject.Blackboard)
            {
                // If the key has a # symbol, this means that it is dependent on a pre-randomised variable.
                string key = entry.Split(":")[0];
                string optionsSection = entry.Split(":")[1];
                string[] options = optionsSection.Split("|");

                if(debug)
                {
                    Blackboard.AddObject(key, options[0]);
                    continue;
                }

                if (entry.Contains("#"))
                {
                    string preRandomID = entry.Substring(0, entry.IndexOf("#"));

                    // If the dictionary doesn't have this ID, we set it to a random number
                    if (!preRandomIndexes.ContainsKey(preRandomID))
                        preRandomIndexes[preRandomID] = Random.Range(0, options.Length);

                    // Get the value from the dictionary and set the blackboard's string based on that
                    Blackboard.AddObject(key, options[preRandomIndexes[preRandomID]]);
                }
                else
                {
                    Blackboard.AddObject(key, options[Random.Range(0, options.Length)]);
                }
            }
        }
    }
}