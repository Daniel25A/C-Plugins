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
    [Info("ServerLog", "Daniel25A", 0.2)]
    [Description("Mandar Log de Gives u otros")]

    class Log : RustLegacyPlugin
    {
        static String SysName = "ServerLog";
        void OnRunCommand(ConsoleSystem.Arg arg, bool reply)
        {
            NetUser Player = arg.argUser;
            String _Comando=arg.ArgsStr.ToLower().RemoveChars('"');
            if (Player == null) return;

            if (Player.admin)
            {
                if (_Comando.Contains("give"))
                {
                    rust.BroadcastChat(SysName, string.Format("The Administrator [color yellow] {0} [color white] Use the Command [color red]{1}", Player.displayName, rust.QuoteSafe(_Comando).Replace("/", "")));
                }
                if (_Comando.Contains("kit"))
                {
                    if (arg.Args[0] == null) return;
                    rust.BroadcastChat(SysName, string.Format("The Administrator [color yellow] {0} [color white] Use the Kit [color red]{1}", Player.displayName, arg.Args[0]));
                }
            }
        }

    }
}
