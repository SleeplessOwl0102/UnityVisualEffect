using UnityEngine;

namespace Ren.VFX
{
    [ExecuteAlways]
    public class ControlPointSetter : MonoBehaviour
    {
        public Transform target;
        private void Update()
        {
            if (target != null)
                Shader.SetGlobalFloat("_threshold", target.position.y);
        }
    }
}