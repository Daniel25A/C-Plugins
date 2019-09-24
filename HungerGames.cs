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
    [Info("HungerGames", "Daniel25A", 0.2)]
    [Description("Juegos del Hambre")]

    class HungerGames : RustLegacyPlugin
    {
        enum OptionFroze
        {
            PlayerFroze,
            PlayerunFroze
        }
        public class PlayerStatus : global::UnityEngine.MonoBehaviour
        {
            public PlayerClient Player;
            public int Kills = 0;
            public bool InGame = false;
            public bool Whiner = false;
            void Start()
            {
                this.Player = GetComponent<PlayerClient>();
            }
        }

        StructureComponent paredMadera;
         Collider AtributeCollider;
         MeshBatchInstance AtributeInstance;
         RaycastHit HitRayCastCharacter;
         Ray AtributeRayCharacter;
         bool AtributeBolean;
         RustServerManagement ServerController;
        static Vector3? cachedLocationEvent=null;
        static FieldInfo StructureComponents;
        static bool EventoIniciado = false;
        static bool EventoEncendido = false;
        static bool ForceStartEvent = false;
        static string SysName = "HungerGames";
        static NetUser PlayerExecuteCommand = null;
        static int MaximoPlayers = 20; //Poner el Maximo de Players Aqui
        static float TiempodeInicio = 60f; //Poner el Tiempo de Inicio una vez forzado o alcanzado el Maximo de Players
        static float RadiationDistance = 50f;
        static string Blue = "[color #0099FF]",
                 Red = "[color #FF0000]",
                 Pink = "[color #CC66FF]",
                 Teal = "[color #00FFFF]",
                 Green = "[color #009900]",
                 Purple = "[color #6600CC]",
                 White = "[color #FFFFFF]",
                 Yellow = "[color #FFFF00]";
        static Colecciones::Dictionary<ulong, Vector3> PlayersPostions = new Colecciones.Dictionary<ulong, Vector3>();
        static Colecciones::Dictionary<string, object> Mensajes = new Colecciones.Dictionary<string, object>();
        static Colecciones::List<NetUser> JugadoresenJuego = new Colecciones.List<NetUser>();
        static Colecciones::List<NetUser> Players2 = new Colecciones.List<NetUser>();
        static Colecciones::Dictionary<int, Colecciones::Dictionary<int, string>> RamdomItems = new Colecciones.Dictionary<int, Colecciones::Dictionary<int, string>>() { 
        {0, new Colecciones::Dictionary<int,string>(){{1,"Pipe Shotgun"}}},
        {1,new Colecciones::Dictionary<int,string>(){{1,"Revolver"}}},
        {2,new Colecciones::Dictionary<int,string>(){{2,"Large Medkit"}}},
        {3,new Colecciones::Dictionary<int,string>(){{1,"Pick Axe"}}},
        {4,new Colecciones::Dictionary<int,string>(){{7,"Handmade Shells"}}},
        {5,new Colecciones::Dictionary<int,string>(){{15,"9mm Ammo"}}}
        };
        static Colecciones::Dictionary<ulong, object> Inventarios = new Colecciones.Dictionary<ulong, object>();
        void Init()
        {
            Mensajes.Add("EntroNormal", Red + "HungerGames:" + White + "{0}/{1}" + Green + "players are waiting. " + White + "/hg join");
            Mensajes.Add("Titulo", Red + "---------------HungerGames---------------");
            Mensajes.Add("EventoActivadoInfo", Red + "-----------HungerGames is Active---------");
            Mensajes.Add("Footer", Red + "-----------------------------------------");
            Mensajes.Add("InicioForzado", Green + "HungerGames force started!");
            Mensajes.Add("EntroForzado", Red + "EVENT STARTING" + Green + " Players: {0}" + White + "/hg join");
            Mensajes.Add("PlayerFroze", Red + "You froze!");
            Mensajes.Add("PlayerunFroze", Red + "You are now free");
            Mensajes.Add("PlayerenJuego", White + "You joined the game!");
            Mensajes.Add("EventoActivado", Green + "You can join the EVENT using " + Blue + "/hg join");
            Mensajes.Add("Informacion", Green + "Type /hg to know more!");
        }
        void RecordInventoryPlayer(NetUser Player){
            if (Inventarios.ContainsKey(Player.userID)) return;
            var Ropas = new Colecciones::List<object>();
            var Armas = new Colecciones::List<object>();
            var CosasInventarios = new Colecciones::List<object>();
            var InfoInventario = new Colecciones::Dictionary<string, object>();
            IInventoryItem Item;
            Inventory PlayerInventory = Player.playerClient.rootControllable.idMain.GetComponent<Inventory>();
            try
            {
                for (int i = 0; i < 40; i++)
                {
                    if (PlayerInventory.GetItem(i, out Item)) {
                        if (i >= 0 && i < 30) {
                            CosasInventarios.Add(new Colecciones::Dictionary<string, int>() { { Item.datablock.name, Item.datablock._splittable ? (int)Item.uses : 1 } });
                            continue;
                        }
                        if (i >= 30 && i < 36) {
                            Armas.Add(new Colecciones::Dictionary<string, int>() { { Item.datablock.name, Item.datablock._splittable ? (int)Item.uses : 1 } });
                            continue;
                        }
                        Ropas.Add(new Colecciones::Dictionary<string, int>() { { Item.datablock.name, Item.datablock._splittable ? (int)Item.uses : 1 } });
                    }
                }
                InfoInventario.Add("Ropas", new Colecciones::List<object>(Ropas));
                InfoInventario.Add("Armas", new Colecciones::List<object>(Armas));
                InfoInventario.Add("CosasInventario", new Colecciones::List<object>(CosasInventarios));
                Inventarios.Add(Player.userID, new Colecciones::Dictionary<string, object>(InfoInventario));
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
        void ForceFinally(NetUser Player)
        {
            if (Player.admin == false) return;
            if (JugadoresenJuego.Count > 0 && JugadoresenJuego.Count==1)
            {
                FinalizarEvento(JugadoresenJuego.FirstOrDefault());
            }
            foreach (var x in Players2)
            {
                if (Inventarios.ContainsKey(x.userID)) ReturnPlayerInventory(x);
                if (PlayersPostions.ContainsKey(x.userID)) {
                    ServerController.TeleportPlayerToWorld(x.networkPlayer, PlayersPostions[x.userID]);
                    PlayersPostions.Remove(x.userID);
                }
            }
            ResetEvent();
        }
        void ReturnPlayerInventory(NetUser Player) {
            if (!Inventarios.ContainsKey(Player.userID)) return;
            var InfoKeyValueInventory = Inventarios[Player.userID] as Colecciones::Dictionary<string, object>;
            Inventory PlayerInventory = Player.playerClient.rootControllable.idMain.GetComponent<Inventory>();
            Inventory.Slot.Preference SlopPreference;
            ItemDataBlock InventoryItem;
            try
            {
                var Ropa = InfoKeyValueInventory["Ropas"] as Colecciones::List<object>;
                var Armas = InfoKeyValueInventory["Armas"] as Colecciones::List<object>;
                var CosasInventario = InfoKeyValueInventory["CosasInventario"] as Colecciones::List<object>;
                PlayerInventory.Clear();
                if (Ropa.Count > 0)
                {
                    SlopPreference = Inventory.Slot.Preference.Define(Inventory.Slot.KindFlags.Armor, false, Inventory.Slot.KindFlags.Belt);
                    foreach (var x in Ropa)
                    {
                        var KeyValueItem = x as Colecciones::Dictionary<string, int>;
                        if (KeyValueItem != null) {
                            InventoryItem = DatablockDictionary.GetByName(KeyValueItem.First().Key);
                            PlayerInventory.AddItemAmount(InventoryItem, KeyValueItem.First().Value,SlopPreference);
                        }
                    }
                }
                if (Armas.Count > 0)
                {
                    SlopPreference = Inventory.Slot.Preference.Define(Inventory.Slot.KindFlags.Belt, false, Inventory.Slot.KindFlags.Belt);
                    foreach (var x in Armas)
                    {
                        var KeyValueItem = x as Colecciones::Dictionary<string, int>;
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
                        var KeyValueItem = x as Colecciones::Dictionary<string, int>;
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
        void OnServerInitialized()
        {
            StructureComponents = typeof(StructureMaster).GetField("_structureComponents", (BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.NonPublic));
            ServerController = RustServerManagement.Get();
        }
        void Loaded()
        {
            Debug.Log("Cargado con Exito");
        }
        void ResetEvent()
        {
            PlayerExecuteCommand = null;
            EventoIniciado = false;
            ForceStartEvent = false;
            EventoEncendido = false;
            paredMadera = null;
            AtributeBolean = false;
            AtributeCollider = null;
            AtributeInstance = null;
            AtributeRayCharacter.direction = new Vector3(float.NaN, float.NaN, float.NaN);
            foreach (NetUser UserGame in JugadoresenJuego.Where(x => x.playerClient.GetComponent<PlayerStatus>() != null))
                GameObject.Destroy(UserGame.playerClient.GetComponent<PlayerStatus>());
            JugadoresenJuego.Clear();
            Players2.Clear();
            if (PlayersPostions.Count == 0)
                PlayersPostions.Clear();
        }
        void PlayerFreezer(NetUser Player, OptionFroze Opcion, bool SendMensaje)
        {
            if (Player == null)
                return;
            if (Opcion == OptionFroze.PlayerFroze) { 
                ConsoleNetworker.SendClientCommand(Player.networkPlayer, "input.bind Up 7 None");
                ConsoleNetworker.SendClientCommand(Player.networkPlayer, "input.bind Down 7 None");
                ConsoleNetworker.SendClientCommand(Player.networkPlayer, "input.bind Left 7 None");
                ConsoleNetworker.SendClientCommand(Player.networkPlayer, "input.bind Right 7 None");
                ConsoleNetworker.SendClientCommand(Player.networkPlayer, "input.bind Sprint 7 None");
                ConsoleNetworker.SendClientCommand(Player.networkPlayer, "input.bind Duck 7 None");
                ConsoleNetworker.SendClientCommand(Player.networkPlayer, "input.bind Jump 7 None");
                ConsoleNetworker.SendClientCommand(Player.networkPlayer, "input.bind Fire 7 None");
                if (SendMensaje)
                    rust.SendChatMessage(Player, SysName, Mensajes["PlayerFroze"].ToString());
            }
            else if (Opcion == OptionFroze.PlayerunFroze) {
                ConsoleNetworker.SendClientCommand(Player.networkPlayer, "input.bind Up W UpArrow");
                ConsoleNetworker.SendClientCommand(Player.networkPlayer, "input.bind Down S DownArrow");
                ConsoleNetworker.SendClientCommand(Player.networkPlayer, "input.bind Right D RightArrow");
                ConsoleNetworker.SendClientCommand(Player.networkPlayer, "input.bind Sprint LeftShift RightShift");
                ConsoleNetworker.SendClientCommand(Player.networkPlayer, "input.bind Left A LeftArrow");
                ConsoleNetworker.SendClientCommand(Player.networkPlayer, "input.bind Duck LeftControl RightControl");
                ConsoleNetworker.SendClientCommand(Player.networkPlayer, "input.bind Jump Space None");
                ConsoleNetworker.SendClientCommand(Player.networkPlayer, "input.bind Fire Mouse0 None");
                if (SendMensaje)
                    rust.SendChatMessage(Player, SysName, Mensajes["PlayerunFroze"].ToString());
            }
        }
        void EntraralJuego(NetUser Player)
        {
            if (JugadoresenJuego.Count >= MaximoPlayers) {
                rust.SendChatMessage(Player, SysName, Green + "HungerGames have the max of " + Red + "players " + White + "please wait that the finally of current event");
                return;
            }
            if (EventoIniciado==true) {
                rust.SendChatMessage(Player, SysName, Green + "HungerGames already started " + Red + "please wait that the event finally");
                return;
            }
            if (EventoEncendido == false) { rust.SendChatMessage(Player, SysName, "Hunger Games no esta Iniciado"); return; }
            if (JugadoresenJuego.Contains(Player) == true)
            {
                rust.SendChatMessage(Player, SysName, Green + " You Already in Game");
                return;
            }
            if (Player.playerClient.GetComponent<PlayerStatus>() == null)
                Player.playerClient.gameObject.AddComponent<PlayerStatus>();
            if (Player.playerClient.GetComponent<PlayerStatus>().InGame)
                return;
            JugadoresenJuego.Add(Player);
            Players2.Add(Player);
            RecordInventoryPlayer(Player);
            PlayersPostions.Add(Player.userID, Player.playerClient.rootControllable.transform.localPosition);
            ServerController.TeleportPlayerToWorld(Player.networkPlayer, cachedLocationEvent ?? new Vector3(Player.playerClient.transform.localPosition.x, Player.playerClient.transform.localPosition.y, Player.playerClient.transform.localPosition.z));
            if (ForceStartEvent)
                rust.BroadcastChat(SysName, string.Format(Mensajes["EntroForzado"].ToString(), JugadoresenJuego.Count));
            else
                rust.BroadcastChat(SysName, string.Format(Mensajes["EntroNormal"].ToString(), JugadoresenJuego.Count, MaximoPlayers));
           timer.Once(1f, () =>
           {
                PlayerFreezer(Player, OptionFroze.PlayerFroze, true);
           });
           timer.Once(3f, () =>
           {
               PlayerFreezer(Player, OptionFroze.PlayerunFroze, true);
           });

        }
        void OnKilled(TakeDamage damage, DamageEvent evt)
        {
            if (evt.victim.client != null)
            {
                if (JugadoresenJuego.Contains(evt.victim.client.netUser))
                {
                    JugadoresenJuego.Remove(evt.victim.client.netUser);
                    rust.BroadcastChat(SysName, Yellow + evt.victim.client.netUser.displayName + Red + " out" + White + " " + JugadoresenJuego.Count.ToString() + Blue + " Players in Game");
                    if (evt.attacker.client != null)
                    {
                        if (JugadoresenJuego.Contains(evt.attacker.client.netUser))
                        {
                            evt.attacker.client.netUser.playerClient.GetComponent<PlayerStatus>().Kills++;
                        }
                    }
                    if (JugadoresenJuego.Count == 1)
                    {
                        FinalizarEvento(JugadoresenJuego.FirstOrDefault());
                    }
                }
            }
        }
 /*       void OnPlayerSpawn(PlayerClient client, bool usecamp, RustProto.Avatar avatar)
        {
            if (client == null) return;
            if (Players2.Contains(client.netUser))
            {
                if (Inventarios.ContainsKey(client.netUser.userID)) ReturnPlayerInventory(client.netUser);
                if (PlayersPostions.ContainsKey(client.netUser.userID))
                {
                    ServerController.TeleportPlayerToWorld(client.netUser.networkPlayer, PlayersPostions[client.netUser.userID]);
                    PlayersPostions.Remove(client.netUser.userID);
                }
            }
        }*/

        void ForceStart(NetUser Player)
        {
            if (Player.admin == false) { rust.Notice(Player, "No Puedes usar este comando"); return; }
            if (!EventoEncendido) { rust.SendChatMessage(Player, SysName, "Hunger Games no Esta Iniciado"); return; }
            if (ForceStartEvent) return;
            rust.BroadcastChat(SysName, Mensajes["InicioForzado"].ToString());
            rust.BroadcastChat(SysName, Green + "Loaded 1%");
            rust.BroadcastChat(SysName, Green + "Loaded 15%");
            timer.Once(3f, () =>
            {
                rust.BroadcastChat(SysName, Green + "Loaded 20%");
                rust.BroadcastChat(SysName, Green + "Loaded 50%");
            });
            timer.Once(5f, () =>
            {
                rust.BroadcastChat(SysName, Green + "Loaded 75%");
                rust.BroadcastChat(SysName, Green + "Loaded 85%");
                rust.BroadcastChat(SysName, Green + "Loaded 100%");
                ForceStartEvent = true;
                rust.BroadcastChat(SysName, Yellow + "HungerGames Started in 30 Seconds");
                timer.Once(30f, () =>
                {
                    BuscarParedes(Player);
                    rust.BroadcastChat(SysName, Mensajes["Titulo"].ToString());
                    rust.BroadcastChat(SysName, Green + "Good Luck" + Yellow + " NO TEAMS" + Red + " Try Whinn");
                    rust.BroadcastChat(SysName, Mensajes["Footer"].ToString());
                    EventoIniciado = true;
                    ForceStartEvent = true;
                });
            });
        }
        void IniciarEvento(NetUser Player)
        {
            if (!Player.admin)
                return;
            if (EventoEncendido)
            {
                rust.SendChatMessage(Player, SysName, Green + "HungerGames already started " + Red + "wait Please");
                return;
            }
            if (cachedLocationEvent == null) {
                rust.SendChatMessage(Player, SysName, Green + "Please Insert the Location where start the event" + Yellow + " /config setlocation");
                return;
            }
            EventoEncendido = true;
            rust.BroadcastChat(SysName, Mensajes["EventoActivadoInfo"].ToString());
            rust.BroadcastChat(SysName, Mensajes["EventoActivado"].ToString());
            rust.BroadcastChat(SysName, Mensajes["Informacion"].ToString());
            rust.BroadcastChat(SysName, Mensajes["Footer"].ToString());
        }
        void BuscarParedes(NetUser Player)
        {
            if (!JugadoresenJuego.Contains(Player) && Player!=PlayerExecuteCommand) return;
            AtributeRayCharacter = Player.playerClient.rootControllable.idMain.GetComponent<Character>().eyesRay;
            if (MeshBatchPhysics.Raycast(AtributeRayCharacter, out HitRayCastCharacter, out AtributeBolean, out AtributeInstance))
            {
                AtributeCollider = AtributeInstance.physicalColliderReferenceOnly;

                if (AtributeCollider.GetComponent<StructureComponent>() != null)
                {
                    this.paredMadera = AtributeCollider.GetComponent<StructureComponent>();
                    foreach (StructureComponent objs in (Colecciones::HashSet<StructureComponent>)StructureComponents.GetValue(paredMadera.gameObject.GetComponent<StructureComponent>()._master))
                    {
                        if (objs.IsWallType()) TakeDamage.KillSelf(objs);
                    }
                }
            }
            else
            {
                rust.Notice(Player, "You dont see a WAll Structure ¿?");
            }
        }
        void FinalizarEvento(NetUser Player)
        {
            try
            {
                if (Player != null)
                    Player.playerClient.GetComponent<PlayerStatus>().Whiner = true;
                rust.BroadcastChat(SysName, Green + "Event End!");
                rust.BroadcastChat(SysName, Yellow + "TOP PLAYERS");
                int contador = 0;
                foreach (var x in Players2.OrderByDescending(x => x.playerClient.GetComponent<PlayerStatus>().Kills))
                {
                    contador++;
                    try
                    {
                        if (x.playerClient.GetComponent<PlayerStatus>().Whiner == true)
                        {
                            rust.BroadcastChat(SysName, string.Format("{0} - {1} {2}The Whinner", contador, x.displayName, Yellow));
                        }
                        else
                        {
                            rust.BroadcastChat(SysName, string.Format("{0} - {1}", contador, x.displayName));
                        }
                    }
                    catch (Exception)
                    {
                        rust.BroadcastChat(SysName, Red + " Error -> " + Blue + "Notificar a Daniel25A");
                    }
                    if (contador == 3) break;
                }
            }
            catch (Exception)
            {
              
            }
            finally
            {
                ResetEvent();
            }
         
        }
        void ClearPlayers(NetUser Player)
        {
            if (Player.admin == false) return;
            JugadoresenJuego.Clear();
            Players2.Clear();
           
        }
        [ChatCommand("hg")]
        void HungerGamesCommands(NetUser netUser, string command, string[] args)
        {
            if (args.Length == 0) return;
            switch (args[0])
            {
                case "init":
                    IniciarEvento(netUser);
                    break;
                case "join":
                    EntraralJuego(netUser);
                    break;
                case "forcestart":
                    ForceStart(netUser);
                    return;
                case "inventory":
                    if (Inventarios.ContainsKey(netUser.userID)) ReturnPlayerInventory(netUser);
                    if (PlayersPostions.ContainsKey(netUser.userID))
                    {
                        ServerController.TeleportPlayerToWorld(netUser.networkPlayer, PlayersPostions[netUser.userID]);
                        PlayersPostions.Remove(netUser.userID);
                    }
                    break;
                case "clear":
                    ClearPlayers(netUser);
                    break;
                case "forcefinally":
                    ForceFinally(netUser);
                    break;
                default:
                    break;
            }
        }
        [ChatCommand("config")]
        void cmdConfigCommands(NetUser Player, string command, string[] args)
        {
            if (Player.admin == false)
                return;
            if (args.Length == 0)
                return;
            switch (args[0])
            {
                case "setlocation":
                    cachedLocationEvent = Player.playerClient.rootControllable.transform.localPosition;
                    rust.Notice(Player,"DONE !");
                    break;
                case "testsave":
                    RecordInventoryPlayer(Player);
                    break;
                case "testreturn":
                    ReturnPlayerInventory(Player);
                    break;
                default:
                    rust.SendChatMessage(Player, SysName, "No Existe el Comando");
                    break;
            }
        }
    }
}
