using System;
using UnityEngine;

namespace BlackJack.ProjectH.EditorRuntime.SceneValidator
{
    public class LogEntry
    {
        public const string LogTypeInfo = "";

        public const string LogTypeWarning  = "";
        public const string LogTypeError = "";

        internal static LogEntry Create(object logTypeWarning, string str, GameObject m_gameObject)
        {
            throw new NotImplementedException();
        }

    }
}