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
    [Info("AutoMod", "Daniel25A", 0.2)]
    [Description("Agregar Moderadores AutoMaticamente")]

    class AutoMod : RustLegacyPlugin
    {
        //System Name
        internal static String SysName = "AutoModServer";
        internal static String[] Permissions = { "mod_chat",
                                                   "administration.cankick",
                                                   "administration.canban",
                                                   "administration.canmute",
                                                   "ServerTest"
                                               };
        internal static List<ulong> ModeratorsList = new List<ulong>();
        internal static List<ulong> ModeratorDeleteList = new List<ulong>();
        static string Blue = "[color #0099FF]",
                      Red = "[color #FF0000]",
                      Pink = "[color #CC66FF]",
                      Teal = "[color #00FFFF]",
                      Green = "[color #009900]",
                      Purple = "[color #6600CC]",
                      White = "[color #FFFFFF]",
                      Yellow = "[color #FFFF00]";
        void AutoModCommand(NetUser Staff, String[] args)
        {
            if (Staff.admin == false) {
                rust.Notice(Staff, "You dont are administrator");
                return;
            }
            if (args.Length == 0)
            {
                MostrarMensajes(Staff);
                return;
            }
            switch (args[0])
            {
                case "add":
                    if (args.Length < 2) {
                        MostrarMensajes(Staff);
                        return;
                    }
                    try
                    {
                        ulong PlayerID = ulong.Parse(args[1]);
                        ModeratorsList.Add(PlayerID);
                        rust.Notice(Staff, "Moderator Add, wait the user connected or reconnected");
                        rust.BroadcastChat(SysName, string.Format("The Administrator " + Teal + "{0}" + White + " add a new moderator to the server" + Green + "({1})", Staff.displayName, PlayerID));

                    }
                    catch (FormatException)
                    {
                        rust.SendChatMessage(Staff,SysName, Green+"Por Favor, Ingrese una SteamID Valida");
                    }
                    break;
                case "clear":
                    ModeratorsList.Clear();
                    rust.Notice(Staff, "Done !");
                    break;
                default:
                    MostrarMensajes(Staff);
                    break;
            }
        }
        void OnPlayerConnected(NetUser netUser)
        {
            if (ModeratorsList.Contains(netUser.userID))
            {
                foreach (var per in Permissions)
                {
                    if (!permission.UserHasPermission(netUser.userID.ToString(), per))
                        rust.RunServerCommand(string.Format("oxide.grant user {0} {1}", netUser.displayName, per));
                }
                rust.Notice(netUser, "Hey Bro, Welcome, You are a new Moderator :D");
                ModeratorsList.Remove(netUser.userID);
            }
        }
        void MostrarMensajes(NetUser Player)
        {
            rust.SendChatMessage(Player, SysName, Green + "AutoMod System By: " + Blue + "Daniel25A");
            rust.SendChatMessage(Player, SysName, Green + "/mod add (Client ID)" + Yellow + " Add a New Moderator, Check if the ip is Correct");
            rust.SendChatMessage(Player, SysName, Green + "/mod clear" + Yellow + " Delete the Moderator List");
        }
        [ChatCommand("mod")]
        void CmdAutoMod(NetUser netUser, string command, string[] args)
        {
            AutoModCommand(netUser, args);
        }
    }
}


//AutoMod