using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Rust;
using Oxide;
using Oxide.Core;
using Facepunch;
using Facepunch.MeshBatch;
using Oxide.Core.Plugins;
using System.Reflection;
namespace Oxide.Plugins
{
    [Info("Teams", "Daniel25A", 0.2)]
    [Description("Team Plugin")]

    class Teams : RustLegacyPlugin
    {
        static string SysName = "Teams";
        static Dictionary<ulong, UserTeam> TeamsPlayers = new Dictionary<ulong, UserTeam>();
        void CreateTeam(NetUser Admin, String[] Args)
        {
            string _TeamName;
            string _Owner;
            string _MsgColor;
            if (!Admin.admin) return;
            if (Args.Length < 3)
            {
                rust.SendChatMessage(Admin, SysName, "Syntax Error, /cteam (TeamName) (Owner) (ChatColor)");
            }
            else
            {
                _TeamName = Args[0].Trim();
                _Owner = Args[1];
                _MsgColor = Args[2];
                var User = rust.GetAllNetUsers().FirstOrDefault(x => x.displayName.Contains(_Owner));
                if (User == null)
                {
                    rust.SendChatMessage(Admin, SysName, "The user doest Exist");
                }
                else
                {
                    TeamsPlayers.Add(User.userID, new UserTeam()
                    {
                        PlayerID = User.userID,
                        isTheOwner = true,
                        TAG = _TeamName,
                        Color = _MsgColor
                    });
                    rust.SendChatMessage(Admin, SysName, string.Format("Team Create Sucefully -> Owner {0}", User.displayName));
                }
            }
        }
        void AddUserToTeam(NetUser Player,String UserName="")
        {
            if (TeamsPlayers.ContainsKey(Player.userID) == false) return;
            if (TeamsPlayers.Values.FirstOrDefault(x => x.PlayerID == Player.userID).isTheOwner == false)
            {
                rust.Notice(Player, "You dont are the Owner of the Team");
                return;

            }
            if (UserName == string.Empty || UserName == null) return;
            if (rust.GetAllNetUsers().FirstOrDefault(x => x.displayName.Contains(UserName)) == null)
                rust.Notice(Player, "User doest exist");
            else
            {
                UserTeam _config = TeamsPlayers.Values.First(x => x.PlayerID == Player.userID);
                var userAddToTeam = rust.GetAllNetUsers().FirstOrDefault(x => x.displayName.Contains(UserName));
                TeamsPlayers.Add(userAddToTeam.userID, new UserTeam()
                {
                    PlayerID=userAddToTeam.userID,
                    Color=_config.Color,
                    isTheOwner=false,
                    TAG=_config.TAG
                });
                rust.Notice(Player, "Done !");
            }
        }
        void OnPlayerChat(NetUser Player, string message)
        {
   
        }
        [ChatCommand("cteam")]
        void cmdCreateTeam(NetUser netUser, string command, string[] args)
        {
            CreateTeam(netUser, args);
        }
        [ChatCommand("cadduser")]
        void cmdadduser(NetUser netUser, string command, string[] args)
        {
            AddUserToTeam(netUser, args[0]);
        }
        [ChatCommand("serverinfo")]
        void cmdinfo(NetUser netUser, string command, string[] args)
        {
            if (netUser.admin == false) return;
            rust.BroadcastChat("ServerInfo", string.Format("Players Online:{0}", rust.GetAllNetUsers().Count()));
            rust.BroadcastChat("ServerInfo", string.Format("AutoMod: false"));
            rust.BroadcastChat("ServerInfo", string.Format("The Player:{0} is the that more playing Here", rust.GetAllNetUsers().FirstOrDefault(x => x.connectTime == rust.GetAllNetUsers().Max(t => t.connectTime)).displayName));
        }
    }
    class UserTeam
    {
        public ulong PlayerID { get; set; }
        public bool isTheOwner { get; set; }
        public string TAG { get; set; }
        public string Color { get; set; }
    }
}

