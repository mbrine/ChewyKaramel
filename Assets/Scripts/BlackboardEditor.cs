using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class BlackboardEditor : MonoBehaviour
{
	public string blackboardTargetValue;
	public TMPro.TMP_InputField inputField;

	private void OnEnable()
	{
		UpdateValue();
	}
	public void UpdateValue()
	{
		if (Blackboard.HasObject(blackboardTargetValue))
		{
			inputField.text = Blackboard.GetObject(blackboardTargetValue);
		}
	}
	public void SetValue()
	{
		Blackboard.AddObject(blackboardTargetValue, inputField.text);
	}
}
