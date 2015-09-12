using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bricklayer.Plugins.Permissions.Common
{
    public class PermissionsGroup
    {
        /// <summary>
        /// Name of the group
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Permissions that group members get
        /// </summary>
        public List<PermissionsNode> Nodes { get; private set; }

        public PermissionsGroup(string name, List<PermissionsNode> nodes)
        {
            Name = name;
            Nodes = nodes;
        }

        public PermissionsGroup(string name)
        {
            Name = name;
            Nodes = new List<PermissionsNode>();
        }
    }
}
