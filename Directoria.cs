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
    [Info("Directoria", "Daniel25A", 0.2)]
    class Directoria : RustLegacyPlugin
    {
        static string Prefix = "Direcotia :V";
        void SendMessageToServer(NetUser Player, String[] args)
        {
            String _Mensaje = "";
            if (Player.admin == false) return;
            foreach (var x in args)
                _Mensaje += x + " ";
            rust.BroadcastChat(Prefix, _Mensaje);
        }
        void ChangePrefix(NetUser Player, String NewPrefix)
        {
            if (Player.admin == false) return;
            Prefix = NewPrefix;
            rust.Notice(Player, "Done!");
        }
        [ChatCommand("enviar")]
        void SendMSGCommand(NetUser netUser, string command, string[] args)
        {
            SendMessageToServer(netUser, args);
        }
        [ChatCommand("ctag")]
        void ChangePrefixCommand(NetUser netUser, string command, string[] args)
        {
            if (args.Length == 0) return;
            ChangePrefix(netUser, args[0]);
        }
        [ChatCommand("cname")]
        void ChangeNameCommand(NetUser Player, string command, string[] args)
        {
            if (Player.admin == false) return;
            if (args.Length == 0) return;
            NetUser User = rust.GetAllNetUsers().Where(x => x.displayName.Contains(args[0])).FirstOrDefault();
            if (User == null) return;
            User.playerClient.name = args[1];
        }
    }
}


//Directoria