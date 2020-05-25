

using System.Collections.Generic;
using Oxide;
using System;
using UnityEngine;
using static UICamera.Mouse;
using System.Collections;

namespace Oxide.Plugins
{
    [Info("SurvilandAnticheat", "Daniel25A", "1.0.0")]
    class SurvilandAnticheat : RustLegacyPlugin
    {
        //Colleciones a usar
        private Dictionary<ulong, DateTime> HeadShotChecker;
        private Dictionary<ulong, int> Strikes;
        //Variables a usar
        private string SysName = "[SAnticheat]";
        static readonly float MaxSpeed = 11f; //Variable de Solo lectura, no intente modificar en tiempo de ejecución o abara Error
        class PlayerController : MonoBehaviour
        {
            PlayerClient player;
            Vector3 oldPosition;
            string SysName = "[SAnticheat]";
            SurvilandAnticheat Logs = new SurvilandAnticheat();
            CharacterController rootMovementController;
            void Start()
            {
                player = GetComponent<PlayerClient>();
                rootMovementController = GetComponent<CharacterController>();
                if (player == null) throw new InvalidOperationException("Error al Obtener el Componente PlayerClient");
                else
                {
                    Debug.Log("Componente Cargado con Exito");
                }
            }
            void Awake()
            {
                StartCoroutine(Loop());
            }
            IEnumerator Loop()
            {
                while (true)
                {
                    yield return new WaitForSeconds(1);
                    SpeedHack();
                }
            }

            void SpeedHack()
            {
                if (oldPosition == default(Vector3))
                {
                    oldPosition = this.player.rootControllable.transform.position;
                    return;
                }
                var Distance = Math.Round(Vector3.Distance(oldPosition, this.player.rootControllable.transform.position));

                if (Distance > MaxSpeed)
                {
                    Logs.SendLogServer($"{player.netUser.displayName} Was Kicked by Speed Hack Detected. {Distance} MTS.");
                    player.netUser.Kick(NetError.Facepunch_Approval_ConnectorDidNothing, true);
                }
                oldPosition = player.transform.position;
            }
        }
        void OnPlayerConnected(NetUser player)
        {
            if (player.playerClient.gameObject.GetComponent<PlayerController>() == null)
            {
                player.playerClient.gameObject.AddComponent<PlayerController>();
            }
        }
        void OnPlayerDisconnected(uLink.NetworkPlayer player)
        {
            if (player.GetLocalData<NetUser>().playerClient.GetComponent<PlayerController>() != null)
            {
                GameObject.DestroyImmediate(player.GetLocalData<NetUser>().playerClient.GetComponent<PlayerController>());
            }
        }
        void OnKilled(TakeDamage takedamage, DamageEvent damage)
        {
            if (!(takedamage is HumanBodyTakeDamage)) return;
            NetUser Attacker = damage.attacker.client?.netUser ?? null;
            NetUser Victim = damage.victim.client?.netUser ?? null;
            if (Attacker == null || Victim == null) return;
            int Strike;
            DateTime Time;
            var AttackerID = Attacker.userID;
            if (HeadShotChecker.TryGetValue(AttackerID, out Time))
            {
                if ((Time - DateTime.Now).Seconds < 10)
                {
                    if (Strikes.TryGetValue(AttackerID, out Strike))
                    {
                        if (Strike >= 2)
                        {
                            damage.attacker.client.netUser.Kick(NetError.ConnectionBanned, true);
                            SendLogServer($"{Attacker.displayName} Detected Aimbot Hack | Will Kick from the server");
                        }
                        else
                        {
                            Strikes[AttackerID]++;
                        }
                    }
                    else
                    {
                        Strikes.Add(AttackerID, 1);
                    }
                }
                else
                {
                    Debug.Log("Timer No es Mayor");
                    var TimerAdd = DateTime.Now.AddSeconds(10);
                    HeadShotChecker[AttackerID] = TimerAdd;

                    if (Strikes.ContainsKey(AttackerID))
                    {
                        Strikes[AttackerID] = 0;
                    }
                    else
                    {
                        Strikes.Add(AttackerID, 1);
                    }
                }
            }
            else
            {
                Debug.Log("No Existe la Clave");
                var TimerAdd = DateTime.Now.AddSeconds(10);
                HeadShotChecker.Add(AttackerID, TimerAdd);
            }
        }
    
        void OnServerInitialized()
        {
            foreach (var x in rust.GetAllNetUsers())
            {
                x.playerClient.gameObject.AddComponent<PlayerController>();
            }
            HeadShotChecker = new Dictionary<ulong, DateTime>();
            Strikes = new Dictionary<ulong, int>();
        }
        void Unload()
        {
            var Objetos = GameObject.FindObjectsOfType<PlayerController>();
            if (Objetos != null)
                foreach (var gameObject in Objetos)
                    GameObject.Destroy(gameObject);
        }

        void SendLogServer(string Log)
        {
            rust.BroadcastChat(SysName, Log);
        }
    }
}
