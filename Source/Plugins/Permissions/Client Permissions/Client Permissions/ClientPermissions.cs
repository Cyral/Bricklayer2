using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using Bricklayer.Core.Client;
using Bricklayer.Core.Client.Interface.Screens;
using Bricklayer.Core.Common;
using Bricklayer.Plugins.Permissions.Common;
using MonoForce.Controls;

namespace Bricklayer.Plugins.Permissions.Client
{
    public class ClientPermissions : ClientPlugin
    {
        /// <summary>
        /// All nodes available on the server client is currently connected to
        /// </summary>
        public List<PermissionsNode> AllNodes { get; private set; } 

        /// <summary>
        /// Permission the client has
        /// </summary>
        public List<PermissionsNode> Permissions { get; private set; }

        /// <summary>
        /// Group the client is apart of
        /// </summary>
        public PermissionsGroup Group { get; private set; }
         
        public ClientPermissions(Core.Client.Client host) : base(host)  { }

        public override void Load()
        {
            Permissions = new List<PermissionsNode>();
            AllNodes = new EventedList<PermissionsNode>();

            AllNodes.Add(new PermissionsNode("lobby.createlevel"));
            Permissions.Add(GetNode("lobby.createlevel"));


            Client.Events.Game.ScreenChanged.AddHandler(args =>
            {
                if (!(Client.Window.ScreenManager.Current is LobbyScreen) && !HasPermission("lobby.createlevel")) return;

                var screen = (LobbyScreen) Client.Window.ScreenManager.Current;

                screen.WndLobby.BtnCreate.Enabled = false;

                Debug.WriteLine(screen.WndLobby.BtnCreate.Text);
            });

        }

        public override void Unload()
        {
            AllNodes.Clear();
            Permissions.Clear();
        }

        /// <summary>
        /// If client has permission
        /// </summary>
        /// <param name="permission"></param>
        /// <returns></returns>
        public bool HasPermission(string permission)
        {
            if (GetNode(permission) != null)
            {
                return Permissions.Contains(GetNode(permission)) ||
                Group.Nodes.Contains(GetNode(permission));
            }
            return false;
        }

        /// <summary>
        /// Convert string version of permission to PermissionsNode object
        /// </summary>
        /// <returns></returns>
        public PermissionsNode GetNode(string permission)
        {
            var nodes = permission.Split('.');

            // If child nodes exist
            if (nodes.Length != 1)
            {
                // Get parent node
                var parent = AllNodes.FirstOrDefault(x => x.Node == nodes[0]);
                // Loop through every child node
                for (var x = 1; x < nodes.Length; x++)
                {
                    parent = parent?.ChildNodes.FirstOrDefault(i => i.Node == nodes[x]);
                    if (parent == null)
                        break;
                }
                return parent;
            }
            else
                return AllNodes.FirstOrDefault(x => x.Node == permission);

        }

    }
}
