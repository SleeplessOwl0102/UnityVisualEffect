using System.Collections.Generic;
using UnityEngine;

namespace BlackJack.ProjectH.EditorRuntime.SceneValidator
{
    [CreateAssetMenu(fileName = "Data", menuName = "Custom/SpawnManagerScriptableObject", order = 1)]
    public class SceneValidateConfig : ScriptableObject
    {

        /// <summary>
        /// 从既有场景生成节点
        /// </summary>
        public void NodeFromSceneGenerate()
        {
            int id = 0;
            AddGo(GameObject.Find("/SceneRoot").transform, 0);

            void AddGo(Transform go, int depth)
            {
                if (depth == 3)
                    return;
                id++;
                m_node.Add(new NodeTreeElement(go.gameObject.name, depth, id));
                for (int i = 0; i < go.childCount; i++)
                {
                    AddGo(go.GetChild(i), depth + 1);
                }
            }

        }


        public string m_description;

        [SerializeField]
        public List<NodeTreeElement> m_node =
            new List<NodeTreeElement>() { new NodeTreeElement("Hidden root", -1, 0) };

        public void NodePathFieldUpdate()
        {
            int preDepth = -1;
            Stack<string> stack = new Stack<string>(5);
            foreach (var item in m_node)
            {
                if (item.depth == -1)
                {
                    stack.Push("");
                }
                else if (item.depth == preDepth + 1)
                {
                    item.m_path = $"{stack.Peek()}/{item.name}";
                    stack.Push(item.m_path);
                    preDepth = item.depth;
                }
                else if (item.depth == preDepth)
                {
                    stack.Pop();
                    preDepth = item.depth;
                    item.m_path = $"{stack.Peek()}/{item.name}";
                    stack.Push(item.m_path);
                }
                else if (item.depth < preDepth)
                {
                    for (int i = 0; i <= preDepth - item.depth; i++)
                    {
                        if (preDepth - item.depth >= 1)
                            stack.Pop();
                    }
                    preDepth = item.depth;
                    item.m_path = $"{stack.Peek()}/{item.name}";
                    stack.Push(item.m_path);
                }


            }


        }

        public void GenerateNodeInScene()
        {
            foreach (var item in m_node)
            {
                if (item.depth == -1)
                    continue;
                if (GameObject.Find(item.m_path) == null)
                {
                    var go = new GameObject(item.name);

                    if (item.depth > 0)
                    {
                        Debug.Log(item.m_path.Substring(0, item.m_path.LastIndexOf("/")));
                        go.transform.parent = GameObject.Find(item.m_path.Substring(0, item.m_path.LastIndexOf("/") + 1)).transform;

                    }
                }
                Debug.Log(GameObject.Find(item.m_path));
            }
        }


        public void ValidateScene(IList<LogEntry> logs)
        {
            NodePathFieldUpdate();

            var dic = new Dictionary<NodeTreeElement, GameObject>();
            var skipIfLargeThan = int.MaxValue;

            int i = 0;
            foreach (var item in m_node)
            {
                i++;
                if (item.depth == -1)
                    continue;

                if (item.depth > skipIfLargeThan)
                    continue;

                var go = GameObject.Find(item.m_path);

                if (go == null)
                {

                    if (item.m_isOptionNode == false)
                    {
                        go = GameObject.Find(item.name);

                        if (go == null && string.IsNullOrEmpty(item.m_preName) == false)
                        {
                            go = GameObject.Find(item.m_preName);
                            go.name = item.name;
                        }

                        if (go == null)
                            go = new GameObject(item.name);

                        if (item.depth > 0)
                        {
                            go.transform.parent = GameObject.Find(item.m_path.Substring(0, item.m_path.LastIndexOf("/") + 1)).transform;
                        }
                        if (item.depth == 0)
                        {
                            go.transform.parent = null;
                        }
                        Debug.Log(go.name);
                        dic.Add(item, go);
                    }
                    else
                    {
                        //此点不存在，所以更深的点也不用检查了
                        skipIfLargeThan = item.depth;
                        continue;
                    }

                }
                else
                {
                    //恢复检查子节点
                    skipIfLargeThan = int.MaxValue;
                    Debug.Log(go.name);
                    dic.Add(item, go);

                }

                //设置同depth物件的排序
                go.transform.SetSiblingIndex(i);

                //SelfRule检查
                foreach (var pair in dic)
                {
                    SelfRuleCheck(pair.Key, pair.Value, logs);
                }

                //AllChildsRule
                foreach (var pair in dic)
                {
                    if (pair.Key.m_allChildsRule.Count > 0)
                    {
                        AllChildsRuleCheck(pair.Key, pair.Value, logs);
                    }
                }

            }
        }

        private void AllChildsRuleCheck(NodeTreeElement element, GameObject go, IList<LogEntry> logs)
        {
            var allChilds = go.GetAllChilds();
            foreach (var rule in element.m_allChildsRule)
            {
                foreach (var child in allChilds)
                {
                    rule.SceneNodeVerify(child,logs);
                }
            }
        }

        private void SelfRuleCheck(NodeTreeElement element, GameObject go, IList<LogEntry> logs)
        {
            if (element.m_isEditorOnly && go.CompareTag("EditorOnly") == false)
            {
                go.tag = "EditorOnly";
                logs.Add(LogEntry.Create(LogEntry.LogTypeError, $"物件 {go.name} 的Tag修正为 EditorOnly Tag", go));
            }

            var comp = go.GetComponent<Transform>();
            if (element.m_isRootNode)
            {
                if (comp.position != Vector3.zero)
                {
                    comp.position = Vector3.zero;
                    logs.Add(LogEntry.Create(LogEntry.LogTypeError, $"物件 {go.name} 的 position 修正为 (0,0,0)", go));
                }
                if (comp.rotation != Quaternion.identity)
                {
                    comp.rotation = Quaternion.identity;
                    logs.Add(LogEntry.Create(LogEntry.LogTypeError, $"物件 {go.name} 的 rotation 修正为 (0,0,0)", go));
                }
                if (comp.localScale != Vector3.one)
                {
                    comp.localScale = Vector3.one;
                    logs.Add(LogEntry.Create(LogEntry.LogTypeError, $"物件 {go.name} 的 scale 修正为 (1,1,1)", go));
                }
            }

            foreach (var rule in element.m_selfRule)
            {
                rule.SceneNodeVerify(go, logs);
            }

        }
    }


    public static class Helper
    {
        public static List<GameObject> GetAllChilds(this GameObject Go)
        {
            List<GameObject> list = new List<GameObject>();
            for (int i = 0; i < Go.transform.childCount; i++)
            {
                list.Add(Go.transform.GetChild(i).gameObject);
            }
            return list;
        }

    }

}