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
	public CharacterStats stats { 
		get 
		{
			return _stats;
		}
		set
		{
			_stats=value;
			UpdateValues();
		}
	}
	private CharacterStats _stats;

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
	private static GameObject _sliderReference = null;

	public bool editable
	{
		get
		{
			return _editable;
		}
		set
		{
			_editable = value;
			foreach (TMPro.TMP_InputField statsInput in statsInputs)
			{
				statsInput.interactable = _editable;
			}
		}
	}
	public bool _editable;

	public List<TMPro.TMP_InputField> statsInputs;

	private void Awake()
	{
		FieldInfo[] fields = stats.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

		// Init all the sliders
		foreach (FieldInfo field in fields)
		{
			GameObject newInput = Instantiate(sliderReference);

			newInput.GetComponent<TMPro.TextMeshProUGUI>().text = field.Name.ToUpper();

			var inputField = newInput.GetComponentInChildren<TMPro.TMP_InputField>();

			inputField.contentType = TMP_InputField.ContentType.IntegerNumber;

			// Add the event listener to update the stats
			inputField.onValueChanged.AddListener(delegate { UpdateStats(); });

			// Add to statsinputs
			statsInputs.Add(inputField);

			newInput.transform.SetParent(transform);
		}
		stats = new CharacterStats();

		editable = _editable;
	}

	public void UpdateStats()
	{
		FieldInfo[] fields = _stats.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

		object boxed = _stats;
		// Update the values in the stats object
		for (int i = 0; i < fields.Length; i++)
		{
			fields[i].SetValue(boxed, int.Parse(statsInputs[i].text));
		}
		_stats = (CharacterStats)boxed;
	}

	public void UpdateValues()
	{
		FieldInfo[] fields = _stats.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

		// Update the values in the stats object
		for (int i = 0; i < fields.Length; i++)
		{
			statsInputs[i].text = fields[i].GetValue(_stats).ToString();
		}
	}
}
