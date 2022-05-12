using System.Collections.Generic;
using UnityEngine;

namespace BlackJack.ProjectH.EditorRuntime.SceneValidator
{

    [ExecuteAlways]
    public class SceneVarifier : MonoBehaviour
    {
        [SerializeField]
        private SceneValidateConfig m_sceneValidateConfig;

        [SerializeField]
        private List<LogEntry> m_sceneVerifyLogList = new List<LogEntry>();

        public List<LogEntry> SceneVerifyLogList { get => m_sceneVerifyLogList; }
        public SceneValidateConfig SceneValidateConfig { get => m_sceneValidateConfig; }
    }
}