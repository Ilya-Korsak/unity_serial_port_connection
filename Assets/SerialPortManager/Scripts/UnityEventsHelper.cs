using System.Reflection;
using UnityEngine.Events;

static class UnityEventsHelper 
{
    public static int GetListenerNumber(this UnityEventBase unityEvent)
    {
        var field = typeof(UnityEventBase).GetField("m_Calls", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
        var invokeCallList = field.GetValue(unityEvent);
        var property = invokeCallList.GetType().GetProperty("Count");
        return (int)property.GetValue(invokeCallList);
    }
}
