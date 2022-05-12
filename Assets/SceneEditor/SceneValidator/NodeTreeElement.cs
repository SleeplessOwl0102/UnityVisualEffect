using System;
using System.Collections.Generic;
using TreeViewFramework;
using UnityEngine;

namespace BlackJack.ProjectH.EditorRuntime.SceneValidator
{
    [Serializable]
    public class NodeTreeElement : TreeElement
    {
        public string m_path;

        public string m_preName;

        [HideInInspector]
        public string m_desc;

        [LabelOverride("只存在于Edior下的节点")]
        public bool m_isEditorOnly = false;

        [LabelOverride("非必需的节点")]
        public bool m_isOptionNode = false;

        [LabelOverride("必须在原点不旋转缩放")]
        public bool m_isRootNode = true;


        [SerializeReference]
        [ReferenceTypeSelector(typeof(SceneNodeRuleBase))]
        [LabelOverride("节点自己要符合的规则")]
        public List<SceneNodeRuleBase> m_selfRule = new List<SceneNodeRuleBase>();

        [SerializeReference]
        [ReferenceTypeSelector(typeof(SceneNodeRuleBase))]
        [LabelOverride("所有子节点和自己要符合的规则")]
        public List<SceneNodeRuleBase> m_allChildsRule = new List<SceneNodeRuleBase>();

        public NodeTreeElement(string name, int depth, int id) : base(name, depth, id)
        {
            m_isEditorOnly=false;
            m_isOptionNode = false;
            m_isRootNode = true;
        }
    }

}