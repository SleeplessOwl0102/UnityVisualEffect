using UnityEditor.IMGUI.Controls;

namespace YusiangLai.UnityEditor.MenuTreeFramework
{
    class MenuTreeViewItem : TreeViewItem
    {

        public UnityEngine.Object asset;
        public string path;

        public MenuTreeViewItem(string path, UnityEngine.Object asset = null)
        {
            this.path = path;
            this.asset = asset;
        }

    }
}

