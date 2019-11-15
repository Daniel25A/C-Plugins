using System.Collections.Generic;
using System;
using System.Reflection;
using System.Data;
using UnityEngine;
using Oxide.Core;
using RustProto;
using System.Linq;
namespace Oxide.Plugins
{
    [Info("SurvivaBattle", "Daniel25A", "1.0.0", ResourceId = 001)]
    class SurvivaBattle : RustLegacyPlugin
    {
        static List<NetUser> PlayerInGame = new List<NetUser>();
        static List<NetUser> allplayers = new List<NetUser>();
        static Zone zona = new Zone(new Vector3(316f, 374f, 49f), 300);
        static Boolean EventoIniciado = false;
        static Boolean EventoEnJuego = false;
        static float MetrosADescontar = 50;
        static float TiempodeZona = 30f;
        static List<Vector3> posiciones = new List<Vector3>();
        static Timer Radiacion = null;
        static Timer CheckPlayer = null;
        static Dictionary<ulong, object> Inventarios = new Dictionary<ulong, object>();

        static RustServerManagement management;
        void Loaded()
        {
            management = RustServerManagement.Get();
            ReloadPositions();
        }
        void ReloadPositions()
        {
          
        }
        void CheckRadiacion()
        {
            if (EventoEnJuego == false) return;
            timer.Once(15, () => {
                    CheckRadiacion();
                    zona._Radio -= MetrosADescontar;
                    foreach (var x in rust.GetAllNetUsers())
                    {
                        rust.Notice(x, $"La Radiacion se encuentra a {zona._Radio} Metros");
                    }
                });
          }          
       
