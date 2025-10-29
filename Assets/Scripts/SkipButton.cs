using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkipButton : MonoBehaviour
{
    public TextDisplayer textDisplayer;
    public EventManager eventManager;
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            SkipLine();
    }
    public void SkipLine()
    {
        textDisplayer.SkipLine();
        eventManager.SkipLine();

	}
}
