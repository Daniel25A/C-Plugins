using System.Collections.Generic;
using System;
using System.Reflection;
using System.Data;
using UnityEngine;
using Oxide.Core;
using RustProto;
using Oxide.Core.Plugins;
namespace Oxide.Plugins
{
    [Info("Project4Anticheat", "Daniel25A", "1.0.0")]
    class Project4Anticheat : RustLegacyPlugin
    {
        /// <Variables>
        internal static String Prefix = "[Project4Protection]";
        /// </Variables>
        /// <Colecciones>
        internal static Dictionary<string, double> MaximoDistanciaArmas = new Dictionary<string, double>();
        internal static Dictionary<ulong, int> StrikesOverkill = new Dictionary<ulong, int>();
        internal static Dictionary<ulong, Timer> AutoHeadHack = new Dictionary<ulong, Timer>();
        internal static Dictionary<ulong, int> HeadCounter = new Dictionary<ulong, int>();
        /// </Colecciones>
        void HeadsMethod(ulong UserID)
        {
            if (!AutoHeadHack.ContainsKey(UserID)) return;
            if (!HeadCounter.ContainsKey(UserID)) return;
            if (HeadCounter[UserID] >= 5)
            {
                NetUser player = NetUser.FindByUserID(UserID);
                if (player == null)
                {
                    rust.BroadcastChat(Prefix, $"{UserID} Salio del server pero fue agregado a la lista de autobans por HEADSMULTIPLER hack");
                    BanList.Add(UserID, "Left Player", "HEADSMULTIPLER");
                    BanList.Save();
                }
                else
                {
                    rust.BroadcastChat(Prefix, $"{player.displayName} Se le ha detectado HEADSMULTIPLER y sera baneado del server.");
                    BanList.Add(player.userID, player.displayName, "HEADSMULTIPLER");
                }
            }
            AutoHeadHack[UserID].Destroy();
            AutoHeadHack.Remove(UserID);
            HeadCounter.Remove(UserID);
        }
        void LoadConfigDistanceOfWeapons(Boolean clear = true)
        {
            if (clear) MaximoDistanciaArmas.Clear();
            try
            {
                MaximoDistanciaArmas.Add("p250", 120);
                MaximoDistanciaArmas.Add("9mm pistol", 80);
                MaximoDistanciaArmas.Add("revolver", 80);
                MaximoDistanciaArmas.Add("m4", 140);
                MaximoDistanciaArmas.Add("bolt action rifle", 250);
                MaximoDistanciaArmas.Add("mp5a4", 80);
            }
            catch (Exception)
            {
                Debug.LogError($"{Prefix} Ocurrio un Error al Cargar las Distancias de las armas");
            }
        }
        Boolean CheckOverKIll(Vector3 point1, Vector3 point2, string weapon)
        {
            if (!MaximoDistanciaArmas.ContainsKey(weapon)) return false;
            var distancia = Math.Floor(Vector3.Distance(point1, point2));
            if (distancia > MaximoDistanciaArmas[weapon])
                return true;
            else
                return false;
        }
        private void OnKilled(TakeDamage takedamage, DamageEvent damage)
        {
            if (damage.attacker.client != null)
            {
                if (damage.victim.client == null) return;
                if (takedamage is HumanBodyTakeDamage)
                {
                    if (damage.bodyPart==BodyPart.Head)
                    {

                        ulong id = damage.attacker.client.netUser.userID;
                        if (!AutoHeadHack.ContainsKey(id))
                            AutoHeadHack.Add(id, timer.Once(20f, () => HeadsMethod(id)));
                        if (!HeadCounter.ContainsKey(id))
                            HeadCounter.Add(id, 0);
                        HeadCounter[id]++;
                    }
                }
            }
        }
            object ModifyDamage(TakeDamage takedamage, DamageEvent damage)
        {
            if (damage.attacker.client != null)
            {
                if (damage.victim.client == null) return null;
                NetUser _atacante = damage.attacker.client.netUser;
                NetUser _victima = damage.attacker.client.netUser;
                if (!(damage.extraData is WeaponImpact))
                    return null;
                var weapon = damage.extraData as WeaponImpact;
                Vector3 punto1 = damage.attacker.id.transform.position;
                Vector3 punto2 = damage.attacker.id.transform.position;
                if (CheckOverKIll(punto1, punto2, weapon.dataBlock.name.ToLower()))
                {
                    var distancia = Math.Floor(Vector3.Distance(punto1, punto2));
                    var arma = weapon.dataBlock.name;
                    if (!StrikesOverkill.ContainsKey(_atacante.userID)) StrikesOverkill.Add(_atacante.userID, 1);
                    else
                        StrikesOverkill[_atacante.userID]++;
                    if (StrikesOverkill[_atacante.userID] >= 3)
                    {
                       foreach(var _user in rust.GetAllNetUsers())
                        {
                            rust.Notice(_user, $"{_atacante.displayName} Fue Baneado del Server por ser Detectado con Overkill {distancia}/{arma}");
                        }
                        BanList.Add(_atacante.userID, _atacante.displayName, $"Overkill Whit Distance {distancia}/{arma}");
                        BanList.Save();
                    }
                    else
                    {
                        rust.BroadcastChat(Prefix, $"{_atacante.displayName} Se le ha Detectado OverKill  con la distancia de {distancia} con el arma {arma}");
                    }
                }
            }
            return null;
        }
    }
}
