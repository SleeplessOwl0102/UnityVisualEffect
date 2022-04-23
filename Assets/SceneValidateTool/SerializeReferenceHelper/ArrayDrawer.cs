using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

public abstract class ArrayDrawer : DecoratorDrawer {

    bool injectArrayDrawer;

    public sealed override bool CanCacheInspectorGUI () {
        if (!injectArrayDrawer) {
            InjectArrayDrawer ();
        }
        return false;
    }

    public sealed override float GetHeight () {
        if (!injectArrayDrawer) {
            InjectArrayDrawer ();
        }
        return 0;
    }

    public sealed override void OnGUI (Rect position) { }

    void InjectArrayDrawer () {
        injectArrayDrawer = true;

        var propertyHandler = PropertyDrawerUtil.GetPropertyHandler (this);
        var propertyDrawer = PropertyDrawerUtil.GetPropertyDrawer (propertyHandler);

        if (propertyDrawer == null) {
            propertyDrawer = new ArrayDrawerAdapter (this);
            PropertyDrawerUtil.SetPropertyDrawer (propertyHandler, propertyDrawer);
        }
    }

    public virtual bool CanCacheInspectorGUI (SerializedProperty property) {
        return true;
    }

    public virtual float GetPropertyHeight (SerializedProperty property, GUIContent label) {
        return EditorGUI.GetPropertyHeight (property, label, true); ;
    }

    public virtual void OnGUI (Rect position, SerializedProperty property, GUIContent label) {
        EditorGUI.PropertyField (position, property, label, true);
    }
}

public class ArrayDrawerAdapter : PropertyDrawer {

    ArrayDrawer arrayDrawer;

    public ArrayDrawerAdapter (ArrayDrawer arrayDrawer) {
        this.arrayDrawer = arrayDrawer;
    }

    public override bool CanCacheInspectorGUI (SerializedProperty property) {
        return arrayDrawer.CanCacheInspectorGUI (property);
    }

    public override float GetPropertyHeight (SerializedProperty property, GUIContent label) {
        return arrayDrawer.GetPropertyHeight (property, label);
    }

    public override void OnGUI (Rect position, SerializedProperty property, GUIContent label) {
        arrayDrawer.OnGUI (position, property, label);
    }
}
#endif