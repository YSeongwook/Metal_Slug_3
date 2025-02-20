using System;
using System.Collections.Generic;
using UnityEngine.Events;

namespace _01.Scripts.Utils
{
    // 제네릭 UnityEvent 클래스 정의 (매개변수가 있는 이벤트용)
    [Serializable]
    public class GenericEvent<T> : UnityEvent<T> { }

    public class EventManager<E> where E : Enum
    {
        // 이벤트 이름과 해당 UnityEvent를 저장하는 딕셔너리
        private static readonly Dictionary<E, UnityEventBase> EventDictionary = new Dictionary<E, UnityEventBase>();
        // 스레드 안전성을 위한 객체
        private static readonly object LockObj = new object();

        // 이벤트 리스너를 추가하는 메서드 (매개변수 없는 리스너 추가)
        public static void StartListening(E eventName, UnityAction listener)
        {
            AddListener(eventName, listener);
        }

        // 이벤트 리스너를 추가하는 메서드 (매개변수 있는 리스너 추가)
        public static void StartListening<T>(E eventName, UnityAction<T> listener)
        {
            AddListener(eventName, listener);
        }

        // 이벤트 리스너를 제거하는 메서드 (매개변수 없는 리스너 제거)
        public static void StopListening(E eventName, UnityAction listener)
        {
            RemoveListener(eventName, listener);
        }

        // 이벤트 리스너를 제거하는 메서드 (매개변수 있는 리스너 제거)
        public static void StopListening<T>(E eventName, UnityAction<T> listener)
        {
            RemoveListener(eventName, listener);
        }

        // 매개변수가 없는 이벤트를 트리거하는 메서드
        public static void TriggerEvent(E eventName)
        {
            InvokeEvent(eventName);
        }

        // 매개변수가 있는 이벤트를 트리거하는 메서드
        public static void TriggerEvent<T>(E eventName, T parameter)
        {
            InvokeEvent(eventName, parameter);
        }

        // 이벤트가 존재하면 반환하고, 없으면 새로 생성하여 추가하는 메서드
        private static TEvent GetOrCreateEvent<TEvent>(E eventName) where TEvent : UnityEventBase, new()
        {
            if (!EventDictionary.TryGetValue(eventName, out var thisEvent))
            {
                thisEvent = new TEvent();
                EventDictionary.Add(eventName, thisEvent);
            }
            return thisEvent as TEvent;
        }

        // 특정 이벤트를 수동으로 삭제하는 메서드 (필요 시 호출)
        public static void ClearEvent(E eventName)
        {
            lock (LockObj)
            {
                if (EventDictionary.ContainsKey(eventName))
                {
                    EventDictionary.Remove(eventName);
                    DebugLogger.Log($"이벤트 {eventName}가 클리어되었습니다.");
                }
            }
        }

        // 모든 이벤트를 수동으로 삭제하는 메서드
        public static void ClearAllEvents()
        {
            lock (LockObj)
            {
                EventDictionary.Clear();
                DebugLogger.Log("모든 이벤트가 클리어되었습니다.");
            }
        }

        // 매개변수가 있는 리스너를 추가하는 내부 메서드
        private static void AddListener<T>(E eventName, UnityAction<T> listener)
        {
            lock (LockObj)
            {
                GenericEvent<T> genericEvent = GetOrCreateEvent<GenericEvent<T>>(eventName);
                genericEvent.AddListener(listener);
            }
        }

        // 매개변수가 없는 리스너를 추가하는 내부 메서드
        private static void AddListener(E eventName, UnityAction listener)
        {
            lock (LockObj)
            {
                UnityEvent unityEvent = GetOrCreateEvent<UnityEvent>(eventName);
                unityEvent.AddListener(listener);
            }
        }

        // 매개변수가 있는 리스너를 제거하는 내부 메서드
        private static void RemoveListener<T>(E eventName, UnityAction<T> listener)
        {
            lock (LockObj)
            {
                if (EventDictionary.TryGetValue(eventName, out var thisEvent) && thisEvent is GenericEvent<T> genericEvent)
                {
                    genericEvent.RemoveListener(listener);
                    // 자동 삭제 로직 제거: 런타임 리스너 수 확인이 불가능하여 위험할 수 있음.
                }
            }
        }

        // 매개변수가 없는 리스너를 제거하는 내부 메서드
        private static void RemoveListener(E eventName, UnityAction listener)
        {
            lock (LockObj)
            {
                if (EventDictionary.TryGetValue(eventName, out var thisEvent) && thisEvent is UnityEvent unityEvent)
                {
                    unityEvent.RemoveListener(listener);
                    // 자동 삭제 로직 제거: 런타임 리스너 수 확인이 불가능하여 위험할 수 있음.
                }
            }
        }

        // 매개변수가 있는 이벤트를 호출하는 내부 메서드
        private static void InvokeEvent<T>(E eventName, T parameter)
        {
            lock (LockObj)
            {
                try
                {
                    if (EventDictionary.TryGetValue(eventName, out var thisEvent) && thisEvent is GenericEvent<T> genericEvent)
                    {
                        genericEvent.Invoke(parameter);
                    }
                    else
                    {
                        DebugLogger.LogWarning($"이벤트 {eventName}가 존재하지 않거나 타입이 일치하지 않습니다.");
                    }
                }
                catch (Exception e)
                {
                    DebugLogger.LogError($"이벤트 {eventName} 호출 중 매개변수 ({parameter}) 관련 에러 발생: {e.Message}");
                }
            }
        }

        // 매개변수가 없는 이벤트를 호출하는 내부 메서드
        private static void InvokeEvent(E eventName)
        {
            lock (LockObj)
            {
                try
                {
                    if (EventDictionary.TryGetValue(eventName, out var thisEvent) && thisEvent is UnityEvent unityEvent)
                    {
                        unityEvent.Invoke();
                    }
                    else
                    {
                        DebugLogger.LogWarning($"이벤트 {eventName}가 존재하지 않거나 타입이 일치하지 않습니다.");
                    }
                }
                catch (Exception e)
                {
                    DebugLogger.LogError($"이벤트 {eventName} 호출 중 에러 발생: {e.Message}");
                }
            }
        }
    }
}
