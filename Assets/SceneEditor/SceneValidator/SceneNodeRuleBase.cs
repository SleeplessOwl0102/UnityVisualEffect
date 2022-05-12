using System;
using System.Collections.Generic;
using UnityEngine;

namespace BlackJack.ProjectH.EditorRuntime.SceneValidator
{

    /// <summary>
    /// 场景校验工具节点规则的基础类别
    /// </summary>
    [Serializable]
    public abstract class SceneNodeRuleBase
    {

        /// <summary>
        /// 实作验证内容
        /// </summary>
        /// <param name="go"></param>
        /// <returns></returns>
        protected abstract bool SceneNodeVerify(GameObject go);

        /// <summary>
        /// 开始验证节点是否符合规则
        /// </summary>
        /// <param name="go"></param>
        /// <param name="logs"></param>
        /// <returns></returns>
        public bool SceneNodeVerify(GameObject go, IList<LogEntry> logs)
        {
            m_isAllPass = true;
            m_gameObject = go;
            m_logs = logs;

            SceneNodeVerify(go);
            return m_isAllPass;
        }

        /// <summary>
        /// 新增修正或错误记录
        /// </summary>
        /// <param name="message"></param>
        /// <param name="logType"></param>
        protected void LogAdd(string message = null, string logType = LogEntry.LogTypeInfo)
        {
#if UNITY_EDITOR
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(m_gameObject.scene);
            //todo set prefab dirty
#endif

            if (m_isAllPass == true)
            {
                var str = $"规则 {this.GetType().Name} 对场景物件 {m_gameObject.name} 进行了修正";
                m_logs.Add(LogEntry.Create(LogEntry.LogTypeWarning, str, m_gameObject));
                m_isAllPass = false;
            }

            if (message != null)
            {
                m_logs.Add(LogEntry.Create(LogEntry.LogTypeInfo, message, m_gameObject));
            }
        }

        /// <summary>
        /// 记录是否完全符合规则
        /// </summary>
        private bool m_isAllPass;

        /// <summary>
        /// los容器缓存
        /// </summary>
        private IList<LogEntry> m_logs;

        /// <summary>
        /// 节点缓存
        /// </summary>
        private GameObject m_gameObject;
    }

}