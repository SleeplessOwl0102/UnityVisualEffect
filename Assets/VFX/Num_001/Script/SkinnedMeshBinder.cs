using UnityEngine;
using UnityEngine.VFX;
using UnityEngine.VFX.Utility;

namespace Ren.VFX
{
    [AddComponentMenu("VFX/Property Binders/Smrvfx/Skinned Mesh Binder")]
    [VFXBinder("Smrvfx/Skinned Mesh")]
    sealed class VFXSkinnedMeshBinder : VFXBinderBase
    {
        public string PositionMapProperty {
            get => (string)_positionMapProperty;
            set => _positionMapProperty = value;
        }

        [VFXPropertyBinding("UnityEngine.Texture2D"), SerializeField]
        ExposedProperty _positionMapProperty = "PositionMap";


        public SkinnedMeshBaker Target = null;

        public override bool IsValid(VisualEffect component)
        { 
          return Target != null && component.HasTexture(_positionMapProperty);
        }

        public override void UpdateBinding(VisualEffect component)
        {
            component.SetTexture(_positionMapProperty, Target.PositionMap);
        }

        public override string ToString()
        { 
          return $"Skinned Mesh : '{_positionMapProperty}' -> {Target?.name ?? "(null)"}";
        }
    }
}
