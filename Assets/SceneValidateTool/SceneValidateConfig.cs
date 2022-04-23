using System;
using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;
using UnityEditor.TreeViewExamples;
using UnityEngine;
using Object = UnityEngine.Object;

[CreateAssetMenu(fileName = "Data", menuName = "Custom/SpawnManagerScriptableObject", order = 1)]
public class SceneValidateConfig : ScriptableObject
{


    public string m_description;

    [SerializeField]
    public List<ValidateTreeElement> m_node = new List<ValidateTreeElement>(){new ValidateTreeElement("Hidden root",-1,0)};




}

[Serializable]
public class ValidateTreeElement : TreeElement
{

    [SerializeReference]
    [ReferenceTypeSelector(typeof(ISceneValidate))]
    public List<ISceneValidate> ComponentRule = new List<ISceneValidate>();

    [SerializeReference]
    [ReferenceTypeSelector(typeof(ISceneValidate))]
    public List<ISceneValidate> CommonRule = new List<ISceneValidate>();

    public ValidateTreeElement(string name, int depth, int id) : base(name, depth, id)
    {
    }
}


public interface ISceneValidate
{

}


[Serializable]
public class ValidateZero : ISceneValidate
{
    public Vector3 pos;
}


