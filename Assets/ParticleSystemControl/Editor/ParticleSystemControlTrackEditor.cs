// Timeline Particle Control Example
// https://github.com/keijiro/TimelineParticleControl



namespace Klak.Timeline
{
    using NoUse;
    using UnityEditor;
    [CustomEditor(typeof(ParticleSystemControlTrack)), CanEditMultipleObjects]
    class ParticleSystemControlTrackEditor : Editor
    {
        SerializedProperty _snapTarget;
        SerializedProperty _randomSeed;

        void OnEnable()
        {
            _snapTarget = serializedObject.FindProperty("template.snapTarget");
            _randomSeed = serializedObject.FindProperty("template.randomSeed");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(_snapTarget);
            EditorGUILayout.PropertyField(_randomSeed);
            serializedObject.ApplyModifiedProperties();
        }
    }

}
