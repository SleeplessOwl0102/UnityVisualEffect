using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

//https://m.facebook.com/groups/UnityFrontier/posts/4114981571879512/

[AttributeUsage (AttributeTargets.Field)]
public class ReferenceTypeSelectorAttribute : PropertyAttribute {

    public bool isAbstractBase { get; private set; }
    public bool isValidate { get; private set; }
    public Type baseType { get; private set; }
    public IEnumerable<Type> subTypes { get; private set; }
    string filterMethod;

    public ReferenceTypeSelectorAttribute (Type baseType) {
        this.baseType = baseType;
        subTypes = baseType.Assembly.GetTypes ().Where (t => baseType.IsAssignableFrom (t) && t != baseType && !t.IsAbstract && !t.IsGenericTypeDefinition);
        isAbstractBase = baseType.IsAbstract;
        isValidate = true;
    }

    public ReferenceTypeSelectorAttribute (Type baseType, string filterMethod) {
        this.baseType = baseType;
        this.filterMethod = filterMethod;
        isAbstractBase = baseType.IsAbstract;
    }

    public void GetSubTypes (SerializedProperty property) {
        if (subTypes != null) {
            return;
        }

        if (string.IsNullOrEmpty (filterMethod)) {
            isValidate = false;
            subTypes = Enumerable.Empty<Type> ();
            return;
        }

        var obj = property.serializedObject.targetObject as object;
        var objType = obj.GetType ();
        var filter = objType.GetMethod (filterMethod, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);

        if (filter != null && filter.ReturnParameter.ParameterType == typeof (IEnumerable<System.Type>) && filter.GetParameters ().Length == 0) {
            isValidate = true;
            subTypes = filter.Invoke (null, null) as IEnumerable<System.Type>;
            subTypes = subTypes.Where (t => baseType.IsAssignableFrom (t) && t != baseType && !t.IsAbstract && !t.IsGenericType);
        } else {
            isValidate = false;
            subTypes = Enumerable.Empty<Type> ();
        }
    }
}

#if UNITY_EDITOR
[CustomPropertyDrawer (typeof (ReferenceTypeSelectorAttribute))]
public class ReferenceTypeSelectorDrawer : PropertyDrawer {

    public override float GetPropertyHeight (SerializedProperty property, GUIContent label) {
        var height = EditorGUI.GetPropertyHeight (property, label, true);
        if (property.hasVisibleChildren && property.isExpanded) {
            var targetObject = SerializedPropertyUtil.GetTargetObject (property);
            if (targetObject != null) {
                height += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            }
        }
        return height;
    }

    public override void OnGUI (Rect position, SerializedProperty property, GUIContent label) {
        var typeSelector = attribute as ReferenceTypeSelectorAttribute;
        var targetObject = SerializedPropertyUtil.GetTargetObject (property);
        typeSelector.GetSubTypes (property);

        if (targetObject != null && property.hasVisibleChildren) {
            ShowFoldout (position, property, label, typeSelector, targetObject);
        } else {
            ShowInline (position, property, label, typeSelector, targetObject);
        }
    }

    void ShowInline (Rect position, SerializedProperty property, GUIContent label, ReferenceTypeSelectorAttribute typeSelector, object targetObject) {
        var labalPosition = new Rect (position.x, position.y, EditorGUIUtility.labelWidth, position.height);
        var buttonPosition = new Rect (position.x + EditorGUIUtility.labelWidth + 2, position.y, position.width - EditorGUIUtility.labelWidth - 2, position.height);
        EditorGUI.LabelField (labalPosition, label);
        ShowInstanceMenu (buttonPosition, property, typeSelector, targetObject);
    }

    void ShowFoldout (Rect position, SerializedProperty property, GUIContent label, ReferenceTypeSelectorAttribute typeSelector, object targetObject) {
        EditorGUI.PropertyField (position, property, label, true);
        if (property.isExpanded) {
            var buttonPosition = new Rect (position.x, position.y + position.height - EditorGUIUtility.singleLineHeight, position.width, EditorGUIUtility.singleLineHeight);
            using (new EditorGUI.IndentLevelScope (1)) {
                buttonPosition = EditorGUI.IndentedRect (buttonPosition);
                ShowInstanceMenu (buttonPosition, property, typeSelector, targetObject);
            }
        }
    }

    void ShowInstanceMenu (Rect position, SerializedProperty property, ReferenceTypeSelectorAttribute typeSelector, object targetObject) {
        var baseType = typeSelector.baseType;
        var buttonText = (targetObject == null) ? "null" : targetObject.GetType ().Name;

        if (GUI.Button (position, buttonText)) {
            var context = new GenericMenu ();

            foreach (var subType in typeSelector.subTypes) {
                context.AddItem (new GUIContent (subType.Name), false, () => {
                    property.managedReferenceValue = Activator.CreateInstance (subType);
                    property.serializedObject.ApplyModifiedProperties ();
                });
            }
            context.AddSeparator ("");
            context.AddItem (new GUIContent ("Set null"), false, () => {
                property.managedReferenceValue = null;
                property.serializedObject.ApplyModifiedProperties ();
            });

            if (typeSelector.isAbstractBase) {
                context.AddDisabledItem (new GUIContent (baseType.Name));
            } else {
                context.AddItem (new GUIContent (baseType.Name), false, () => {
                    property.managedReferenceValue = Activator.CreateInstance (baseType);
                    property.serializedObject.ApplyModifiedProperties ();
                });
            }

            var pos = new Rect (position.x, position.y + EditorGUIUtility.singleLineHeight, 0, 0);
            context.DropDown (pos);
        }
    }
}
#endif
