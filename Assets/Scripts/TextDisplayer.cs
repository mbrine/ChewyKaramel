using UnityEngine;

public class TextDisplayer : MonoBehaviour
{
	// Text display speed in characters per second
	private static readonly float _textDisplaySpeed = 850.0f;
	private float progress = 0.0f;
	public bool textDisplaying { get; private set; } = true;
	public string text;

	private TMPro.TextMeshProUGUI textComponent;
	public EventManager eventManager;

	public void Start()
	{
		textComponent = GetComponent<TMPro.TextMeshProUGUI>();
	}
	public void Update()
	{
		if (textDisplaying)
		{
			progress += Time.deltaTime * _textDisplaySpeed;
			transform.position = Vector3.zero;
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

		textComponent.text = text.Substring(0, (int)progress);
	}

	public void ResetText()
	{
		text = "";
		progress = 0;
	}
}