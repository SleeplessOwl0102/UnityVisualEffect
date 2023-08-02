
using UnityEngine;
using UnityEngine.VFX;

namespace Ren.VFX
{
    [RequireComponent(typeof(VisualEffect))]
    public class VFXEventHelper : MonoBehaviour
    {
        [HideInInspector]
        public VisualEffect target;

        public string[] EventName;

        private void OnValidate()
        {
            target = GetComponent<VisualEffect>();
        }

        public void SendEventToVisualEffect(string eventName)
        {
            if (target != null)
            {
                target.SendEvent(eventName);
            }
        }
    }
}

#if UNITY_EDITOR
namespace Ren.VFX.Editor
{
    using UnityEditor;
    [CustomEditor(typeof(VFXEventHelper))]
    public class VFXEventBinderEditor : Editor
    {
        VFXEventHelper instance;
        private void OnEnable()
        {
            instance = (VFXEventHelper)target;
        }
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            for (int i = 0; i < instance.EventName.Length; i++)
            {
                if (GUILayout.Button(instance.EventName[i]))
                {
                    instance.SendEventToVisualEffect(instance.EventName[i]);
                }
            }

            if (GUILayout.Button("ReInit"))
            {
                instance.target.Reinit();
            }
        }
    }
}
#endif