using Godot;
using System.Collections.Generic;
using System.Linq;

namespace Globals
{
    [GlobalClass]
    public partial class GlobalSettings : Node
    {
        public static GlobalSettings Instance { get; private set; }

        [Export] public bool bIsMultiplayer { get; set; }

        public override void _Ready()
        {
            Instance = this;
        }
    }

    public static class GlobalHelpers
    {
        public static List<T> GetChildrenOfType<T>(Node parent, bool bRecursive = true)
        {
            List<T> children = new List<T>();
            foreach (Node node in parent.GetChildren())
            {
                if (node is T nodeT)
                {
                    children.Add(nodeT);
                }

                if (bRecursive)
                {
                    List<T> recursiveChildren = GetChildrenOfType<T>(node, true);
                    foreach (T recursiveChild in recursiveChildren)
                    {
                        children.Add(recursiveChild);
                    }
                }
            }

            return children;
        }
    }

}