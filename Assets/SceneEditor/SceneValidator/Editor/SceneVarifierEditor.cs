
using BlackJack.ProjectH.EditorRuntime.SceneValidator;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace BlackJack.ProjectH.Editor.SceneValidator
{
    [CustomEditor(typeof(SceneVarifier))]
    public class SceneVarifierEditor : UnityEditor.Editor
    {
        private SceneVarifier m_target;

    }
}
