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
    [Info("ProfessionalKIller", "Daniel25A", 0.2)]
    class Directoria : RustLegacyPlugin
    {
        private static Dictionary<ulong, PlayerData> PlayersStats = new Dictionary<ulong, PlayerData>();
        private static List<ulong> KillersOfTheServer = new List<ulong>();
        private static String SystemName = "WorlRustKD";
        static string Blue = "[color #0099FF]",
         Red = "[color #FF0000]",
         Pink = "[color #CC66FF]",
         Teal = "[color #00FFFF]",
         Green = "[color #009900]",
         Purple = "[color #6600CC]",
         White = "[color #FFFFFF]",
         Yellow = "[color #FFFF00]";
        void OnKilled(TakeDamage damage, DamageEvent evt)
        {
            if (evt.victim.client != null && evt.attacker.client!=null)
            {
                NetUser Victima = evt.victim.client.netUser, Killer = evt.attacker.client.netUser;
                if (!PlayersStats.ContainsKey(Victima.userID))
                    PlayersStats.Add(Victima.userID, new PlayerData(1, 1));
                else
                    PlayersStats[Victima.userID].AddDeathsToPlayer();
                if (!PlayersStats.ContainsKey(Killer.userID))
                    PlayersStats.Add(Killer.userID, new PlayerData(2, 1));
                else
                    PlayersStats[Killer.userID].AddKillToPlayer();
                rust.BroadcastChat(SystemName, string.Format(Red + "{0}" + White + " -> " + Yellow + "{1} " + White + "{2}" + Green + "KD", Killer.displayName
                    , Victima.displayName
                    , PlayersStats[Killer.userID].GetKDOfPlayer()));
                if (PlayersStats[Killer.userID].GetKDOfPlayer() >= 0.5)
                {
                    if(KillersOfTheServer.Contains(Killer.userID)) return;
                    foreach (var x in rust.GetAllNetUsers())
                    {
                        rust.Notice(x, string.Format("{0} Is ProfesionalKiller", Killer.displayName), "", 7f);
                    }
                    KillersOfTheServer.Add(Killer.userID);
                }
            }
        }

    }
    class PlayerData
    {
        private float kills;
        private float muertes;

        public float Muertes { get { return muertes; } set { muertes = value; } }
        public float Kills { get { return kills; } set { kills = value; } }
        public PlayerData(float kill =0 , float muerte = 0)
        {
            this.kills = kill;
            this.muertes = muerte;
        }
        public void AddKillToPlayer(float Kill = 1)
        {
            kills = Kills + 1;
        }
        public void AddDeathsToPlayer(float Death = 1)
        {
            muertes = muertes + Death;
        }
        public float GetKDOfPlayer()
        {
            float KD = 0;
            try
            {
                KD = kills * 10 / 100;
            }
            catch (DivideByZeroException)
            {
                KD = 0;
            }
            return KD;
        }
    }
}


//ProfessionalKIller