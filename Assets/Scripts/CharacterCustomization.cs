using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Unity.VisualScripting;
using UnityEngine;

public class CharacterCustomization : MonoBehaviour
{
    public TMPro.TMP_InputField m_characterName;
    public TMPro.TMP_Dropdown m_characterGender;

    public EventManager eventManagerReference;
    public GameObject advancedPane;

    List<BlackboardEditor> blackboardEditors;

	bool inited = false;

	public void ToggleAdvancedPanel()
    {
        advancedPane.SetActive(!advancedPane.activeSelf);
    }

	private void OnEnable()
	{
		foreach (BlackboardEditor bbEditor in GetComponentsInChildren<BlackboardEditor>(true))
		{
			bbEditor.UpdateValue();
		}
        //ToggleAdvancedPanel();
	}
    private void Update()
    {
        if (!inited)
        {
            foreach (BlackboardEditor bbEditor in GetComponentsInChildren<BlackboardEditor>(true))
            {
                bbEditor.UpdateValue();
            }
            blackboardEditors = new List<BlackboardEditor>();
            blackboardEditors.AddRange(GetComponentsInChildren<BlackboardEditor>(true));
            inited = true;
        }
    }
	public void ApplyCustomizations()
    {
        if (Input.GetKey(KeyCode.O))
            Blackboard.AddObject("Player", "Ωμεγα");
        else
            Blackboard.AddObject("Player", m_characterName.text);

        // Init the filesystem
        eventManagerReference.InitFileSystem();

        // Change to the "Root" event
        // We can do random logic here as well if we want different start states, all up to the writers ofc
        eventManagerReference.MoveToEvent("Root",false);

        // Reset Character Stats
        eventManagerReference.characterStats = new CharacterStats();
        eventManagerReference.statsEditor.stats = eventManagerReference.characterStats;
        eventManagerReference.statsEditor.editable = false;

        foreach(BlackboardEditor bbEditor in blackboardEditors)
        {
            bbEditor.SetValue();
        }


        gameObject.SetActive(false);
    }
}
