using System.Collections.Generic;
using System.Reflection;
using Unity.VisualScripting;
using UnityEngine;

[System.Serializable]
public class DFSWordCountResult
{
	public int wordCount;
	public List<string> route;
	public string story;
}
public class DFSWordCountResultSummary
{
	public float averageWordCount;
	public float averageBranchDepth;
	public int longestStoryLength;
	public int shortestStoryLength;
	public int numStoryPathsUnder1000;
	public int numStoryPathsOver1000;

	public int numRandomCommands;
}