using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
#endif

[AttributeUsage (AttributeTargets.Field)]
public class ReorderableListAttribute : PropertyAttribute {
    // TODO
}

#if UNITY_EDITOR
[CustomPropertyDrawer (typeof (ReorderableListAttribute))]
public class ReorderableListDrawer : ArrayDrawer {

    readonly Dictionary<string, ReorderableListWarpper> reorderableListMap = new Dictionary<string, ReorderableListWarpper> ();

    public override float GetPropertyHeight (SerializedProperty property, GUIContent label) {
        var height = EditorGUIUtility.singleLineHeight;
        if (property.isExpanded) {
            var list = GetReorderableList (property);
            height += EditorGUIUtility.standardVerticalSpacing + list.GetHeight ();
        }
        return height;
    }

    public override void OnGUI (Rect position, SerializedProperty property, GUIContent label) {
        position.height = EditorGUIUtility.singleLineHeight;
        property.isExpanded = EditorGUI.Foldout (position, property.isExpanded, label);

        if (property.isExpanded) {
            var spacing = EditorGUIUtility.standardVerticalSpacing;
            position.y += spacing;
            position.y += position.height;

            var list = GetReorderableList (property);
            position.height = list.GetHeight ();
            list.DoList (position);
        }
    }

    ReorderableListWarpper GetReorderableList (SerializedProperty property) {
        var propertyPath = property.propertyPath;
        var reorderableList = default (ReorderableListWarpper);
        if (reorderableListMap.TryGetValue (propertyPath, out reorderableList)) {
            return reorderableList;
        } else {
            reorderableList = new ReorderableListWarpper (property);
            reorderableListMap.Add (propertyPath, reorderableList);
            return reorderableList;
        }
    }
}

public class ReorderableListWarpper {

    SerializedProperty serializedProperty;
    ReorderableList list;

    public ReorderableListWarpper (SerializedProperty property) {
        serializedProperty = property;
        list = new ReorderableList (property.serializedObject, property, true, true, true, true);
        list.elementHeightCallback = GetListElementHeight;
        list.drawHeaderCallback = DrawListHeader;
        list.drawElementCallback = DrawListElement;
    }

    public float GetHeight () {
        return list.GetHeight ();
    }

    public void DoList (Rect position) {
        list.DoList (position);
    }

    float GetListElementHeight (int index) {
        var element = serializedProperty.GetArrayElementAtIndex (index);
        return EditorGUI.GetPropertyHeight (element);
    }

    void DrawListHeader (Rect rect) {
        EditorGUI.LabelField (rect, serializedProperty.displayName);
    }

    void DrawListElement (Rect rect, int index, bool isActive, bool isFocused) {
        var element = serializedProperty.GetArrayElementAtIndex (index);
        using (new EditorGUI.IndentLevelScope (1)) {
            EditorGUI.PropertyField (rect, element, true);
        }
    }
}

#endif