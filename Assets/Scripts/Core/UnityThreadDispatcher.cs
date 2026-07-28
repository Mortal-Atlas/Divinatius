using System;
using System.Collections.Generic;
using UnityEngine;

namespace Divinatius.Core
{
    /// <summary>
    /// Thread-safe dispatcher for executing actions on Unity's main thread.
    /// Essential for handling async network responses (Gemini API, ElevenLabs TTS/STT)
    /// and safely modifying Unity GameObjects/UI components.
    /// </summary>
    public class UnityThreadDispatcher : MonoBehaviour
    {
        private static readonly Queue<Action> ExecutionQueue = new Queue<Action>();
        private static UnityThreadDispatcher _instance;

        public static UnityThreadDispatcher Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("UnityThreadDispatcher");
                    _instance = go.AddComponent<UnityThreadDispatcher>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }

        private void Update()
        {
            lock (ExecutionQueue)
            {
                while (ExecutionQueue.Count > 0)
                {
                    try
                    {
                        ExecutionQueue.Dequeue()?.Invoke();
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"[UnityThreadDispatcher] Error executing action on main thread: {ex}");
                    }
                }
            }
        }

        /// <summary>
        /// Enqueues an action to be executed on Unity's main thread during the next Update.
        /// </summary>
        public static void Enqueue(Action action)
        {
            if (action == null) return;
            lock (ExecutionQueue)
            {
                ExecutionQueue.Enqueue(action);
            }
        }
    }
}
