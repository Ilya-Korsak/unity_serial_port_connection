using UnityEngine.Events;

[System.Serializable]
public class ParserElement : InspectorElement
{
    public UnityEvent<string> Action;
    public bool isSubstring;
}