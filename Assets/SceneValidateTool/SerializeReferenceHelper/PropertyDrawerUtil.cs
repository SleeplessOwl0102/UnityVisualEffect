using System.Collections;
using System.Collections.Generic;
using System;
using System.Reflection;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

public static class PropertyDrawerUtil {

    static readonly Type scriptAttributeUtilityType = typeof (PropertyDrawer).Assembly.GetType ("UnityEditor.ScriptAttributeUtility");
    static readonly PropertyInfo propertyHandlerCacheInfo = scriptAttributeUtilityType.GetProperty ("propertyHandlerCache", BindingFlags.NonPublic | BindingFlags.Static);

    static readonly Type PropertyHandlerCacheType = typeof (PropertyDrawer).Assembly.GetType ("UnityEditor.PropertyHandlerCache");
    static readonly FieldInfo propertyHandlersInfo = PropertyHandlerCacheType.GetField ("m_PropertyHandlers", BindingFlags.NonPublic | BindingFlags.Instance);

    static readonly Type propertyHandlerType = typeof (PropertyDrawer).Assembly.GetType ("UnityEditor.PropertyHandler");
    static readonly FieldInfo propertyDrawerInfo = propertyHandlerType.GetField ("m_PropertyDrawer", BindingFlags.NonPublic | BindingFlags.Instance);
    static readonly FieldInfo decoratorDrawersInfo = propertyHandlerType.GetField ("m_DecoratorDrawers", BindingFlags.NonPublic | BindingFlags.Instance);

    public static object GetPropertyHandler (DecoratorDrawer decoratorDrawer) {
        var propertyHandlerCache = propertyHandlerCacheInfo.GetValue (null, null);
        var propertyHandlerDictionary = (IDictionary)propertyHandlersInfo.GetValue (propertyHandlerCache);
        var propertyHandlers = propertyHandlerDictionary.Values;

        foreach (var propertyHandler in propertyHandlers) {
            var decoratorDrawers = (List<DecoratorDrawer>)decoratorDrawersInfo.GetValue (propertyHandler);
            if (decoratorDrawers == null) {
                continue;
            }

            var index = decoratorDrawers.IndexOf (decoratorDrawer);
            if (index < 0) {
                continue;
            }

            return propertyHandler;
        }
        return null;
    }

    public static PropertyDrawer GetPropertyDrawer (object propertyHandler) {
        return (PropertyDrawer)propertyDrawerInfo.GetValue (propertyHandler);
    }

    public static void SetPropertyDrawer (object propertyHandler, PropertyDrawer propertyDrawer) {
        propertyDrawerInfo.SetValue (propertyHandler, propertyDrawer);
    }
}
#endif