        void DescontarVida()
        {
            if (EventoEnJuego==false) return;
            timer.Once(3, () =>
            {
                DescontarVida();
                PlayerInGame.ForEach(x =>
                {
                    if (!IsPlayerInZone(x))
                    {
                        rust.Notice(x, "Estas Fuera de la Zona");
                        var Controller = x.playerClient.rootControllable;
                        var healController = Controller.GetComponent<TakeDamage>();
                        var vida = x.playerClient.rootControllable.rootCharacter.takeDamage.health - 5;
                        healController.health = vida;
                    }
                });
            });
        }
        bool IsPlayerInZone(NetUser Player)
        {
            bool InZone = false;
            Vector3 cachedPosition = Player.playerClient.rootControllable.transform.localPosition;
                if (Math.Round(Vector3.Distance(cachedPosition, zona.GetZone())) < Convert.ToDouble(zona.GetRadio()))
                {
                    InZone = true;
                }
            return InZone;
        }
         void OnKilled(TakeDamage takedamage, DamageEvent damage)
        {
            if(damage.attacker.client!=null && damage.victim.client != null)
            {
                var victima = damage.victim.client.netUser;
                var atacante = damage.attacker.client.netUser;
                if(PlayerInGame.Contains(atacante) && PlayerInGame.Contains(victima))
                {
                    PlayerInGame.Remove(victima);
                    rust.BroadcastChat("Server", $"{atacante.displayName} Mato a {victima.displayName} | {PlayerInGame.Count} Players en Juego");
                    if (PlayerInGame.Count <= 1)
                    {
                        rust.BroadcastChat("Server",$"{atacante.displayName} Gano el evento");
                        timer.Once(10f, () => Limpiar());
                    }
                }
            }
        }
        void Limpiar()
        {
            foreach (var x in allplayers)
            {
                try
                {
                    if (Inventarios.ContainsKey(x.userID))
                    {
                        ReturnPlayerInventory(x);
                        Inventarios.Remove(x.userID);
                    }
                }
                catch (Exception)
                {
                    Debug.Log("ERROR");
                }
            }
                PlayerInGame.Clear();
            if (CheckPlayer != null)
                CheckPlayer.Destroy();
            CheckPlayer = null;
            if (Radiacion != null)
                Radiacion.Destroy();
            Radiacion = null;
            EventoEnJuego = false;
            EventoIniciado = false;
            PlayerInGame.Clear();
            Inventarios.Clear();
            allplayers.Clear();
            zona._Radio = 300;
        }
        [ChatCommand("iniciar")]
        void cmdInitEvent(NetUser player, string command, string[] args)
        {
            if (player.admin == false) return;
            if (EventoEnJuego) return;
            rust.BroadcastChat("Server", "=============================");
            rust.BroadcastChat("Server", $"Evento Iniciado, GOGOGO!");
            rust.BroadcastChat("Server", "=============================");
            EventoEnJuego = true;
            zona._Radio = 300;
            CheckRadiacion();
            DescontarVida();
        }
        [ChatCommand("abrir")]
        void cmdOpenEvent(NetUser player, string command, string[] args)
        {
            if (player.admin == false) return;
            rust.BroadcastChat("Server", "=============================");
            rust.BroadcastChat("Server", $"{player.displayName} Inicio el Evento, Usar /entrar");
            rust.BroadcastChat("Server", "=============================");
            EventoIniciado = true;
        }
        void OnPlayerDeath(TakeDamage takedamage, DamageEvent damage)
        {
            if (damage.victim.client != null)
            {
                if (PlayerInGame.Contains(damage.victim.client.netUser))
                {
                    PlayerInGame.Remove(damage.victim.client.netUser);
                    rust.BroadcastChat("Server", $"{damage.victim.client.netUser.displayName} Murio | {PlayerInGame.Count} Players en Juego");
                    if (PlayerInGame.Count == 1)
                    {
                        var lastplayer = PlayerInGame.First();
                        rust.BroadcastChat("Server", $"{lastplayer.displayName} Gano el evento");
                        timer.Once(10f, () => Limpiar());
                    }
                }
            }
        }
        [ChatCommand("addspaw")]
        void cmdaddSpawn(NetUser player, string command, string[] args)
        {
            if (player.admin == false) return;
        }
        [ChatCommand("entrar")]
        void cmdJoinevent(NetUser player, string command, string[] args)
        {
            if (PlayerInGame.Contains(player)) return;
            if (!EventoIniciado) return;
            if (EventoEnJuego) return;
            if (PlayerInGame.Count > 10) return;
            PlayerInGame.Add(player);
            RecordInventoryPlayer(player);
            rust.BroadcastChat("Server", $"{player.displayName} Entro al evento, Players:{PlayerInGame.Count.ToString()}/10");
            allplayers.Add(player);
             management.TeleportPlayerToWorld(player.networkPlayer, new Vector3(316, 374, 49));
        }
        void RecordInventoryPlayer(NetUser Player)
        {
            if (Inventarios.ContainsKey(Player.userID)) return;
            var Ropas = new List<object>();
            var Armas = new List<object>();
            var CosasInventarios = new List<object>();
            var InfoInventario = new Dictionary<string, object>();
            IInventoryItem Item;
            Inventory PlayerInventory = Player.playerClient.rootControllable.idMain.GetComponent<Inventory>();
            try
            {
                for (int i = 0; i < 40; i++)
                {
                    if (PlayerInventory.GetItem(i, out Item))
                    {
                        if (i >= 0 && i < 30)
                        {
                            CosasInventarios.Add(new Dictionary<string, int>() { { Item.datablock.name, Item.datablock._splittable ? (int)Item.uses : 1 } });
                            continue;
                        }
                        if (i >= 30 && i < 36)
                        {
                            Armas.Add(new Dictionary<string, int>() { { Item.datablock.name, Item.datablock._splittable ? (int)Item.uses : 1 } });
                            continue;
                        }
                        Ropas.Add(new Dictionary<string, int>() { { Item.datablock.name, Item.datablock._splittable ? (int)Item.uses : 1 } });
                    }
                }
                InfoInventario.Add("Ropas", new List<object>(Ropas));
                InfoInventario.Add("Armas", new List<object>(Armas));
                InfoInventario.Add("CosasInventario", new List<object>(CosasInventarios));
                Inventarios.Add(Player.userID, new Dictionary<string, object>(InfoInventario));
                InfoInventario.Clear();
                Ropas.Clear();
                Armas.Clear();
                CosasInventarios.Clear();
                rust.Notice(Player, "Your Inventory Save, if you inventory dont return type /hg Inventory");
                PlayerInventory.Clear();
            }
            catch (Exception ex)
            {

                Debug.LogError("Error the record Player Inventory in Plugin HungerGames " + ex.Message);
            }
        }
        void ReturnPlayerInventory(NetUser Player)
        {
            if (!Inventarios.ContainsKey(Player.userID)) return;
            var InfoKeyValueInventory = Inventarios[Player.userID] as Dictionary<string, object>;
            Inventory PlayerInventory = Player.playerClient.rootControllable.idMain.GetComponent<Inventory>();
            Inventory.Slot.Preference SlopPreference;
            ItemDataBlock InventoryItem;
            try
            {
                var Ropa = InfoKeyValueInventory["Ropas"] as List<object>;
                var Armas = InfoKeyValueInventory["Armas"] as List<object>;
                var CosasInventario = InfoKeyValueInventory["CosasInventario"] as List<object>;
                PlayerInventory.Clear();
                if (Ropa.Count > 0)
                {
                    SlopPreference = Inventory.Slot.Preference.Define(Inventory.Slot.KindFlags.Armor, false, Inventory.Slot.KindFlags.Belt);
                    foreach (var x in Ropa)
                    {
                        var KeyValueItem = x as Dictionary<string, int>;
                        if (KeyValueItem != null)
                        {
                            InventoryItem = DatablockDictionary.GetByName(KeyValueItem.First().Key);
                            PlayerInventory.AddItemAmount(InventoryItem, KeyValueItem.First().Value, SlopPreference);
                        }
                    }
                }
                if (Armas.Count > 0)
                {
                    SlopPreference = Inventory.Slot.Preference.Define(Inventory.Slot.KindFlags.Belt, false, Inventory.Slot.KindFlags.Belt);
                    foreach (var x in Armas)
                    {
                        var KeyValueItem = x as Dictionary<string, int>;
                        if (KeyValueItem != null)
                        {
                            InventoryItem = DatablockDictionary.GetByName(KeyValueItem.First().Key);
                            PlayerInventory.AddItemAmount(InventoryItem, KeyValueItem.First().Value, SlopPreference);
                        }
                    }
                }
                if (CosasInventario.Count > 0)
                {
                    SlopPreference = Inventory.Slot.Preference.Define(Inventory.Slot.KindFlags.Default, false, Inventory.Slot.KindFlags.Belt);
                    foreach (var x in CosasInventario)
                    {
                        var KeyValueItem = x as Dictionary<string, int>;
                        if (KeyValueItem != null)
                        {
                            InventoryItem = DatablockDictionary.GetByName(KeyValueItem.First().Key);
                            PlayerInventory.AddItemAmount(InventoryItem, KeyValueItem.First().Value, SlopPreference);
                        }
                    }
                }
                rust.Notice(Player, "Your Inventory Retorned");
                Inventarios.Remove(Player.userID);
            }
            catch (Exception ex)
            {

                Debug.LogError("Error the return Player Inventory in Plugin HungerGames " + ex.Message);
            }
        }
        void CargarItems()
        {

        }
        class Zone
        {
           public float _Radio;
            Vector3 Posicion;
            public Zone(Vector3 CachedZone, float radio)
            {
                _Radio = radio;
                Posicion = CachedZone;
            }
            public Vector3 GetZone()
            {
                return Posicion;
            }
            public float GetRadio()
            {
                return _Radio;
            }
        }
    }
    
}