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
        static Dictionary<string, String> Mensajes = new Dictionary<string, string>();
        static Dictionary<ulong, System.Timers.Timer> Invitaciones = new Dictionary<ulong, System.Timers.Timer>();
        static Dictionary<ulong, bool> aprobaciones = new Dictionary<ulong, bool>();
        static string Blue = "[color #0099FF]",
                      Red = "[color #FF0000]",
                      Pink = "[color #CC66FF]",
                      Teal = "[color #00FFFF]",
                      Green = "[color #009900]",
                      Purple = "[color #6600CC]",
                      White = "[color #FFFFFF]",
                      Yellow = "[color #FFFF00]";
        void CreateTeam(NetUser Admin, String[] Args)
        {
            string _TeamName;
            string _Owner;
            if (!Admin.admin) return;
            if (Args.Length < 2)
            {
                rust.SendChatMessage(Admin, SysName, "Syntax Error, /cteam (TeamName) (Owner)");
            }
            else
            {
                _TeamName = Args[0].Trim();
                _Owner = Args[1];
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
                    });
                    rust.SendChatMessage(Admin, SysName, string.Format("Team Create Sucefully -> Owner {0}", User.displayName));
                }
            }
        }
        void Init()
        {
            Mensajes.Clear();
            Mensajes.Add("noteam", Red + "Tu no tienes un team ni eres owner de ninguno");
            Mensajes.Add("playerdontfind", White + "{0} " + Red + "No Existe");
            Mensajes.Add("jointeam", Yellow + "{0}" + White + " Ingreso al TEAM " + Blue + "{1}");
            Mensajes.Add("syntax", Blue + "Por Favor Usa -> " + Yellow + "/tinvite (Nombre del Player)");
            Mensajes.Add("alreadyinvite", Yellow + "{0} " + Blue + " Ya tiene una invitacion pendiente");
            Mensajes.Add("invite", Yellow + "{0} " + Green + "Te invito a unirte al TEAM " + Blue + "{1}");
            Mensajes.Add("nojoin", Green + "El Player no respondio a tu pedido");
        }
        void InviteUser(NetUser Player, String[] args)
        {
            if (TeamsPlayers.ContainsKey(Player.userID) == false)
            {
                rust.SendChatMessage(Player, SysName, Mensajes["noteam"]);
                return;
            }
            if (TeamsPlayers.Values.Where(x => x.PlayerID == Player.userID).FirstOrDefault().isTheOwner == false)
            {
                rust.SendChatMessage(Player, SysName, "Tu no eres el Owner del clan, no puedes realizar invitaciones");
                return;
            }
            if (args.Length == 0)
            {
                rust.SendChatMessage(Player, SysName, Mensajes["syntax"]);
                return;
            }
            NetUser PlayerToInvite = rust.GetAllNetUsers().FirstOrDefault(x => x.displayName.Contains(args[0]));
            if (PlayerToInvite == null)
            {
                rust.SendChatMessage(Player, SysName, string.Format(Mensajes["playerdontfind"], args[0]));
                return;
            }
            if (aprobaciones.ContainsKey(PlayerToInvite.userID))
            {
                rust.SendChatMessage(Player, SysName, string.Format(Mensajes["alreadyinvite"], PlayerToInvite.displayName));
                return;
            }
            aprobaciones.Add(PlayerToInvite.userID, false);
            Invitaciones.Add(Player.userID, new System.Timers.Timer()
            {

                AutoReset = false,
                Interval = 1000 * 15
            });
            Invitaciones[Player.userID].Elapsed += (p1, p2) => {
                ulong PlayerID = Player.userID;
                NetUser UserInvite = PlayerToInvite;
                NetUser UserSendInvite = Player;
                if (aprobaciones[UserInvite.userID] == false)
                {
                    rust.SendChatMessage(UserSendInvite, SysName, Mensajes["nojoin"]);
                }
                else
                {
                    var _Config = TeamsPlayers.Values.FirstOrDefault(x => x.PlayerID == PlayerID);
                    TeamsPlayers.Add(UserSendInvite.userID, new UserTeam() { PlayerID = UserSendInvite.userID, isTheOwner = false, TAG = _Config.TAG });
                    rust.BroadcastChat(SysName, string.Format(Mensajes["jointeam"], UserInvite.displayName, _Config.TAG));
                }
                Invitaciones.Remove(PlayerID);
                aprobaciones.Remove(UserSendInvite.userID);
                (p1 as System.Timers.Timer).Dispose();
            };
            Invitaciones[Player.userID].Start();
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
    }
}

