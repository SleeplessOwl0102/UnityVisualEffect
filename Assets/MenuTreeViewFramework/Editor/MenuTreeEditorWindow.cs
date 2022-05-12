using UnityEngine;
using UnityEditor.IMGUI.Controls;
using UnityEditor;

namespace YusiangLai.UnityEditor.MenuTreeFramework
{
	class MenuTreeEditorWindow : EditorWindow
	{
		// We are using SerializeField here to make sure view state is written to the window 
		// layout file. This means that the state survives restarting Unity as long as the window
		// is not closed. If omitting the attribute then the state just survives assembly reloading 
		// (i.e. it still gets serialized/deserialized)
		[SerializeField] TreeViewState m_TreeViewState;

		// The TreeView is not serializable it should be reconstructed from the tree data.
		MenuTreeView m_TreeView;
		SearchField m_SearchField;

		void OnEnable ()
		{
			// Check if we already had a serialized view state (state 
			// that survived assembly reloading)
			if (m_TreeViewState == null)
				m_TreeViewState = new TreeViewState ();

			m_TreeView = new MenuTreeView(m_TreeViewState);
			m_SearchField = new SearchField ();
			m_SearchField.downOrUpArrowKeyPressed += m_TreeView.SetFocusAndEnsureSelectedItem;

			InitTreeItem();
			m_TreeView.Reload();
		}

		protected virtual void InitTreeItem()
		{
			m_TreeView.AddItem(new MenuTreeViewItem("123/sfggsf/23214", AssetDatabase.LoadAssetAtPath<Object>("Assets/UniversalRenderPipelineGlobalSettings.asset")));
			m_TreeView.AddItem(new MenuTreeViewItem("123/sfggsf"));
			m_TreeView.AddItem(new MenuTreeViewItem("123/sfggsf/safdfsd"));
			m_TreeView.AddItem(new MenuTreeViewItem("23214"));
			m_TreeView.AddItem(new MenuTreeViewItem("23214/fdgsdfg"));
			m_TreeView.AddItem(new MenuTreeViewItem("23214/erwe"));
		}


		void OnGUI ()
		{
			EditorGUILayout.BeginHorizontal();
			{

				EditorGUILayout.BeginVertical(GUILayout.Width(150));
				{
					DoToolbar();
					scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
					{
						Rect rect = GUILayoutUtility.GetRect(0, 150, 0, 100000);
						m_TreeView.OnGUI(rect);
					}
					EditorGUILayout.EndScrollView();
				}
				EditorGUILayout.EndVertical();

				DrawVerticalLine();

				EditorGUILayout.BeginVertical();
				{
					if (m_TreeView.HasSelection())
					{
						DrawSelectedToolEditor();
					}
				}
				EditorGUILayout.EndVertical();
			}
			EditorGUILayout.EndHorizontal();
		}

		void DrawSelectedToolEditor()
		{
			var temp = m_TreeView.GetSelection();

			var item = m_TreeView.GetMenuTreeviewItem(temp[0]);
			EditorGUILayout.LabelField("Test");
			if (item.asset != null)
			{
				//EditorGUILayout.ObjectField
				var ss = Editor.CreateEditor(item.asset);
				ss.OnInspectorGUI();
			}
		}


		void DoToolbar()
		{
			GUILayout.BeginHorizontal (EditorStyles.toolbar);
			//GUILayout.Space (100);
			GUILayout.FlexibleSpace();
			m_TreeView.searchString = m_SearchField.OnToolbarGUI (m_TreeView.searchString);
			GUILayout.EndHorizontal();
		}
		Vector2 scrollPosition;

		void DrawVerticalLine()
		{
			EditorGUILayout.BeginVertical(GUILayout.Width(1));
			{
				//EditorGUILayout.GetControlRect
				Rect rect = GUILayoutUtility.GetRect(1, position.height);
				EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 1));
			}
			EditorGUILayout.EndVertical();
		}


		// Add menu named "My Window" to the Window menu
		[MenuItem ("TreeView Examples/Simple Tree Window")]
		static void ShowWindow ()
		{
			// Get existing open window or if none, make a new one:
			var window = GetWindow<MenuTreeEditorWindow> ();
			window.titleContent = new GUIContent ("My Window");
			window.Show ();
		}
	}
}
