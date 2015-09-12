using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bricklayer.Plugins.Permissions.Common
{
    public class PermissionsNode
    {
        /// <summary>
        /// Identifier of permission
        /// </summary>
        public string Node { get; private set; }

        /// <summary>
        /// Child nodes of permission. (e.g. lobby.createlevel , "createlevel" would be a child node)
        /// </summary>
        private readonly List<PermissionsNode> childNodes;

        /// <summary>
        /// Get readonly list of child nodes.
        /// </summary>
        public IEnumerable<PermissionsNode> ChildNodes => new ReadOnlyCollection<PermissionsNode>(childNodes);

        /// <summary>
        /// Nodes parent to permission
        /// </summary>
        private List<PermissionsNode> ParentNodes;

        public PermissionsNode(string node)
        {
            Node = node;
            childNodes = new List<PermissionsNode>();
            ParentNodes = new List<PermissionsNode>();
        }

        public PermissionsNode(string node, List<PermissionsNode> childNodes)
        {
            Node = node;
            this.childNodes = childNodes;
            ParentNodes = new List<PermissionsNode>();
        }

        private PermissionsNode(string node, PermissionsNode parent)
        {
            Node = node;
            childNodes = new List<PermissionsNode>();
            ParentNodes = parent.ParentNodes;
            ParentNodes.Add(parent);
        }

        /// <summary>
        /// Add child to node
        /// </summary>
        /// <param name="node"></param>
        public void AddChild(string node)
        {
            childNodes.Add(new PermissionsNode(node, this));
        }

        /// <summary>
        /// Add child to node
        /// </summary>
        /// <param name="node"></param>
        public void AddChild(PermissionsNode node)
        {
            node.ParentNodes = ParentNodes;
            node.ParentNodes.Add(this);
            childNodes.Add(node);
        }


        /// <summary>
        /// Get node including the parents (e.g. lobby.createlevel)
        /// </summary>
        /// <returns></returns>
        public string FullNode()
        {
            if (ParentNodes.Count == 0) return Node;

            var node = new StringBuilder(ParentNodes[ParentNodes.Count - 1].Node + ".");
            if (ParentNodes.Count <= 1) return node + Node;
            for (var x = ParentNodes.Count - 2; x >= 0; x--)
                node.Append(ParentNodes[x].Node + ".");
            return node + Node;

        }

    }
}
