using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace YusiangLai.UnityEditor.MenuTreeFramework
{
    class MenuTreeViewItem : TreeViewItem
    {

        public Object asset;
        public string path;

        public MenuTreeViewItem()
        {

        }


        public MenuTreeViewItem(string path, Object asset = null)
        {
            this.path = path;
            this.asset = asset;
        }

        

    }
}

