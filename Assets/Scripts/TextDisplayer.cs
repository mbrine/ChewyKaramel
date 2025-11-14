using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TextDisplayer : MonoBehaviour
{
	// Text display speed in characters per second
	public float _textDisplaySpeed = 850.0f;
	public float progress { get; set; } = 0.0f;
	public bool textDisplaying { get; private set; } = true;
	public string text;

	public TMPro.TextMeshProUGUI textComponent;
	public EventManager eventManager;
	public Slider speedSlider;

	private bool caretVisible = false;
	private float caretBlinkTime = 0.5f;
	private float currCaretBlinkTime = 0.0f;

	public void Start()
	{
		textComponent = GetComponent<TMPro.TextMeshProUGUI>();
	}
	public void Update()
	{
		_textDisplaySpeed = speedSlider.value;
		string tmpTxt = "";

		if ((int)progress < text.Length)
		{
			if (text[(int)progress] == '<')
				progress = text.IndexOf('>', (int)progress) + 2;
		}
		if ((int)progress > 0)
			tmpTxt = text.Substring(0, (int)progress - 1);
		
		tmpTxt += $"{(caretVisible?"|":"_")}";//█';
		//tmpTxt += $"{(caretVisible?"|":"_")}<color=#00000000>";//█';
		//if ((int)progress > 0)
		//	tmpTxt += text.Substring((int)progress);

		textComponent.text = tmpTxt;//.Substring(0, (int)progress);
		//textComponent.maxVisibleCharacters = (int)progress;
		if (textDisplaying)
		{
			progress += Time.deltaTime * _textDisplaySpeed;
			//transform.localPosition = Vector3.zero;
		}
		else
		{
			currCaretBlinkTime -= Time.deltaTime;
			if (currCaretBlinkTime < 0.0f)
			{
				currCaretBlinkTime = caretBlinkTime;
				caretVisible = !caretVisible;
			}

        }
        if (progress >= text.Length && textDisplaying)
		{
			SkipLine();
		}
		else if (progress < text.Length)
		{
			textDisplaying = true;
		}

	}
	public void SkipLine()
	{
		if ((textDisplaying))
		{
			progress = text.Length;
			textDisplaying = false;
			eventManager.ShowChoices();
		}
	}
	public void ResetText()
	{
		text = "";
		progress = 0;
	}
}