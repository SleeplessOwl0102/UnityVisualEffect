using BlackJack.ProjectH.EditorRuntime.SceneValidator;
using System.Collections.Generic;
using System.Linq;
using TreeViewFramework;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEditor.TreeViewFramework;
using UnityEngine;

namespace BlackJack.ProjectH.Editor.SceneValidator
{
    [CustomEditor(typeof(SceneValidateConfig))]
    public class SceneValidateConfigEditor : UnityEditor.Editor
    {
        ValidateNodeTreeView m_TreeView;
        SearchField m_SearchField;

        const string kSessionStateKeyPrefix = "kSessionStateKeyPrefix";
        private IList<int> cache_selections;
        private List<int> cache_dataIndex;

        SceneValidateConfig asset
        {
            get { return (SceneValidateConfig)target; }
        }



        void OnEnable()
        {
            cache_selections = new List<int>();
            cache_dataIndex = new List<int>();

            Undo.undoRedoPerformed += OnUndoRedoPerformed;

            var treeViewState = new TreeViewState();
            var jsonState = SessionState.GetString(kSessionStateKeyPrefix + asset.GetInstanceID(), "");
            if (!string.IsNullOrEmpty(jsonState))
                JsonUtility.FromJsonOverwrite(jsonState, treeViewState);
            var treeModel = new TreeModel<NodeTreeElement>(asset.m_node);
            m_TreeView = new ValidateNodeTreeView(treeViewState, treeModel);

            m_TreeView.beforeDroppingDraggedItems += OnBeforeDroppingDraggedItems;
            m_TreeView.Reload();

            m_SearchField = new SearchField();
            m_SearchField.downOrUpArrowKeyPressed += m_TreeView.SetFocusAndEnsureSelectedItem;
        }


        void OnDisable()
        {
            Undo.undoRedoPerformed -= OnUndoRedoPerformed;

            SessionState.SetString(kSessionStateKeyPrefix + asset.GetInstanceID(), JsonUtility.ToJson(m_TreeView.state));
        }

        void OnUndoRedoPerformed()
        {
            if (m_TreeView != null)
            {
                m_TreeView.treeModel.SetData(asset.m_node);
                m_TreeView.Reload();
            }
        }

        void OnBeforeDroppingDraggedItems(IList<TreeViewItem> draggedRows)
        {
            Undo.RecordObject(asset, string.Format("Moving {0} Item{1}", draggedRows.Count, draggedRows.Count > 1 ? "s" : ""));
        }

        private void BuildSelectIndexList()
        {
            var temp = m_TreeView.GetSelection();
            if (Enumerable.SequenceEqual(temp, cache_selections))
                return;

            cache_selections = temp;

            cache_dataIndex.Clear();
            foreach (var id in cache_selections)
            {
                var data = m_TreeView.treeModel.Find(id);

                int index = asset.m_node.FindIndex((x) => (x.id == id));
                cache_dataIndex.Add(index);

            }
        }

