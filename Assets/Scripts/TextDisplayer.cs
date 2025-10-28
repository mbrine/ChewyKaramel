using UnityEngine;
using UnityEngine.UI;

public class TextDisplayer : MonoBehaviour
{
	// Text display speed in characters per second
	public float _textDisplaySpeed = 850.0f;
	private float progress = 0.0f;
	public bool textDisplaying { get; private set; } = true;
	public string text;

	public TMPro.TextMeshProUGUI textComponent;
	public EventManager eventManager;
	public Slider speedSlider;

	public void Start()
	{
		textComponent = GetComponent<TMPro.TextMeshProUGUI>();
	}
	public void Update()
	{
		_textDisplaySpeed = speedSlider.value;
		textComponent.text = text;//.Substring(0, (int)progress);
		textComponent.maxVisibleCharacters = (int)progress;
		if (textDisplaying)
		{
			progress += Time.deltaTime * _textDisplaySpeed;
			//transform.localPosition = Vector3.zero;
		}
		if (progress >= text.Length && textDisplaying)
		{
			progress = text.Length;
			textDisplaying = false;
			eventManager.ShowChoices();
		}
		else if (progress < text.Length)
		{
			textDisplaying = true;
		}

	}

	public void ResetText()
	{
		text = "";
		progress = 0;
	}
}