using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEditor.TreeViewExamples;
using UnityEngine;

using System.Linq;
[CustomEditor(typeof(SceneValidateConfig))]
public class SceneValidateConfigEditor : Editor
{
    ValidateNodeTreeView m_TreeView;
    SearchField m_SearchField;

    const string kSessionStateKeyPrefix = "KKKKsdaK";
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
        var treeModel = new TreeModel<ValidateTreeElement>(asset.m_node);
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
        GUILayout.Space(5f);
        ToolBar();
        GUILayout.Space(3f);

        const float topToolbarHeight = 20f;
        const float spacing = 2f;
        float totalHeight = m_TreeView.totalHeight + topToolbarHeight + 2 * spacing;
        Rect rect = GUILayoutUtility.GetRect(0, 100, 0, totalHeight);
        Rect toolbarRect = new Rect(rect.x, rect.y, rect.width, topToolbarHeight);
        Rect multiColumnTreeViewRect = new Rect(rect.x, rect.y + topToolbarHeight + spacing, rect.width, rect.height - topToolbarHeight - 2 * spacing);
        SearchBar(toolbarRect);
        DoTreeView(multiColumnTreeViewRect);

        BuildSelectIndexList();
        foreach (var id in cache_dataIndex)
        {
            var prop = serializedObject.FindProperty("m_node").GetArrayElementAtIndex(id);
            prop.isExpanded = true;
            EditorGUILayout.PropertyField(prop, new GUIContent("Game Object"));
            
        }
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
                var element = new ValidateTreeElement("Item " + id, depth, id);
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

class ValidateNodeTreeView : TreeViewWithTreeModel<ValidateTreeElement>
{
    public ValidateNodeTreeView(TreeViewState state, TreeModel<ValidateTreeElement> model)
        : base(state, model)
    {
        showBorder = true;
        showAlternatingRowBackgrounds = true;
    }

    protected override void RowGUI(RowGUIArgs args)
    {
        var item = (TreeViewItem<ValidateTreeElement>)args.item;
        var data = item.data;
        var rect = args.rowRect;

        rect.xMin += 14;
            for (int i = 0; i < data.depth; i++)
            {
                rect.xMin += 14;
            }
        
        GUI.Label(rect, item.data.name);
    }
}
