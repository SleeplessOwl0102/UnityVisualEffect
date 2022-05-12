using System;
using UnityEngine;


namespace BlackJack.ProjectH.EditorRuntime.SceneValidator.Rule
{
    [Serializable]
    public class SetLayer : SceneNodeRuleBase
    {
        [SerializeField]
        private SingleUnityLayer m_layer;
        protected override bool SceneNodeVerify(GameObject go)
        {
            if (go.layer != m_layer.LayerIndex)
            {
                LogAdd($"Layer 设置为 {LayerMask.LayerToName(m_layer.LayerIndex)}");
                go.layer = m_layer.LayerIndex;
            }
            return true;
        }
    }

}