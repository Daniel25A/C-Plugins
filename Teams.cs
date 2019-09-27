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
        void LeaveUser(NetUser Player)
        {
            if (TeamsPlayers.ContainsKey(Player.userID) == false)
                return;
            rust.BroadcastChat(SysName, Green + Player.displayName + Green + " Salio del team " + Blue + TeamsPlayers[Player.userID].TAG);
            TeamsPlayers.Remove(Player.userID);
        }
        void CancelarInvitaciones(NetUser Player)
        {
            if (aprobaciones.ContainsKey(Player.userID))
                aprobaciones.Remove(Player.userID);
            rust.Notice(Player, "Cancelaste todos los pedidos de Team");

        }
        void DeleteTeam(NetUser administrator, String[] Args)
        {
            if (administrator.admin == false)
                return;
            if (Args.Length == 0)
            {
                rust.SendChatMessage(administrator, SysName, "Usa /deleteteam (TeamName)");
                return;
            }
            var TempList = TeamsPlayers.Values.Where(x => x.TAG.Contains(Args[0].Trim())).Select(x => x.PlayerID).ToList<ulong>();
            foreach (var x in TempList)
            {
                TeamsPlayers.Remove(x);
            }
            rust.Notice(administrator, "Done !");
            TempList.Clear();
        }
        void TeamInfo(NetUser Player, String[] Args)
        {
            if (Args.Length == 0)
                return;
            if (!TeamsPlayers.Values.Any(x => x.TAG.Contains(Args[0].Trim())))
            {
                rust.Notice(Player, "no existe el TEAM");
                return;
            }
            UserTeam Team = TeamsPlayers.Values.FirstOrDefault(x => x.TAG.Contains(Args[0]) && x.isTheOwner==true);
            rust.SendChatMessage(Player, SysName, Yellow + "Nombre del Team: " + Blue + Team.TAG);
            rust.SendChatMessage(Player, SysName, Yellow + "ID Del Owner: " + Blue + Team.PlayerID);
            rust.SendChatMessage(Player, SysName, Yellow + "Integrantes: " + Blue+ TeamsPlayers.Values.Where(x => x.TAG.Contains(Args[0])).Count().ToString());
        }
        void ExpulsarMiembro(NetUser Player, string[] args)
        {
            if (TeamsPlayers.ContainsKey(Player.userID) == false)
            {
                rust.SendChatMessage(Player, SysName, Red+"No tienes un team");
                return;
            }
            if (args.Length == 0)
            {
                rust.SendChatMessage(Player, SysName, "Use /tkick (PlayerName)");
                return;
            }
            var User = rust.GetAllNetUsers().ToList().FirstOrDefault(x => x.displayName.Contains(args[0]));
            if (User == null)
            {
                rust.Notice(Player, "Este Miembro no existe en el server");
                return;
            }
            if (TeamsPlayers.Values.Where(x => x.PlayerID == User.userID).FirstOrDefault().isTheOwner == true)
            {
                rust.SendChatMessage(Player, SysName, "No puedes salir de tu propio Team, pidele a un administrador que lo elimine");
                return;
            }
            if (TeamsPlayers.ContainsKey(User.userID) == false)
            {
                rust.Notice(Player, "Este Miembro no forma parte de ningun Clan");
                return;
            }
            if (TeamsPlayers.Values.FirstOrDefault(x => x.PlayerID == User.userID).TAG != TeamsPlayers.Values.FirstOrDefault(x2 => x2.PlayerID == Player.userID).TAG)
            {
                rust.Notice(Player, "Este Miebro no es parte de tu Team");
                return;
            }
            TeamsPlayers.Remove(User.userID);
            rust.BroadcastChat(SysName, User.displayName + Green + "Fue Expulsado de Su team.." + Yellow + " Este solo, Reclutenlo..");

        }
        object ModifyDamage(TakeDamage takedamage, DamageEvent damage)
		{
            if (damage.attacker.client == null || damage.victim.client == null)
                return null;
            NetUser Atacante = damage.attacker.client.netUser;
            NetUser Victima = damage.victim.client.netUser;
            if (TeamsPlayers.ContainsKey(Atacante.userID) && TeamsPlayers.ContainsKey(Victima.userID))
            {
                UserTeam CheckAttack = TeamsPlayers.Values.FirstOrDefault(x => x.PlayerID == Victima.userID),
                         CheckVictim = TeamsPlayers.Values.FirstOrDefault(x => x.PlayerID == Atacante.userID);
                if (CheckAttack.PlayerID == CheckVictim.PlayerID)
                    return null;
                if (CheckAttack.TAG == CheckVictim.TAG)
                {
                    rust.Notice(Atacante, "Stop, es de tu clan");
                   return  CancelDamage(damage);
                }
            }
            return null;
		}
        object CancelDamage(DamageEvent damage)
        {
            damage.amount = 0f;
            damage.status = LifeStatus.IsAlive;
            return damage;
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
        [ChatCommand("tcancel")]
        void cmdcancelar(NetUser netUser, string command, string[] args)
        {
            CancelarInvitaciones(netUser);
        }
        [ChatCommand("cteam")]
        void cmdCreateTeam(NetUser netUser, string command, string[] args)
        {
            CreateTeam(netUser, args);
        }
        [ChatCommand("e")]
        void cmdsendMsg(NetUser netUser, string command, string[] args)
        {
            TeamMSG(netUser, args);
        }
        [ChatCommand("iteam")]
        void cmdadduser(NetUser netUser, string command, string[] args)
        {
            InviteUser(netUser, args);     
        }
        [ChatCommand("tkick")]
        void cmdkick(NetUser netUser, string command, string[] args)
        {
            ExpulsarMiembro(netUser, args);
        }
        [ChatCommand("tleave")]
        void cmdleave(NetUser netUser, string command, string[] args)
        {
            LeaveUser(netUser);
        }
        [ChatCommand("tdelete")]
        void cmddeleteTeam(NetUser netUser, string command, string[] args)
        {
            DeleteTeam(netUser, args);
        }
        [ChatCommand("tinfo")]
        void cmdTeamInfo(NetUser netUser, string command, string[] args)
        {
            TeamInfo(netUser, args);
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

