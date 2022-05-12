//#undef UNITY_EDITOR

using System.Collections.Generic;
using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class LabelOverride : PropertyAttribute
{
    public string label;
    public LabelOverride(string label)
    {
        this.label = label;
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(LabelOverride))]
    public class ThisPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            try
            {
                var propertyAttribute = this.attribute as LabelOverride;
                label.text = propertyAttribute.label;
                EditorGUI.PropertyField(position, property, label);
            }
            catch (System.Exception ex) { Debug.LogException(ex); }
        }

    }
#endif
}

[System.Serializable]
public class SingleUnityLayer
{
    [SerializeField]
    private int m_LayerIndex = 0;
    public int LayerIndex
    {
        get { return m_LayerIndex; }
    }

    public void Set(int _layerIndex)
    {
        if (_layerIndex > 0 && _layerIndex < 32)
        {
            m_LayerIndex = _layerIndex;
        }
    }

    public int Mask
    {
        get { return 1 << m_LayerIndex; }
    }
#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(SingleUnityLayer))]
    public class SingleUnityLayerPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect _position, SerializedProperty _property, GUIContent _label)
        {
            EditorGUI.BeginProperty(_position, GUIContent.none, _property);
            SerializedProperty layerIndex = _property.FindPropertyRelative("m_LayerIndex");
            _position = EditorGUI.PrefixLabel(_position, GUIUtility.GetControlID(FocusType.Passive), _label);
            if (layerIndex != null)
            {
                layerIndex.intValue = EditorGUI.LayerField(_position, layerIndex.intValue);
            }
            EditorGUI.EndProperty();
        }
    }
#endif
}


[AttributeUsage(AttributeTargets.Field)]
public class ReferenceTypeSelectorAttribute : PropertyAttribute
{
    public bool isAbstractBase { get; private set; }
    public bool isValidate { get; private set; }
    public Type baseType { get; private set; }
    public IEnumerable<Type> subTypes { get; private set; }
    string filterMethod;

    public ReferenceTypeSelectorAttribute(Type baseType)
    {
        this.baseType = baseType;
        subTypes = baseType.Assembly.GetTypes().Where(t => baseType.IsAssignableFrom(t) && t != baseType && !t.IsAbstract && !t.IsGenericTypeDefinition);
        isAbstractBase = baseType.IsAbstract;
        isValidate = true;
    }

    public ReferenceTypeSelectorAttribute(Type baseType, string filterMethod)
    {
        this.baseType = baseType;
        this.filterMethod = filterMethod;
        isAbstractBase = baseType.IsAbstract;
    }

#if UNITY_EDITOR
    public void GetSubTypes(SerializedProperty property)
    {
        if (subTypes != null)
        {
            return;
        }

        if (string.IsNullOrEmpty(filterMethod))
        {
            isValidate = false;
            subTypes = Enumerable.Empty<Type>();
            return;
        }

        var obj = property.serializedObject.targetObject as object;
        var objType = obj.GetType();
        var filter = objType.GetMethod(filterMethod, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);

        if (filter != null && filter.ReturnParameter.ParameterType == typeof(IEnumerable<System.Type>) && filter.GetParameters().Length == 0)
        {
            isValidate = true;
            subTypes = filter.Invoke(null, null) as IEnumerable<System.Type>;
            subTypes = subTypes.Where(t => baseType.IsAssignableFrom(t) && t != baseType && !t.IsAbstract && !t.IsGenericType);
        }
        else
        {
            isValidate = false;
            subTypes = Enumerable.Empty<Type>();
        }

    }
#endif
}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(ReferenceTypeSelectorAttribute))]
public class ReferenceTypeSelectorDrawer : PropertyDrawer
{

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        var height = EditorGUI.GetPropertyHeight(property, label, true);
        if (property.hasVisibleChildren && property.isExpanded)
        {
            var targetObject = SerializedPropertyUtil.GetTargetObject(property);
            if (targetObject != null)
            {
                height += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            }
        }
        return height;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var typeSelector = attribute as ReferenceTypeSelectorAttribute;
        var targetObject = SerializedPropertyUtil.GetTargetObject(property);
        typeSelector.GetSubTypes(property);



        if (targetObject != null)
        {
            label.text = targetObject.GetType().Name;
            if (property.hasVisibleChildren == false)
            {
                ShowInline(position, property, label, typeSelector, targetObject);
            }
            else
            {
                ShowFoldout(position, property, label, typeSelector, targetObject);
            }
        }
        else
        {
            label.text = "Select Rule";
            ShowInline(position, property, label, typeSelector, targetObject);
        }
    }

    void ShowInline(Rect position, SerializedProperty property, GUIContent label, ReferenceTypeSelectorAttribute typeSelector, object targetObject)
    {
        var labalPosition = new Rect(position.x, position.y, EditorGUIUtility.labelWidth, position.height);
        var buttonPosition = new Rect(position.x + EditorGUIUtility.labelWidth + 2, position.y, position.width - EditorGUIUtility.labelWidth - 2, position.height);
        EditorGUI.LabelField(labalPosition, label);
        ShowInstanceMenu(buttonPosition, property, typeSelector, targetObject);
    }

    void ShowFoldout(Rect position, SerializedProperty property, GUIContent label, ReferenceTypeSelectorAttribute typeSelector, object targetObject)
    {
        var buttonPosition = new Rect(position.x + EditorGUIUtility.labelWidth + 2, position.y, position.width - EditorGUIUtility.labelWidth - 2, EditorGUIUtility.singleLineHeight);
        ShowInstanceMenu(buttonPosition, property, typeSelector, targetObject);
        EditorGUI.PropertyField(position, property, label, true);

    }

    void ShowInstanceMenu(Rect position, SerializedProperty property, ReferenceTypeSelectorAttribute typeSelector, object targetObject)
    {
        var baseType = typeSelector.baseType;
        var buttonText = (targetObject == null) ? "null" : "Change";

        if (GUI.Button(position, buttonText))
        {
            var context = new GenericMenu();

            foreach (var subType in typeSelector.subTypes)
            {
                context.AddItem(new GUIContent(subType.Name), false, () =>
                {
                    property.managedReferenceValue = Activator.CreateInstance(subType);
                    property.serializedObject.ApplyModifiedProperties();
                });
            }
            context.AddSeparator("");
            context.AddItem(new GUIContent("Set null"), false, () =>
            {
                property.managedReferenceValue = null;
                property.serializedObject.ApplyModifiedProperties();
            });

            if (typeSelector.isAbstractBase)
            {
                context.AddDisabledItem(new GUIContent(baseType.Name));
            }
            else
            {
                context.AddItem(new GUIContent(baseType.Name), false, () =>
                {
                    property.managedReferenceValue = Activator.CreateInstance(baseType);
                    property.serializedObject.ApplyModifiedProperties();
                });
            }

            var pos = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight, 0, 0);
            context.DropDown(pos);
        }
    }
}
#endif
