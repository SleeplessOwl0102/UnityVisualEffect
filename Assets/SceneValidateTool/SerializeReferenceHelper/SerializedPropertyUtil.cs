using System.Collections;
using System.Collections.Generic;
using System;
using System.Reflection;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

public static class SerializedPropertyUtil {

    public static object GetTargetObject (SerializedProperty property) {
        if (property == null) {
            return null;
        }

        var obj = property.serializedObject.targetObject as object;
        var path = property.propertyPath.Replace (".Array.data[", "[");
        var elements = path.Split ('.');
        foreach (var element in elements) {
            if (element.Contains ("[")) {
                var charIndex = element.IndexOf ("[");
                var elementIndex = System.Convert.ToInt32 (element.Substring (charIndex + 1, element.Length - charIndex - 2));
                var elementName = element.Substring (0, charIndex);
                obj = GetValue (obj, elementName, elementIndex);
            } else {
                obj = GetValue (obj, element);
            }
        }
        return obj;
    }


    static object GetValue (object source, string name) {
        if (source == null) {
            return null;
        }

        var type = source.GetType ();
        while (type != null) {
            var field = type.GetField (name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            if (field != null) {
                return field.GetValue (source);
            }

            var property = type.GetProperty (name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (property != null) {
                return property.GetValue (source, null);
            }

            type = type.BaseType;
        }
        return null;
    }

    static object GetValue (object source, string name, int index) {
        var enumerable = GetValue (source, name) as System.Collections.IEnumerable;
        if (enumerable == null) {
            return null;
        }

        var enm = enumerable.GetEnumerator ();
        for (int i = 0; i <= index; i++) {
            if (!enm.MoveNext ()) return null;
        }
        return enm.Current;
    }
}
#endif