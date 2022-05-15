using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace YusiangLai.UnityEditor.MenuTreeFramework
{

    class MenuTreeView : TreeView
	{
		public MenuTreeView(TreeViewState treeViewState) : base(treeViewState)
		{
			itemDict = new Dictionary<string, MenuTreeViewItem>();
			menuTreeviewItems = new List<TreeViewItem>();
		}

		public MenuTreeViewItem GetMenuTreeviewItem(int id)
		{
			return (MenuTreeViewItem)menuTreeviewItems[id];
		}


		private List<TreeViewItem> menuTreeviewItems;
		private Dictionary<string, MenuTreeViewItem> itemDict;

		public void AddItem(MenuTreeViewItem item)
		{
			if (itemDict.ContainsKey(item.path))
			{
				if (item.asset != null && itemDict[item.path].asset == null)
					itemDict[item.path].asset = item.asset;
				return;
			}

			//use path depth as item indent
			var pathsplits = item.path.Split('/');
			item.depth = pathsplits.Length - 1;

			string path = string.Empty;
            for (int i = 0; i < pathsplits.Length - 1; i++)
            {
				 path += pathsplits[i];
				if(itemDict.ContainsKey(path)==false)
                {
					AddItem(new MenuTreeViewItem(path));
                }

				if (i < pathsplits.Length - 1)
					path += "/";
            }

			item.id = menuTreeviewItems.Count;
			item.displayName = pathsplits[pathsplits.Length - 1];
			//Debug.Log($"{item.depth}     {item.path}");
			menuTreeviewItems.Add(item);
			itemDict.Add(item.path, item);

		}

		/// <summary>
        /// Draw RowGUi content
        /// </summary>
        /// <param name="args"></param>
        protected override void RowGUI(RowGUIArgs args)
        {
            base.RowGUI(args);
        }

		/// <summary>
		/// BuildRoot is called every time Reload is called to ensure that TreeViewItems are created from data.
		/// </summary>
		/// <returns></returns>
		protected sealed override TreeViewItem BuildRoot ()
		{
			// This section illustrates that IDs should be unique and that the root item is required to 
			// have a depth of -1 and the rest of the items increment from that.
			var root = new TreeViewItem {id = -1, depth = -1, displayName = "Root"};

			// Utility method that initializes the TreeViewItem.children and -parent for all items.
			SetupParentsAndChildrenFromDepths(root, menuTreeviewItems);

			return root;
		}

        protected override IList<TreeViewItem> BuildRows(TreeViewItem root)
        {
			return base.BuildRows(root);
        }
    }
}

