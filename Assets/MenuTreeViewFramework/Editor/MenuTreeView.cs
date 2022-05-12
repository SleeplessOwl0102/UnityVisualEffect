using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace YusiangLai.UnityEditor.MenuTreeFramework
{

    class MenuTreeView : TreeView
	{
		public MenuTreeView(TreeViewState treeViewState) : base(treeViewState)
		{
			dic = new Dictionary<string, MenuTreeViewItem>();
			menuTreeviewItems = new List<TreeViewItem>();
		}

		public MenuTreeViewItem GetMenuTreeviewItem(int id)
		{
			return (MenuTreeViewItem)menuTreeviewItems[id];
		}


		private List<TreeViewItem> menuTreeviewItems;
		private Dictionary<string, MenuTreeViewItem> dic;

		public void AddItem(MenuTreeViewItem item)
		{
			if (dic.ContainsKey(item.path))
			{
				//todo override parameter
				return;
			}

			var pathsplits = item.path.Split('/');

			//depth 0 is 
			item.depth = pathsplits.Length - 1;
			string path = string.Empty;
            for (int i = 0; i < pathsplits.Length - 1; i++)
            {
				 path += pathsplits[i];
				if(dic.ContainsKey(path)==false)
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
			dic.Add(item.path, item);

		}


        protected override void RowGUI(RowGUIArgs args)
        {
            base.RowGUI(args);
        }


        protected sealed override TreeViewItem BuildRoot ()
		{
			
			// BuildRoot is called every time Reload is called to ensure that TreeViewItems 
			// are created from data. Here we just create a fixed set of items, in a real world example
			// a data model should be passed into the TreeView and the items created from the model.

			// This section illustrates that IDs should be unique and that the root item is required to 
			// have a depth of -1 and the rest of the items increment from that.
			var root = new TreeViewItem {id = -1, depth = -1, displayName = "Root"};

			SetupParentsAndChildrenFromDepths(root, menuTreeviewItems);
			// Utility method that initializes the TreeViewItem.children and -parent for all items.
			//SetupParentsAndChildrenFromDepths (root, menuTreeviewItems);
			//SetupDepthsFromParentsAndChildren(root)
			// Return root of the tree
			return root;
		}

        protected override IList<TreeViewItem> BuildRows(TreeViewItem root)
        {
			return base.BuildRows(root);
        }
    }
}

