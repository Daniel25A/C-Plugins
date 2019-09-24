using System;
using Colecciones= System.Collections.Generic;
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
    [Info("ChatSystem", "Daniel25A", 0.2)]
    [Description("ChatSystem en Grupo")]

    class ChatSystem : RustLegacyPlugin
    {
        Colecciones::List<NetUser> Players = new Colecciones.List<NetUser>();
        static string TAG = "ChatSystem";
        void AgregarPlayeralChat(NetUser Player,string Name)
        {
            if (!Player.admin)
            {
                rust.Notice(Player, "No Puedes Usar este Comando");
                return;
            }
            NetUser targeUser = rust.GetAllNetUsers().Where(x => x.displayName.ToLower().Contains(Name.ToLower())).FirstOrDefault();
            if (targeUser != null) {
                if (Players.Contains(targeUser))
                {
                    rust.SendChatMessage(Player, TAG, "Ya Esta el Player en el chat");
                    return;
                }
                rust.Notice(targeUser, "Se te Agrego al Chat Grupal");
                Players.Add(targeUser);
            }
            else
            {
                rust.SendChatMessage(Player, "ChatSystem", "No Existe el Player");
            }
        }
        void EliminarUserChat(NetUser Player,string Name)
        {
            if (!Player.admin) return;
            NetUser targeUser = rust.GetAllNetUsers().Where(x => x.displayName.ToLower().Contains(Name.ToLower())).FirstOrDefault();
            if (targeUser != null)
            {
                if (Players.Contains(targeUser))
                {
                    Players.Remove(targeUser);
                    rust.Notice(targeUser, "Fuiste Eliminado");
                }
                else
                {
                    rust.Notice(Player, "No Existe el Player");
                }
            }
        }
        [ChatCommand("adduser")]
        void cmdAddUserChat(NetUser Player, string command, string[] args)
        {
            if (args.Length == 0)
                return;
            AgregarPlayeralChat(Player, args[0]);
        }
        [ChatCommand("sendmsg")]
        void cmdSendChat(NetUser Player, string command, string[] args)
        {
            string mensaje = "";

            if (args.Length == 0)
                return;
            foreach (string Msg in args)
            {
                mensaje += Msg + " ";
            }
            if (Players.Contains(Player) || Player.admin)
            {
                foreach (var x in rust.GetAllNetUsers())
                {
                    if (Players.Contains(x))
                    {
                        rust.SendChatMessage(x, TAG, (Player.admin ? "(Staff)" : "(ChatUser)") + Player.displayName + "=>[Color Green]" + mensaje);
                    }
                }
            }
        }
        [ChatCommand("deleteuser")]
        void cmdDeleteUser(NetUser Player, string command, string[] args)
        {
            if (args.Length == 0) return;
            EliminarUserChat(Player, args[0]);
        }
    }
}

