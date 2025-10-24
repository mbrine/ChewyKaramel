using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CharacterCustomization : MonoBehaviour
{
    public TMPro.TMP_InputField m_characterName;
    public TMPro.TMP_Dropdown m_characterGender;

    public EventManager eventManagerReference;

    public void ApplyCustomizations()
    {
        Blackboard.AddObject("Player", m_characterName.text);
        gameObject.SetActive(false);

        // Init the filesystem
        eventManagerReference.InitFileSystem();

        // Change to the "Root" event
        // We can do random logic here as well if we want different start states, all up to the writers ofc
        eventManagerReference.MoveToEvent("Root");

        // Reset Character Stats
        eventManagerReference.characterStats = new CharacterStats();
        eventManagerReference.statsEditor.stats = eventManagerReference.characterStats;
        eventManagerReference.statsEditor.editable = false;
    }
}
