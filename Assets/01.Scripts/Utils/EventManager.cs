using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using System;
using EnumTypes;

namespace EventLibrary
{
    [Serializable]
    public class TransformEvent : UnityEvent<Transform> { }

    [Serializable]
    public class FloatEvent : UnityEvent<float> { }

    public class EventManager
    {
        private static Dictionary<GlobalEvents, UnityEvent> eventDictionary = new Dictionary<GlobalEvents, UnityEvent>();
        private static Dictionary<GlobalEvents, TransformEvent> transformEventDictionary = new Dictionary<GlobalEvents, TransformEvent>();
        private static Dictionary<GlobalEvents, FloatEvent> floatEventDictionary = new Dictionary<GlobalEvents, FloatEvent>();

        public static void StartListening(GlobalEvents eventName, UnityAction listener)
        {
            UnityEvent thisEvent = null;
            if (eventDictionary.TryGetValue(eventName, out thisEvent))
            {
                thisEvent.AddListener(listener);
            }
            else
            {
                thisEvent = new UnityEvent();
                thisEvent.AddListener(listener);
                eventDictionary.Add(eventName, thisEvent);
            }
        }

        public static void StartListening(GlobalEvents eventName, UnityAction<Transform> listener)
        {
            TransformEvent thisEvent = null;
            if (transformEventDictionary.TryGetValue(eventName, out thisEvent))
            {
                thisEvent.AddListener(listener);
            }
            else
            {
                thisEvent = new TransformEvent();
                thisEvent.AddListener(listener);
                transformEventDictionary.Add(eventName, thisEvent);
            }
        }

        public static void StartListening(GlobalEvents eventName, UnityAction<float> listener)
        {
            FloatEvent thisEvent = null;
            if (floatEventDictionary.TryGetValue(eventName, out thisEvent))
            {
                thisEvent.AddListener(listener);
            }
            else
            {
                thisEvent = new FloatEvent();
                thisEvent.AddListener(listener);
                floatEventDictionary.Add(eventName, thisEvent);
            }
        }

        public static void StopListening(GlobalEvents eventName, UnityAction listener)
        {
            UnityEvent thisEvent = null;
            if (eventDictionary.TryGetValue(eventName, out thisEvent))
            {
                thisEvent.RemoveListener(listener);
            }
        }

        public static void TriggerEvent(GlobalEvents eventName)
        {
            UnityEvent thisEvent = null;
            if (eventDictionary.TryGetValue(eventName, out thisEvent))
            {
                thisEvent.Invoke();
            }
        }

        public static void TriggerEvent(GlobalEvents eventName, Transform transform)
        {
            TransformEvent thisEvent = null;
            if (transformEventDictionary.TryGetValue(eventName, out thisEvent))
            {
                thisEvent.Invoke(transform);
            }
        }

        public static void TriggerEvent(GlobalEvents eventName, float val)
        {
            FloatEvent thisEvent = null;
            if (floatEventDictionary.TryGetValue(eventName, out thisEvent))
            {
                thisEvent.Invoke(val);
            }
        }
    }
}