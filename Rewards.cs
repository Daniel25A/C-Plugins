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
    enum RewardsType{
       Primero,
       Segundo,
       Tercero,
       Cuarto
    }
    [Info("Rewards", "Daniel25A", 0.2)]
    [Description("Rewards Plugin for oxide")]
    class Rewards:RustLegacyPlugin
    {
        protected static String SystemName = "[Rewards]";
        static string Blue = "[color #0099FF]",
         Red = "[color #FF0000]",
         Pink = "[color #CC66FF]",
         Teal = "[color #00FFFF]",
         Green = "[color #009900]",
         Purple = "[color #6600CC]",
         White = "[color #FFFFFF]",
         Yellow = "[color #FFFF00]";
        protected static Dictionary<ulong, float> TiempoDeJugadoresEnElServer = new Dictionary<ulong, float>();
        void Loaded()
        {
            foreach (var x in rust.GetAllNetUsers())
            {
                if (TiempoDeJugadoresEnElServer.ContainsKey(x.userID) == false)
                    TiempoDeJugadoresEnElServer.Add(x.userID, 0);
            }
            CallTimer();
        }
        void CallTimer()
        {
            timer.Once(60f, () =>
            {
                foreach (var SPlayer in rust.GetAllNetUsers().ToList())
                {
                    if (TiempoDeJugadoresEnElServer.ContainsKey(SPlayer.userID) == false) continue;
                    TiempoDeJugadoresEnElServer[SPlayer.userID] += 1;
                    if (TiempoDeJugadoresEnElServer[SPlayer.userID] == 1)
                    {
                        GiveGiftToPlayer(SPlayer, RewardsType.Primero, TiempoDeJugadoresEnElServer[SPlayer.userID]);
                    }
                    if (TiempoDeJugadoresEnElServer[SPlayer.userID] == 5)
                    {
                        GiveGiftToPlayer(SPlayer, RewardsType.Segundo, TiempoDeJugadoresEnElServer[SPlayer.userID]);
                    }
                    if (TiempoDeJugadoresEnElServer[SPlayer.userID] == 45)
                    {
                        GiveGiftToPlayer(SPlayer, RewardsType.Tercero, TiempoDeJugadoresEnElServer[SPlayer.userID]);
                    }
                    if (TiempoDeJugadoresEnElServer[SPlayer.userID] == 60)
                    {
                        GiveGiftToPlayer(SPlayer, RewardsType.Cuarto, TiempoDeJugadoresEnElServer[SPlayer.userID]);
                    }
                }
                CallTimer();
            }
            );
        }
        void OnPlayerConnected(NetUser netUser)
        {
            if (TiempoDeJugadoresEnElServer.ContainsKey(netUser.userID) == false)
                TiempoDeJugadoresEnElServer.Add(netUser.userID, 0);
        }
        void OnPlayerDisconnected(uLink.NetworkPlayer networkPlayer)
        {
            NetUser PlayerDisconnect = networkPlayer.GetLocalData() as NetUser;
            if (TiempoDeJugadoresEnElServer.ContainsKey(PlayerDisconnect.userID) == true)
                TiempoDeJugadoresEnElServer.Remove(PlayerDisconnect.userID);
        }

        void GiveGiftToPlayer(NetUser Player,RewardsType GiftType,float TimePlaying)
        {
            switch (GiftType)
            {
                case RewardsType.Primero:
                    timer.Once(5f, () =>
                    {
                        rust.SendChatMessage(Player, SystemName, Green + "------- REWARDS Loading -------");
                        timer.Once(3f, () =>
                        {
                            rust.SendChatMessage(Player, SystemName, Green + "------- REWARDS Loading 50% -------");
                            timer.Once(2f, () =>
                            {
                                rust.SendChatMessage(Player, SystemName, Green + "------- REWARDS Loading 100% -------");
                                timer.Once(3f, () =>
                                {
                                    rust.SendChatMessage(Player, SystemName, Yellow + "------- REWARDS GIFT -------");
                                    rust.SendChatMessage(Player, SystemName, String.Format(White + "Hey" + Red + " {0} " + Green + "Thanks For Playing in our Server :,)", Player.displayName));
                                    rust.SendChatMessage(Player, SystemName, String.Format("{0}Our System Give You Any Items for Playing {1}{2} Minutes {3} in the Server <3", White, Yellow, TimePlaying, Red));
                                    Player.playerClient.rootControllable.GetComponent<Inventory>().AddItemAmount(DatablockDictionary.GetByName("9mm Ammo"), 15);
                                    Player.playerClient.rootControllable.GetComponent<Inventory>().AddItemAmount(DatablockDictionary.GetByName("Large Medkit"), 10);
                                    rust.SendChatMessage(Player, SystemName, Red + "------- REWARDS PLUGIN By Daniel25A -------");
                                });
                            });
                        });
                     });
            
                    break;
                case RewardsType.Segundo:
                    timer.Once(5f, () =>
                    {
                        rust.SendChatMessage(Player, SystemName, Green + "------- REWARDS Loading -------");
                        timer.Once(3f, () =>
                        {
                            rust.SendChatMessage(Player, SystemName, Green + "------- REWARDS Loading 50% -------");
                            timer.Once(2f, () =>
                            {
                                rust.SendChatMessage(Player, SystemName, Green + "------- REWARDS Loading 100% -------");
                                timer.Once(3f, () =>
                                {
                                    rust.SendChatMessage(Player, SystemName, Yellow + "------- REWARDS GIFT -------");
                                    rust.SendChatMessage(Player, SystemName, String.Format(White + "Hey" + Red + " {0} " + Green + "Thanks For Playing in our Server :,)", Player.displayName));
                                    rust.SendChatMessage(Player, SystemName, String.Format("{0}Our System Give You Any Items for Playing {1}{2} Minutes {3} in the Server <3", White, Yellow, TimePlaying, Red));
                                    Player.playerClient.rootControllable.GetComponent<Inventory>().AddItemAmount(DatablockDictionary.GetByName("9mm Ammo"), 250);
                                    rust.SendChatMessage(Player, SystemName, Red + "------- REWARDS PLUGIN By Daniel25A -------");
                                });
                            });
                        });
                    });
                    break;
                case RewardsType.Tercero:
                    timer.Once(5f, () =>
                    {
                        rust.SendChatMessage(Player, SystemName, Green + "------- REWARDS Loading -------");
                        timer.Once(3f, () =>
                        {
                            rust.SendChatMessage(Player, SystemName, Green + "------- REWARDS Loading 50% -------");
                            timer.Once(2f, () =>
                            {
                                rust.SendChatMessage(Player, SystemName, Green + "------- REWARDS Loading 100% -------");
                                timer.Once(3f, () =>
                                {
                                    rust.SendChatMessage(Player, SystemName, Yellow + "------- REWARDS GIFT -------");
                                    rust.SendChatMessage(Player, SystemName, String.Format(White + "Hey" + Red + " {0} " + Green + "Thanks For Playing in our Server :,)", Player.displayName));
                                    rust.SendChatMessage(Player, SystemName, String.Format("{0}Our System Give You Any Items for Playing {1}{2} Minutes {3} in the Server <3", White, Yellow, TimePlaying, Red));
                                    Player.playerClient.rootControllable.GetComponent<Inventory>().AddItemAmount(DatablockDictionary.GetByName("Supply Signal"), 1);
                                    rust.SendChatMessage(Player, SystemName, Red + "------- REWARDS PLUGIN By Daniel25A -------");
                                });
                            });
                        });
                    });
                    break;
                case RewardsType.Cuarto:
                    timer.Once(5f, () =>
                    {
                        rust.SendChatMessage(Player, SystemName, Green + "------- REWARDS Loading -------");
                        timer.Once(3f, () =>
                        {
                            rust.SendChatMessage(Player, SystemName, Green + "------- REWARDS Loading 50% -------");
                            timer.Once(2f, () =>
                            {
                                rust.SendChatMessage(Player, SystemName, Green + "------- REWARDS Loading 100% -------");
                                timer.Once(3f, () =>
                                {
                                    rust.SendChatMessage(Player, SystemName, Yellow + "------- REWARDS GIFT -------");
                                    rust.SendChatMessage(Player, SystemName, String.Format(White + "Hey" + Red + " {0} " + Green + "Thanks For Playing in our Server :,)", Player.displayName));
                                    rust.SendChatMessage(Player, SystemName, String.Format("{0}Our System Give You Any Items for Playing {1}{2} Minutes {3} in the Server <3", White, Yellow, TimePlaying, Red));
                                    Player.playerClient.rootControllable.GetComponent<Inventory>().AddItemAmount(DatablockDictionary.GetByName("M4"), 1);
                                    Player.playerClient.rootControllable.GetComponent<Inventory>().AddItemAmount(DatablockDictionary.GetByName("556 Ammo"), 250);
                                    rust.SendChatMessage(Player, SystemName, Red + "------- REWARDS PLUGIN By Daniel25A -------");
                                });
                            });
                        });
                    });
                    break;
            }
        }
    }
}