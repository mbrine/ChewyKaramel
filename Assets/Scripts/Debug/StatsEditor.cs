using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class StatsEditor : MonoBehaviour
{
	public CharacterStats stats { get; private set; }

	private static GameObject sliderReference
	{
		get
		{
			// Auto-assign to _sliderReference if it's null
			if (_sliderReference == null)
				_sliderReference = Resources.Load<GameObject>("Prefabs/StatsSlider");
			return _sliderReference;
		}
	}
	private static GameObject _sliderReference;

	public List<TMPro.TMP_InputField> statsInputs;

	private void Awake()
	{
		FieldInfo[] fields = stats.GetType().GetFields();

		// Init all the sliders
		foreach (FieldInfo field in fields)
		{
			GameObject newInput = Instantiate(sliderReference);
			var inputField = newInput.GetComponent<TMPro.TMP_InputField>();

			inputField.contentType = TMP_InputField.ContentType.IntegerNumber;

			// Add the event listener to update the stats
			inputField.onValueChanged.AddListener(delegate { UpdateStats(); });

			// Add to statsinputs
			statsInputs.Add(inputField);

			newInput.transform.parent = transform;
		}
	}

	void UpdateStats()
	{
		FieldInfo[] fields = stats.GetType().GetFields();

		// Update the values in the stats object
		for (int i = 0; i < fields.Length; i++)
		{
			fields[i].SetValue(stats, int.Parse(statsInputs[i].text));
		}
	}
}
