using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    // References
    public TextDisplayer textDisplayer;
    public UnityEngine.UI.Slider volumeSlider;
    private AudioSource audioSource;

    // Vars
    public float pitchRange = 0.01f;
    public float volumeRange = 0.01f;
    public List<AudioClip> scribbleSounds;
    public float frequencyMultiplier;
    public float randomFactor = 0.5f;

    // Private vars
    //private float audioPlayCooldown
    private float previousProgress;

	private void Awake()
	{
		audioSource = GetComponent<AudioSource>();
	}

	// Update is called once per frame
	void Update()
    {
        if (textDisplayer.textDisplaying)
        {
            float diff = textDisplayer.progress - previousProgress;
            if (!audioSource.isPlaying)
            {
                for (; diff > 0.0f; diff -= 1.0f)
                {
                    if (Random.Range(0.0f, 1.0f) < randomFactor)
                    {
                        audioSource.pitch = Random.Range(1.0f- pitchRange, 1.0f+ pitchRange);
                        audioSource.PlayOneShot(scribbleSounds[Random.Range(0, scribbleSounds.Count)], Random.Range(1.0f - volumeRange, 1.0f + volumeRange)*volumeSlider.value);
                    }
                }
            }
            previousProgress = Mathf.Floor(textDisplayer.progress);
        }
    }
}