        public override void OnInspectorGUI()
        {
            base.DrawDefaultInspector();
            if (GUILayout.Button("Update Path"))
            {
                asset.NodePathFieldUpdate();
                Repaint();
                return;
            }

            if (GUILayout.Button("Generate Node"))
            {
                asset.GenerateNodeInScene();
            }

            if (GUILayout.Button("Validate Scene"))
            {
                asset.ValidateScene(new List<LogEntry>() );
            }
            if (GUILayout.Button("Generate Node From Scene"))
            {
                asset.NodeFromSceneGenerate();
            }

            GUILayout.Space(5f);
            ToolBar();
            GUILayout.Space(3f);

            EditorGUILayout.BeginHorizontal();
            {
                const float topToolbarHeight = 20f;
                const float spacing = 2f;
                float totalHeight = m_TreeView.totalHeight + topToolbarHeight + 2 * spacing;
                Rect rect = GUILayoutUtility.GetRect(0, 250, 0, totalHeight);

                Rect toolbarRect = new Rect(rect.x, rect.y, rect.width, topToolbarHeight);
                GUI.Label(toolbarRect, "Scene Hierarchy");
                //SearchBar(toolbarRect);

                Rect multiColumnTreeViewRect = new Rect(rect.x, rect.y + topToolbarHeight + spacing, rect.width, Mathf.Max(300, rect.height - topToolbarHeight - 2 * spacing));
                DoTreeView(multiColumnTreeViewRect);


                GUILayout.Space(20f);
                EditorGUILayout.BeginVertical();
                {
                    BuildSelectIndexList();
                    foreach (var id in cache_dataIndex)
                    {
                        var prop1 = serializedObject.FindProperty("m_node");
                        if (id < 0 || id >= prop1.arraySize)
                            continue;
                        
                        var prop = prop1.GetArrayElementAtIndex(id);
                        prop.isExpanded = true;
                        EditorGUILayout.PropertyField(prop, new GUIContent("Game Object"));
                    }
                }
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndHorizontal();
            serializedObject.ApplyModifiedProperties();
        }

        void SearchBar(Rect rect)
        {
            m_TreeView.searchString = m_SearchField.OnGUI(rect, m_TreeView.searchString);
        }

        void DoTreeView(Rect rect)
        {
            m_TreeView.OnGUI(rect);
        }

        void ToolBar()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                var style = "miniButton";
                if (GUILayout.Button("Expand All", style))
                {
                    m_TreeView.ExpandAll();
                }

                if (GUILayout.Button("Collapse All", style))
                {
                    m_TreeView.CollapseAll();
                }

                GUILayout.FlexibleSpace();

                if (GUILayout.Button("Add Item", style))
                {
                    Undo.RecordObject(asset, "Add Item To Asset");

                    // Add item as child of selection
                    var selection = m_TreeView.GetSelection();
                    TreeElement parent = (selection.Count == 1 ? m_TreeView.treeModel.Find(selection[0]) : null) ?? m_TreeView.treeModel.root;
                    int depth = parent != null ? parent.depth + 1 : 0;
                    int id = m_TreeView.treeModel.GenerateUniqueID();
                    var element = new NodeTreeElement("Item " + id, depth, id);
                    m_TreeView.treeModel.AddElement(element, parent, 0);

                    // Select newly created element
                    m_TreeView.SetSelection(new[] { id }, TreeViewSelectionOptions.RevealAndFrame);
                }

                if (GUILayout.Button("Remove Item", style))
                {
                    Undo.RecordObject(asset, "Remove Item From Asset");
                    var selection = m_TreeView.GetSelection();
                    m_TreeView.treeModel.RemoveElements(selection);
                }
            }
        }
    }

    /// <summary>
    /// 绘制TreeView的客制化实现
    /// </summary>
    class ValidateNodeTreeView : TreeViewWithTreeModel<NodeTreeElement>
    {
        public ValidateNodeTreeView(TreeViewState state, TreeModel<NodeTreeElement> model)
            : base(state, model)
        {
            showBorder = true;
            showAlternatingRowBackgrounds = true;
        }

        protected override void RowGUI(RowGUIArgs args)
        {
            var item = (TreeViewItem<NodeTreeElement>)args.item;
            var data = item.data;
            var rect = args.rowRect;

            rect.xMin += 14;
            for (int i = 0; i < data.depth; i++)
            {
                rect.xMin += 14;
            }

            //GUIStyle insertVarNameHere = new GUIStyle();
            //insertVarNameHere.fontStyle = FontStyle.Bold;

            string extraName = string.Empty;
            if(string.IsNullOrEmpty(data.m_preName) == false)
            {
                extraName += $" ({data.m_preName})";
            }
            if (data.m_isEditorOnly)
            {
                extraName += "  (EditorOnly)";
            }
            if (data.m_isOptionNode)
            {
                extraName += " (非必须节点)";
            }

            GUI.Label(rect, item.data.name + extraName);

        }
    }
}