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
    [Info("WorlVoteDay", "Daniel25A", 0.2)]
    [Description("VoteDay Exclusivo para el server")]

    class WorlVoteDay : RustLegacyPlugin
    {
        static bool VotedayOpen = false;
        static string SysName = "WorldVoteDay";
        static int votedia = 0;
        static int votenoche = 0;
        static List<ulong> PlayersVote = new List<ulong>();
        void Loaded()
        {
            if (permission.PermissionExists("canopenvoteday", this) == false)
                permission.RegisterPermission("canopenvoteday", this);
        }
        void OpenVoteDay(NetUser Player)
        {
            if (Player.admin == false && permission.UserHasPermission(Player.userID.ToString(), "canopenvoteday") == false) return;
            rust.BroadcastChat(SysName, string.Format("[color yellow]{0} [color white] Abrio la votacion",Player.displayName));
            rust.GetAllNetUsers().ToList().ForEach(x => rust.Notice(x,"Voteday Open -> Use /vote dia or noche"));
            VotedayOpen = true;
            timer.Once(15f, () =>
            {
                if (votedia > votenoche)
                    rust.RunServerCommand("env.time 6");
                else
                    rust.GetAllNetUsers().ToList().ForEach(x => rust.Notice(x, "Se quedara de noche, cuidado los vichos :3"));
                ResetVoteDay();
            }
           );
        }
        void ResetVoteDay()
        {
            votedia = 0;
            votenoche = 0;
            VotedayOpen = false;
            PlayersVote.Clear();
        }
        void CheckVote(NetUser Player,string voteoption)
        {
            if (!VotedayOpen) return;
            if (PlayersVote.Contains(Player.userID)) return;
            if (voteoption != "dia" && voteoption != "noche") return;
            if (voteoption == "dia")
                votedia++;
            else
                votenoche++;
            PlayersVote.Add(Player.userID);
            rust.BroadcastChat(SysName, string.Format("[color red]{0} [color white] Voto para {1}, Votos Dia:[color green]{2} [color white]Votos Noche:[color green]{3}", Player.displayName, voteoption, votedia, votenoche));
        }
        [ChatCommand("openvoteday")]
        void cmdopen(NetUser netUser, string command, string[] args)
        {
            OpenVoteDay(netUser);
        }
        [ChatCommand("vote")]
        void cmdvotar(NetUser netUser, string command, string[] args)
        {
            if (args.Length == 0)
            {
                rust.SendChatMessage(netUser, SysName, "/vote dia |  noche");
                return;
            }
            CheckVote(netUser, args[0]);
        }
    }
}