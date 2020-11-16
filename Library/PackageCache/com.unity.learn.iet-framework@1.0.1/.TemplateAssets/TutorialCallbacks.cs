﻿using UnityEngine;
using Unity.InteractiveTutorials;

/// <summary>
/// Implement your Tutorial callbacks here.
/// </summary>
[CreateAssetMenu(fileName = DefaultFileName, menuName = "Tutorials/" + DefaultFileName + " Instance")]
public class TutorialCallbacks : ScriptableObject
{
    public const string DefaultFileName = "TutorialCallbacks";

    public static ScriptableObject CreateInstance()
    {
        return ScriptableObjectUtils.CreateAsset<TutorialCallbacks>(DefaultFileName);
    }

    // Example callback for basic UnityEvent
    public void ExampleMethod()
    {
        Debug.Log("ExampleMethod");
    }

    // Example callbacks for AtrbitraryCriterion's BoolCallback
    public bool DoesFooExist()
    {
        return GameObject.Find("Foo") != null;
    }

    public bool AutoComplete()
    {
        var foo = GameObject.Find("Foo");
        if (!foo)
            foo = new GameObject("Foo");
        return foo != null;
    }
}
