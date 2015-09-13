using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bricklayer.Core.Common.Data;
using Bricklayer.Core.Common.Net.Messages;
using Bricklayer.Core.Server;
using Bricklayer.Plugins.Permissions.Common;
using Pyratron.Frameworks.LogConsole;

namespace Bricklayer.Plugins.Permissions.Server
{
    public class Permissions : ServerPlugin
    {
        public List<PermissionsNode> Nodes { get; private set; }
        public List<PermissionsGroup> Groups { get; private set; }
        public Dictionary<PlayerData, PermissionsGroup> PlayerGroups { get; private set; }
        public Dictionary<PlayerData, List<PermissionsNode>> PlayerPermissions { get; private set; }

        public Permissions(Core.Server.Server host) : base(host)
        {
        }

        public override void Load()
        {
                Nodes = new List<PermissionsNode>();
                Groups = new List<PermissionsGroup>();
                PlayerGroups = new Dictionary<PlayerData, PermissionsGroup>();
                PlayerPermissions = new Dictionary<PlayerData, List<PermissionsNode>>();

                // Add default permission group that all players without a group have
                AddGroup("Default");

                AddNode(new PermissionsNode("lobby"));
                GetNode("lobby").AddChild("createworld");

                GetDefaultGroup().Nodes.Add(GetNode("lobby.createworld"));

                Server.Events.Network.UserConnected.AddHandler(args =>
                {
                    // Add Player to default group
                    AddToGroup(args.Player, GetDefaultGroup());
                    PlayerPermissions[args.Player] = new List<PermissionsNode>();
                });
                Server.Events.Network.ChatMessageReceived.AddHandler(args =>
                {
                    if (args.Message == "!perm")
                    {
                        Server.Net.Send(
                        new ChatMessage("[color:Blue]" + HasPermission(args.Sender, "lobby.createworld").ToString() + "[/color]"),
                        args.Sender);
                    }
                    if (args.Message != "!group") return;

                    Server.Net.Send(
                        new ChatMessage("[color:Yellow]Your group: " + GetPlayerGroup(args.Sender).Name + "[/color]"),
                        args.Sender);
                    Server.Net.Send(
                        new ChatMessage("[color:Yellow]Permissions: " + GetPlayerGroup(args.Sender).Nodes[0].FullNode() +
                                        "[/color]"), args.Sender);
                });
        }

        public override void Unload()
        {
            Nodes.Clear();
            Groups.Clear();
            PlayerGroups.Clear();
            PlayerPermissions.Clear();
        }

        /// <summary>
        /// Get permissions group by name
        /// </summary>
        /// <returns></returns>
        public PermissionsGroup GetGroup(string name)
        {
            return Groups.FirstOrDefault(x => x.Name == name);
        }

        /// <summary>
        /// Add permission group
        /// </summary>
        public void AddGroup(string name)
        {
            Groups.Add(new PermissionsGroup(name));
        }

        /// <summary>
        /// Add permission group with list of group permissions
        /// </summary>
        public void AddGroup(string name, List<PermissionsNode> nodes )
        {
            Groups.Add(new PermissionsGroup(name, nodes));
        }

        /// <summary>
        /// Add player to group by group name
        /// </summary>
        public void AddToGroup(PlayerData player, string group)
        {
            PlayerGroups.Add(player, GetGroup(group));
        }

        /// <summary>
        /// Add player to group object
        /// </summary>
        public void AddToGroup(PlayerData player, PermissionsGroup group)
        {
            PlayerGroups.Add(player, group);
        }

        /// <summary>
        /// Get default permissions group
        /// </summary>
        /// <returns></returns>
        public PermissionsGroup GetDefaultGroup()
        {
            return Groups[0];
        }

        /// <summary>
        /// Get player's permission group
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public PermissionsGroup GetPlayerGroup(PlayerData player)
        {
            return PlayerGroups[player];
        }

        /// <summary>
        /// Add permission
        /// </summary>
        public void AddNode(PermissionsNode node)
        {
            Nodes.Add(node);
        }

        /// <summary>
        /// Get player permissions
        /// </summary>
        /// <returns></returns>
        public List<PermissionsNode> GetPlayerPermissions(PlayerData player)
        {
            return PlayerPermissions[player];
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
                var parent = Nodes.FirstOrDefault(x => x.Node == nodes[0]);
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
                return Nodes.FirstOrDefault(x => x.Node == permission);

        }

        /// <summary>
        /// Check if player has permission
        /// </summary>
        /// <returns></returns>
        public bool HasPermission(PlayerData player, string permission)
        {
            if (GetNode(permission) != null)
            {
                return PlayerPermissions[player].Contains(GetNode(permission)) ||
                       GetPlayerGroup(player).Nodes.Contains(GetNode(permission));
            }
            return false;
        }


    }
}
