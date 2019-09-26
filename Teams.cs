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
        static Dictionary<ulong, Oxide.Plugins.Timer> Invitaciones = new Dictionary<ulong, Oxide.Plugins.Timer>();
        static Dictionary<ulong, bool> aprobaciones = new Dictionary<ulong, bool>();
        static Dictionary<ulong, UserTeam> ClanInvited = new Dictionary<ulong, UserTeam>();
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
                else if (TeamsPlayers.ContainsKey(User.userID))
                {
                    rust.Notice(Admin, "El player ya es owner de un clan");
                   
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
            if (TeamsPlayers.ContainsKey(PlayerToInvite.userID))
            {
                rust.SendChatMessage(Player, SysName, Red + "El Player Ya Esta en un TEAM");
                return;
            }
            if (aprobaciones.ContainsKey(PlayerToInvite.userID))
            {
                rust.SendChatMessage(Player, SysName, string.Format(Mensajes["alreadyinvite"], PlayerToInvite.displayName));
                return;
            }
            rust.SendChatMessage(Player, SysName, "Invitaste a " + PlayerToInvite.displayName + " A unirte a tu Team");
            rust.SendChatMessage(PlayerToInvite, SysName, Player.displayName + " te Invito a Unirte a un Clan, Tienes 15 Segundos para Responder" + Yellow + " /taccept");
            aprobaciones.Add(PlayerToInvite.userID, false);
            Invitaciones.Add(Player.userID, timer.Once(15f, () => AddUserToTeam(Player, PlayerToInvite)));
        }
        void AddUserToTeam(NetUser Owner, NetUser Invite)
        {
            ulong PlayerID = Owner.userID;
            NetUser UserInvite = Invite;
            NetUser UserSendInvite = Owner;
            if (aprobaciones[UserInvite.userID] == false)
            {
                rust.SendChatMessage(UserSendInvite, SysName, Mensajes["nojoin"]);
            }
            else
            {
                var _Config = TeamsPlayers.Values.FirstOrDefault(x => x.PlayerID == PlayerID);
                TeamsPlayers.Add(UserInvite.userID, new UserTeam() { PlayerID = UserInvite.userID, isTheOwner = false, TAG = _Config.TAG });
                rust.BroadcastChat(SysName, string.Format(Mensajes["jointeam"], UserInvite.displayName, _Config.TAG));
            }
            Invitaciones[PlayerID].Destroy();
            Invitaciones.Remove(PlayerID);
            aprobaciones.Remove(UserInvite.userID);
        }
        void LeaveUser()
        {

        }
        void TeamMSG(NetUser Player, String[] args)
        {
            if (TeamsPlayers.ContainsKey(Player.userID) == false)
            {
                rust.Notice(Player, "No tienes team");
                return;
            }
            if (args.Length == 0)
            {
                rust.SendChatMessage(Player, SysName, "use /tmsg (Mensaje)");
                return;
            }
            String _Name;
            String _MSG="";
            Boolean Owner=false;
            if (TeamsPlayers.Values.Where(x => x.PlayerID == Player.userID).FirstOrDefault().isTheOwner == true)
                Owner = true;
            else
                Owner = false;
            if (Owner)
            {
                _Name = "[Owner]" + Player.displayName;
                _MSG += Teal;
            }
            else
            {
                _Name = "[Player]" + Player.displayName;
                _MSG += Green;
            }
            foreach (var msg in args)
            {
                _MSG += msg + " ";
            }
            foreach (var xPlayer in TeamsPlayers.Values.Where(x => x.TAG == TeamsPlayers[Player.userID].TAG))
            {
                if (NetUser.FindByUserID(xPlayer.PlayerID) == null)
                    continue;
                rust.SendChatMessage(NetUser.FindByUserID(xPlayer.PlayerID), xPlayer.TAG, _Name + ":" + _MSG);
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
        [ChatCommand("tmsg")]
        void cmdsendMsg(NetUser netUser, string command, string[] args)
        {
            TeamMSG(netUser, args);
        }
        [ChatCommand("iteam")]
        void cmdadduser(NetUser netUser, string command, string[] args)
        {
            InviteUser(netUser, args);     
        }
        [ChatCommand("taccept")]
        void cmdaccept(NetUser netUser, string command, string[] args)
        {
            if (aprobaciones.ContainsKey(netUser.userID) == false)
            {
                rust.Notice(netUser, "No estas invitado a ningun team");
                return;
            }
            aprobaciones[netUser.userID] = true;
            rust.Notice(netUser, "Aguarde..");
        }
    }
    class UserTeam
    {
        public ulong PlayerID { get; set; }
        public bool isTheOwner { get; set; }
        public string TAG { get; set; }
    }
}

