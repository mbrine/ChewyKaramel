using System.Collections.Generic;
using System.Reflection;
using Unity.VisualScripting;
using UnityEngine;

[System.Serializable]
public class DFSWordCountResult
{
	public int wordCount;
	public List<string> route;
}