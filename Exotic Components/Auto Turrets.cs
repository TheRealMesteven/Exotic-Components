﻿using PulsarModLoader.Content.Components.AutoTurret;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using HarmonyLib;
namespace Exotic_Components
{
    internal class Auto_Turrets
    {
        class AutoDefenderMod : AutoTurretMod
        {
            public override string Name => "AutoDefenderTurret";

            public override PLShipComponent PLAutoTurret => new AutoDefender();
        }
        class AutoLaserMod : AutoTurretMod
        {
            public override string Name => "AutoLaserTurret";

            public override PLShipComponent PLAutoTurret => new AutoLaser();
        }
        class AutoLightningTurret : AutoTurretMod
        {
            public override string Name => "AutoLightningTurret";

            public override PLShipComponent PLAutoTurret => new AutoLightning();
        }
        class AutoFocusedTurret : AutoTurretMod
        {
            public override string Name => "AutoFocusedLaserTurret";

            public override PLShipComponent PLAutoTurret => new AutoFocusedLaser();
        }
        class AutoScrapperTurret : AutoTurretMod 
        {
            public override string Name => "AutoScrapperTurret";
            public override PLShipComponent PLAutoTurret => new AutoScrapper();
        }
        class AutoAncientSentryTurret : AutoTurretMod
        {
            public override string Name => "AutoSentryTurret";
            public override PLShipComponent PLAutoTurret => new AutoSentryTurret();
        }
        class AutoPhaseTurret : AutoTurretMod
        {
            public override string Name => "AutoPhaseTurret";
            public override PLShipComponent PLAutoTurret => new AutoPhase();
        }

        class AutoPlasmaMod : AutoTurretMod
        {
            public override string Name => "AutoPlasmaTurret";

            public override PLShipComponent PLAutoTurret => new AutoPlasma();
        }

        class AutoBurtsMod : AutoTurretMod
        {
            public override string Name => "AutoBurstTurret";

            public override PLShipComponent PLAutoTurret => new AutoBurst();
        }
        class AutoSpreadMod : AutoTurretMod
        {
            public override string Name => "AutoSpreadshotTurret";

            public override PLShipComponent PLAutoTurret => new AutoSpreadshot();
        }

        class AutoFlamelanceMod : AutoTurretMod
        {
            public override string Name => "AutoFlamelanceTurret";

            public override PLShipComponent PLAutoTurret => new AutoFlamelance();
        }

        class AutoBioHazardMod : AutoTurretMod
        {
            public override string Name => "AutoBioHazardTurret";

            public override PLShipComponent PLAutoTurret => new AutoBioHazard();
        }

        class ReciveAutoLaserDamage : PulsarModLoader.ModMessage
        {
            public override void HandleRPC(object[] arguments, PhotonMessageInfo sender)
            {
                LaserAutoTurretDamage((int)arguments[0], (int)arguments[1], (float)arguments[2], (float)arguments[3], (Vector3)arguments[4], (int)arguments[5], (int)arguments[6], (int)arguments[7], (int)arguments[8]);
            }
        }

        public static void LaserAutoTurretDamage(int shipID, int inAttackingShipID, float damage, float randomNum, Vector3 localPosition, int inTurretID, int inProjID, int inBeamTickCount, int turretID)
        {
            PLShipInfoBase shipFromID = PLEncounterManager.Instance.GetShipFromID(shipID);
            PLShipInfoBase shipFromID2 = PLEncounterManager.Instance.GetShipFromID(inAttackingShipID);
            PLSpaceTarget spaceTargetFromID = PLEncounterManager.Instance.GetSpaceTargetFromID(shipID);
            if (shipFromID != null && shipFromID2 != null)
            {
                AutoLaser pllaserTurret = shipFromID2.GetAutoTurretAtID(inTurretID) as AutoLaser;
                if (pllaserTurret != null && !pllaserTurret.GetCounterForProjID(inProjID).HasProcessedBeamTickCounter(inBeamTickCount))
                {
                    Vector3 vector = shipFromID.Exterior.transform.TransformPoint(localPosition);
                    bool bottomHit = false;
                    Vector3 normalized = (vector - shipFromID.Exterior.transform.position).normalized;
                    Vector3 rhs = -shipFromID.Exterior.transform.up;
                    if (Vector3.Dot(normalized, rhs) > -0.1f)
                    {
                        bottomHit = true;
                    }
                    PLServer.Instance.LaserTurretExplosion(vector, shipFromID.ShipID, pllaserTurret.laserTurretExplosionID);
                    shipFromID.TakeDamage(damage, bottomHit, pllaserTurret.LaserDamageType, randomNum, -1, shipFromID2, turretID);
                    return;
                }
            }
            else if (spaceTargetFromID != null && shipFromID2 != null)
            {
                AutoLaser pllaserTurret2 = shipFromID2.GetAutoTurretAtID(inTurretID) as AutoLaser;
                if (pllaserTurret2 != null && !pllaserTurret2.GetCounterForProjID(inProjID).HasProcessedBeamTickCounter(inBeamTickCount))
                {
                    spaceTargetFromID.TakeDamage(damage);
                }
            }
        }
        [HarmonyLib.HarmonyPatch(typeof(PLShipInfoBase), "GetTurretAtID")]
        class GetAutoTurret
        {
            static void Postfix(PLShipInfoBase __instance, ref PLTurret __result, int inID)
            {
                if (__result == null) __result = __instance.GetAutoTurretAtID(inID);
            }
        }

    }

    class AutoDefender : PLAutoTurret
    {
        private Vector3[] Offsets_ProjArray;
        public AutoDefender(int inLevel = 0, int inSubTypeData = 0)
        {
            this.Name = "Automated Defender Turret";
            this.Desc = "A turret that fires a barrage of missiles designed for close to medium range damage. This version doesn't require crew control.";
            this.ActualSlotType = ESlotType.E_COMP_AUTO_TURRET;
            this.m_SlotType = ESlotType.E_COMP_AUTO_TURRET;
            this.m_Damage = 38f;
            this.FireDelay = 4.2f;
            this.m_MarketPrice = 20000;
            this.m_ProjSpeed = 200f;
            this.MinFireDelay = 2f;
            this.TurretRange = 4000f;
            base.CargoVisualPrefabID = 3;
            base.Level = inLevel;
            base.SubType = AutoTurretModManager.Instance.GetIDFromName("AutoDefenderTurret");
            this.AutoAimPowerUsageRequest = 1f;
            this.m_MaxPowerUsage_Watts = 2500f;
            this.HeatGeneratedOnFire = 0.07f;
            float x = 0.01f;
            float y = 0.01f;
            this.Offsets_ProjArray = new Vector3[8];
            this.Offsets_ProjArray[0] = Vector3.Scale(new Vector3(x, y, 0f), new Vector3(-3f, 0.5f, 0f));
            this.Offsets_ProjArray[1] = Vector3.Scale(new Vector3(x, y, 0f), new Vector3(-1f, 0.5f, 0f));
            this.Offsets_ProjArray[2] = Vector3.Scale(new Vector3(x, y, 0f), new Vector3(3f, 0.5f, 0f));
            this.Offsets_ProjArray[3] = Vector3.Scale(new Vector3(x, y, 0f), new Vector3(1f, 0.5f, 0f));
            this.Offsets_ProjArray[4] = Vector3.Scale(new Vector3(x, y, 0f), new Vector3(-3f, -0.5f, 0f));
            this.Offsets_ProjArray[5] = Vector3.Scale(new Vector3(x, y, 0f), new Vector3(-1f, -0.5f, 0f));
            this.Offsets_ProjArray[6] = Vector3.Scale(new Vector3(x, y, 0f), new Vector3(3f, -0.5f, 0f));
            this.Offsets_ProjArray[7] = Vector3.Scale(new Vector3(x, y, 0f), new Vector3(1f, -0.5f, 0f));

        }
        public override void Fire(int inProjID, Vector3 dir)
        {
            this.LastFireTime = Time.time;
            this.ChargeAmount = 0f;
            if (Time.time - base.ShipStats.Ship.LastCloakingSystemActivatedTime > 2f)
            {
                base.ShipStats.Ship.SetIsCloakingSystemActive(false);
            }
            PLMusic.PostEvent("play_ship_generic_external_weapon_defenderturret_shoot", this.TurretInstance.gameObject);
            this.TurretInstance.StartCoroutine(this.ControlledFireRoutine(inProjID, dir));
        }
        public override void Tick()
        {
            base.Tick();
            if (base.ShipStats.isPreview)
            {
                return;
            }
        }
        private IEnumerator ControlledFireRoutine(int inProjID, Vector3 dir)
        {
            PLRand spreadRandomness = new PLRand(inProjID);
            int index = 0;
            foreach (Vector3 b in this.Offsets_ProjArray)
            {
                GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this.TurretInstance.Proj, this.TurretInstance.FiringLoc.transform.position + b, this.TurretInstance.FiringLoc.transform.rotation);
                PLMissle component = gameObject.GetComponent<PLMissle>();
                PLProjectile component2 = gameObject.GetComponent<PLProjectile>();
                gameObject.GetComponent<Rigidbody>().velocity = base.ShipStats.Ship.Exterior.GetComponent<Rigidbody>().velocity + dir * this.m_ProjSpeed * Mathf.Clamp((float)spreadRandomness.NextDouble(), 0.5f, 1f);
                component2.ProjID = inProjID + index;
                component.MaxDamage = this.m_Damage * base.LevelMultiplier(0.15f, 1f) * base.ShipStats.TurretDamageFactor;
                component.Damage = this.m_Damage * base.LevelMultiplier(0.15f, 1f) * base.ShipStats.TurretDamageFactor;
                component2.MaxLifetime = 7f;
                component2.OwnerShipID = base.ShipStats.Ship.ShipID;
                component.TurnFactor *= spreadRandomness.Next(0.33f, 0.66f);
                if (base.ShipStats.Ship.TargetShip != null)
                {
                    component.TargetShipID = base.ShipStats.Ship.TargetShip.ShipID;
                }
                component2.TurretID = this.AutoTurretID;
                component.TargetShip = base.ShipStats.Ship.TargetShip;
                component2.ExplodeOnMaxLifetime = false;
                component2.MyDamageType = EDamageType.E_PHYSICAL;
                component.TrackingDelay = spreadRandomness.Next(0.5f, 1.5f);
                component.Speed *= spreadRandomness.Next(0.5f, 1f);
                component.SetShouldLeadTargetShip(true);
                component.SetLerpedSpeed(base.ShipStats.Ship.ExteriorRigidbody.velocity.magnitude * 1.5f);
                Physics.IgnoreCollision(base.ShipStats.Ship.Exterior.GetComponent<Collider>(), gameObject.GetComponent<Collider>());
                PLServer.Instance.m_ActiveProjectiles.Add(component2);
                int num = index;
                index = num + 1;
                yield return new WaitForSeconds(0.1f);
                this.Heat += this.HeatGeneratedOnFire;
            }
            yield break;
        }
        public override void InnerCheckFire()
        {
            PLServer.Instance.photonView.RPC("AutoTurretFire", PhotonTargets.All, new object[]
            {
            base.ShipStats.Ship.ShipID,
            this.AutoTurretID,
            PLServer.Instance.ServerProjIDCounter,
            Vector3.zero
            });
            PLServer.Instance.ServerProjIDCounter += this.Offsets_ProjArray.Length;
        }
        public override string GetTurretPrefabPath()
        {
            return "NetworkPrefabs/Component_Prefabs/DefenderTurret";
        }
    }

    class AutoLaser : PLTurret
    {
        protected bool IsBeamActive;
        protected float BeamActiveTime = 1f;
        protected float DamageChecksPerSecond = 3f;
        protected float LastDamageCheckTime;
        protected float m_Laser_xzScale = 1f;
        protected float m_Laser_yScale = 1f;
        public float LaserDist = 20000f;
        protected float LaserBaseRadius = 0.045f;
        public int laserTurretExplosionID;
        private int ProjIDLast = int.MinValue;
        private int BeamTickCount = int.MinValue;
        private Dictionary<int, ProjBeamCounter> ProjBeamCounters = new Dictionary<int, ProjBeamCounter>();
        private float LastClearProjBeamCountersTime = float.MinValue;
        public EDamageType LaserDamageType = EDamageType.E_BEAM;
        protected string PlayShootSFX = "";
        protected string StopShootSFX = "";
        protected string PlayProjSFX = "";
        protected string StopProjSFX = "";
        private bool beamSFXActive;
        public AutoLaser(int inLevel = 0, int inSubTypeData = 0)
        {
            this.Name = "Automated Laser Turret";
            this.Desc = "Long range laser weapon that is particularly powerful against hulls and ship systems. This is an automated version.";
            this.m_Damage = 34f;
            this.FireDelay = 2f;
            base.SubType = AutoTurretModManager.Instance.GetIDFromName("AutoLaserTurret");
            this.ActualSlotType = ESlotType.E_COMP_AUTO_TURRET;
            this.m_MarketPrice = 3500;
            base.Level = inLevel;
            base.SubTypeData = (short)inSubTypeData;
            this.TurretRange = 8000f;
            base.CargoVisualPrefabID = 3;
            this.CanHitMissiles = true;
            this.AutoAim_CanTargetMissiles = true;
            this.HeatGeneratedOnFire = 0.15f;
            this.PlayShootSFX = "play_ship_generic_external_weapon_laser_shoot";
            this.StopShootSFX = "";
            this.PlayProjSFX = "play_ship_generic_external_weapon_laser_projectile";
            this.StopProjSFX = "stop_ship_generic_external_weapon_laser_projectile";
            this.UpdateMaxPowerUsageWatts();
        }
        public ProjBeamCounter GetCounterForProjID(int inProjID)
        {
            if (this.ProjBeamCounters.ContainsKey(inProjID))
            {
                return this.ProjBeamCounters[inProjID];
            }
            ProjBeamCounter projBeamCounter = new ProjBeamCounter(inProjID);
            this.ProjBeamCounters.Add(inProjID, projBeamCounter);
            return projBeamCounter;
        }
        public override void CheckFire()
        {
            if (this.IsFiring && this.ChargeAmount > 0.99f && PhotonNetwork.isMasterClient && !this.IsBeamActive && !this.IsOverheated)
            {
                PLServer.Instance.photonView.RPC("AutoTurretFire", PhotonTargets.All, new object[]
                {
                base.ShipStats.Ship.ShipID,
                this.AutoTurretID,
                PLServer.Instance.ServerProjIDCounter,
                Vector3.zero
                });
                PLServer.Instance.ServerProjIDCounter++;
            }
        }
        public override void Unequip()
        {
            this.StopBeamSFX();
            base.Unequip();
        }
        public virtual void UpdateMaxPowerUsageWatts()
        {
            base.CalculatedMaxPowerUsage_Watts = 2046f * base.LevelMultiplier(0.2f, 1f);
        }
        public override void Tick()
        {
            if (PLEncounterManager.Instance == null || PLInput.Instance == null)
            {
                return;
            }
            base.Tick();
            this.UpdateMaxPowerUsageWatts();
            if (Time.time - this.LastClearProjBeamCountersTime > 5f)
            {
                this.LastClearProjBeamCountersTime = Time.time;
                List<ProjBeamCounter> list = new List<ProjBeamCounter>(this.ProjBeamCounters.Values);
                for (int i = 0; i < list.Count; i++)
                {
                    if (list[i] != null && list[i].ProjID < this.ProjIDLast - 50)
                    {
                        this.ProjBeamCounters.Remove(list[i].ProjID);
                    }
                }
            }
            if (this.TurretInstance != null && base.ShipStats.Ship != null && base.ShipStats.Ship.ShipTypeID == EShipType.E_BEACON)
            {
                this.TurretRange = 20000f;
            }
            if (this.IsBeamActive && Time.time - this.LastFireTime > this.BeamActiveTime)
            {
                this.IsBeamActive = false;
            }
            if (this.IsBeamActive && this.TurretInstance != null && !this.IsOverheated && Time.time - this.LastDamageCheckTime > 1f / this.DamageChecksPerSecond)
            {
                this.BeamTickCount++;
                this.LastDamageCheckTime = Time.time;
                this.Heat += this.HeatGeneratedOnFire;
                float num = this.m_Damage * base.LevelMultiplier(0.15f, 1f) * base.ShipStats.TurretDamageFactor;
                Ray ray = new Ray(this.TurretInstance.FiringLoc.position, this.TurretInstance.FiringLoc.forward);
                int layerMask = 524289;
                int layer = 0;
                if (base.ShipStats.Ship.GetExteriorMeshCollider() != null)
                {
                    layer = base.ShipStats.Ship.GetExteriorMeshCollider().gameObject.layer;
                    base.ShipStats.Ship.GetExteriorMeshCollider().gameObject.layer = 31;
                }
                float num2 = 20000f;
                try
                {
                    RaycastHit raycastHit;
                    if (Physics.SphereCast(ray, 3f, out raycastHit, num2, layerMask))
                    {
                        PLShipInfoBase plshipInfoBase = null;
                        PLDamageableSpaceObject_Collider pldamageableSpaceObject_Collider = null;
                        if (raycastHit.collider != null)
                        {
                            plshipInfoBase = raycastHit.collider.GetComponentInParent<PLShipInfoBase>();
                            pldamageableSpaceObject_Collider = raycastHit.collider.GetComponent<PLDamageableSpaceObject_Collider>();
                        }
                        bool flag = false;
                        RaycastHit raycastHit2;
                        if (plshipInfoBase != null && plshipInfoBase.Hull_Virtual_MeshCollider != null && !plshipInfoBase.CollisionShieldShouldBeActive() && !plshipInfoBase.Hull_Virtual_MeshCollider.Raycast(ray, out raycastHit2, num2))
                        {
                            flag = true;
                            plshipInfoBase = null;
                        }
                        PLProximityMine plproximityMine = null;
                        if (plshipInfoBase == null)
                        {
                            plproximityMine = raycastHit.collider.GetComponentInParent<PLProximityMine>();
                        }
                        if (raycastHit.collider.gameObject.name.StartsWith("MatrixPoint"))
                        {
                            PLMatrixPoint component = raycastHit.collider.GetComponent<PLMatrixPoint>();
                            if (component != null && component.IsActiveAndBlocking())
                            {
                                component.OnHit(this.ShipStats.Ship);
                            }
                        }
                        PLSpaceTarget plspaceTarget = null;
                        if (raycastHit.transform != null)
                        {
                            plspaceTarget = raycastHit.transform.GetComponentInParent<PLSpaceTarget>();
                        }
                        if (pldamageableSpaceObject_Collider != null)
                        {
                            pldamageableSpaceObject_Collider.MyDSO.TakeDamage_Location(num, raycastHit.point, raycastHit.normal);
                        }
                        else if (plproximityMine != null)
                        {
                            PLServer.Instance.photonView.RPC("ProximityMineExplode", PhotonTargets.All, new object[]
                            {
                            plproximityMine.EncounterNetID
                            });
                        }
                        else if (plshipInfoBase != null)
                        {
                            if (plshipInfoBase != base.ShipStats.Ship)
                            {
                                float num3 = UnityEngine.Random.Range(0f, 1f);
                                if (!PhotonNetwork.isMasterClient)
                                {
                                    PulsarModLoader.ModMessage.SendRPC("Pokegustavo.ExoticComponents", "Exotic_Components.Auto_Turrets.ReciveAutoLaserDamage", PhotonTargets.MasterClient, new object[]
									{
									plshipInfoBase.ShipID,
									base.ShipStats.Ship.ShipID,
									num,
									num3,
									plshipInfoBase.Exterior.transform.InverseTransformPoint(raycastHit.point),
									this.AutoTurretID,
									this.ProjIDLast,
									this.BeamTickCount,
									this.AutoTurretID
									});

                                }
                                Auto_Turrets.LaserAutoTurretDamage(plshipInfoBase.ShipID,base.ShipStats.Ship.ShipID,num,num3,plshipInfoBase.Exterior.transform.InverseTransformPoint(raycastHit.point),AutoTurretID,ProjIDLast,BeamTickCount,AutoTurretID);
                            }
                        }
                        else if (plspaceTarget != null)
                        {
                            if (plspaceTarget != base.ShipStats.Ship)
                            {
                                float num4 = UnityEngine.Random.Range(0f, 1f);
                                if (!PhotonNetwork.isMasterClient)
                                {
                                    PulsarModLoader.ModMessage.SendRPC("Pokegustavo.ExoticComponents", "Exotic_Components.Auto_Turrets.ReciveAutoLaserDamage", PhotonTargets.MasterClient, new object[]
									{
									plspaceTarget.SpaceTargetID,
									base.ShipStats.Ship.ShipID,
									num,
									num4,
									plspaceTarget.transform.InverseTransformPoint(raycastHit.point),
									this.AutoTurretID,
									this.ProjIDLast,
									this.BeamTickCount,
									this.AutoTurretID
									});

                                }
                                Auto_Turrets.LaserAutoTurretDamage(plspaceTarget.SpaceTargetID, base.ShipStats.Ship.ShipID, num, num4, plspaceTarget.transform.InverseTransformPoint(raycastHit.point), this.AutoTurretID, this.ProjIDLast, this.BeamTickCount, this.AutoTurretID);

                            }
                        }
                        else if (raycastHit.collider != null && raycastHit.collider.CompareTag("Projectile"))
                        {
                            PLProjectile component2 = raycastHit.collider.gameObject.GetComponent<PLProjectile>();
                            if (component2 != null && component2.LaserCanCauseExplosion && component2.OwnerShipID != -1 && component2.OwnerShipID != base.ShipStats.Ship.ShipID)
                            {
                                component2.TakeDamage(num, raycastHit.point, raycastHit.normal, base.GetCurrentOperator());
                            }
                            flag = true;
                        }
                        else if (!flag)
                        {
                            PLServer.Instance.photonView.RPC("LaserTurretExplosion", PhotonTargets.Others, new object[]
                            {
                            raycastHit.point,
                            -1,
                            this.laserTurretExplosionID
                            });
                            PLServer.Instance.LaserTurretExplosion(raycastHit.point, -1, this.laserTurretExplosionID);
                        }
                        if (!flag)
                        {
                            this.LaserDist = (raycastHit.point - this.TurretInstance.FiringLoc.position).magnitude;
                        }
                        else
                        {
                            this.LaserDist = num2;
                        }
                    }
                    Vector3 vector;
                    int num5;
                    PLSwarmCollider hitSwarmCollider = PLTurret.GetHitSwarmCollider(this.LaserDist, out vector, out num5, this.TurretInstance);
                    if (hitSwarmCollider != null)
                    {
                        float num6 = UnityEngine.Random.Range(0f, 1f);
                        hitSwarmCollider.MyShipInfo.HandleSwarmColliderHitVisuals(hitSwarmCollider, vector);
                        if (!PhotonNetwork.isMasterClient)
                        {
                            /*PulsarModLoader.ModMessage.SendRPC("Pokegustavo.ExoticComponents", "Exotic_Components.Auto_Turrets.ReciveAutoLaserDamage", PhotonTargets.MasterClient, new object[]
							{
							hitSwarmCollider.MyShipInfo.SpaceTargetID,
							base.ShipStats.Ship.ShipID,
							num * (float)num5,
							num6,
							vector,
							this.AutoTurretID,
							this.ProjIDLast,
							this.BeamTickCount,
							this.AutoTurretID
							});
							*/
                            PLServer.Instance.photonView.RPC("LaserTurretDamage", PhotonTargets.MasterClient, new object[]
                            {
                            hitSwarmCollider.MyShipInfo.SpaceTargetID,
                            base.ShipStats.Ship.ShipID,
                            num * (float)num5,
                            num6,
                            vector,
                            this.AutoTurretID,
                            this.ProjIDLast,
                            this.BeamTickCount,
                            this.AutoTurretID
                            });
                        }
                        PLServer.Instance.LaserTurretDamage(hitSwarmCollider.MyShipInfo.SpaceTargetID, base.ShipStats.Ship.ShipID, num * (float)num5, num6, vector, this.AutoTurretID, this.ProjIDLast, this.BeamTickCount, this.AutoTurretID);
                    }
                }
                catch
                {
                }
                if (base.ShipStats.Ship.GetExteriorMeshCollider() != null)
                {
                    base.ShipStats.Ship.GetExteriorMeshCollider().gameObject.layer = layer;
                }
            }
            if (this.TurretInstance != null && this.TurretInstance.BeamObject != null)
            {
                this.TurretInstance.BeamObject.transform.localPosition = new Vector3(0f, 0f, this.LaserDist * 0.5f) * (1f / this.TurretInstance.transform.lossyScale.x);
                float num7 = Mathf.Abs(Mathf.Sin(Time.time * 5f % 3.1415927f * 2f));
                Vector3 vector2 = new Vector3(this.LaserBaseRadius + 0.01f * num7, this.LaserDist * 0.5f, this.LaserBaseRadius + 0.01f * num7) * (1f / this.TurretInstance.transform.lossyScale.x);
                this.TurretInstance.BeamObject.transform.localScale = new Vector3(vector2.x * this.m_Laser_xzScale, vector2.y * this.m_Laser_yScale, vector2.z * this.m_Laser_xzScale);
                if (this.TurretInstance.BeamObject.activeSelf != this.IsBeamActive)
                {
                    this.TurretInstance.BeamObject.SetActive(this.IsBeamActive);
                }
                if (this.TurretInstance.BeamObjectRenderer == null)
                {
                    this.TurretInstance.BeamObjectRenderer = this.TurretInstance.BeamObject.GetComponent<Renderer>();
                }
                this.TurretInstance.BeamObjectRenderer.material.SetFloat("_TimeSinceShot", Time.time - this.LastFireTime);
                if (this.IsBeamActive && !this.IsOverheated)
                {
                    if (!this.beamSFXActive)
                    {
                        this.beamSFXActive = true;
                        if (this.PlayShootSFX != "")
                        {
                            PLMusic.PostEvent(this.PlayShootSFX, this.TurretInstance.gameObject);
                        }
                        if (this.PlayProjSFX != "")
                        {
                            PLMusic.PostEvent(this.PlayProjSFX, this.TurretInstance.gameObject);
                        }
                    }
                    if (!this.TurretInstance.OptionalGameObjects[0].activeSelf)
                    {
                        this.TurretInstance.OptionalGameObjects[0].SetActive(true);
                        return;
                    }
                }
                else
                {
                    this.StopBeamSFX();
                    if (this.TurretInstance.OptionalGameObjects[0].activeSelf)
                    {
                        this.TurretInstance.OptionalGameObjects[0].SetActive(false);
                    }
                }
            }
        }
        private void StopBeamSFX()
        {
            if (this.beamSFXActive)
            {
                this.beamSFXActive = false;
                if (this.TurretInstance != null)
                {
                    if (this.StopShootSFX != "")
                    {
                        PLMusic.PostEvent(this.StopShootSFX, this.TurretInstance.gameObject);
                    }
                    if (this.StopProjSFX != "")
                    {
                        PLMusic.PostEvent(this.StopProjSFX, this.TurretInstance.gameObject);
                    }
                }
            }
        }
        public override void Fire(int inProjID, Vector3 dir)
        {
            this.ChargeAmount = 0f;
            this.LastFireTime = Time.time;
            this.IsBeamActive = true;
            this.ProjIDLast = inProjID;
            this.BeamTickCount = 0;
            if (Time.time - base.ShipStats.Ship.LastCloakingSystemActivatedTime > 3f)
            {
                base.ShipStats.Ship.SetIsCloakingSystemActive(false);
            }
        }
        public override bool ApplyLeadingToAutoAimShot()
        {
            return false;
        }
        public override string GetTurretPrefabPath()
        {
            return "NetworkPrefabs/Component_Prefabs/LaserTurret";
        }
    }

    class AutoFocusedLaser : AutoLaser 
    {
        public AutoFocusedLaser(int inLevel = 0, int inSubTypeData = 0) : base(0, 0)
        {
            this.Name = "Automated Focused Laser Turret";
            this.Desc = "High powered laser weapon that is particularly powerful against hulls and ship systems. It is also capable of shooting down some missiles. This is an automatic version.";
            this.m_Damage = 75f;
            this.FireDelay = 6f;
            base.SubType = AutoTurretModManager.Instance.GetIDFromName("AutoFocusedLaserTurret");
            this.ActualSlotType = ESlotType.E_COMP_AUTO_TURRET;
            this.m_MarketPrice = 12000;
            base.Level = inLevel;
            base.SubTypeData = (short)inSubTypeData;
            this.TurretRange = 8000f;
            base.CargoVisualPrefabID = 3;
        }
        public override void UpdateMaxPowerUsageWatts()
        {
            this.m_MaxPowerUsage_Watts = 3960f * base.LevelMultiplier(0.2f, 1f);
        }
        public override string GetTurretPrefabPath()
        {
            return "NetworkPrefabs/Component_Prefabs/FocusLaserTurret";
        }
    }

    class AutoScrapper : AutoLaser 
    {
        public AutoScrapper(int inLevel = 0, int inSubTypeData = 0) : base(0, 0)
        {
            this.Name = "Automated Scrapper Turret";
            this.Desc = "Laser turret that has been modified by the W.D. Corporation. It can rip off chunks of hull and create scrap that can be sold or processed. This is an automatic version.";
            this.m_Damage = 27f;
            this.FireDelay = 4f;
            base.SubType = AutoTurretModManager.Instance.GetIDFromName("AutoScrapperTurret");
            this.ActualSlotType = ESlotType.E_COMP_AUTO_TURRET;
            this.m_MarketPrice = 3500;
            base.Level = inLevel;
            base.SubTypeData = (short)inSubTypeData;
            this.TurretRange = 8000f;
            base.CargoVisualPrefabID = 3;
            base.Experimental = true;
            this.PlayShootSFX = "play_ship_generic_external_weapon_laser_shoot";
            this.StopShootSFX = "";
            this.PlayProjSFX = "play_ship_generic_external_weapon_laser_projectile";
            this.StopProjSFX = "stop_ship_generic_external_weapon_laser_projectile";
            this.UpdateMaxPowerUsageWatts();
        }
        public override string GetTurretPrefabPath()
        {
            return "NetworkPrefabs/Component_Prefabs/ScrapperTurret";
        }
    }

    class AutoSentryTurret : AutoLaser 
    {
        public AutoSentryTurret(int inLevel = 0, int inSubTypeData = 0) : base(inLevel, inSubTypeData)
        {
            this.Name = "Automated Ancient Laser Turret";
            this.Desc = "This turret was recovered from an ancient sentry... and I decided to transform into an auto turret... yeah, just remember that it breaks some laws of physics, so if reality starts to collapse... RUN!";
            this.m_Damage = 173f;
            this.m_MarketPrice = 500000;
            this.FireDelay = 5f;
            this.ActualSlotType = ESlotType.E_COMP_AUTO_TURRET;
            base.SubType = AutoTurretModManager.Instance.GetIDFromName("AutoSentryTurret");
            this.Contraband = true;
            base.CalculatedMaxPowerUsage_Watts = 1200f;
            this.TurretRotationLerpSpeed = 20f;
            this.BeamActiveTime = 1f;
            this.LaserBaseRadius = 1f;
            this.DamageChecksPerSecond = 3f;
            this.PlayShootSFX = "play_sx_ship_enemy_ancientsentry_shoot";
            this.AutoAim_CanTargetMissiles = false;
            this.TurretRange = 10000f;
        }
        public override void OnTurretInstanceCreated()
        {
            base.OnTurretInstanceCreated();
            this.TurretInstance.BeamObject.GetComponent<Renderer>().material.SetColor("_LaserColor", this.TurretInstance.BeamObject.GetComponent<Renderer>().material.GetColor("_LaserColor") * 5f);
        }
        public override void Tick()
        {
            base.Tick();
            if (this.TurretInstance != null && this.TurretInstance.BeamObject != null)
            {
                this.TurretInstance.BeamObject.GetComponent<Renderer>().material.SetFloat("_TimeSinceShot", Time.time);
            }
            if (this.TurretInstance != null && this.TurretInstance.OptionalGameObjects[1] != null)
            {
                bool flag = this.ChargeAmount > 0.7f;
                GameObject gameObject = this.TurretInstance.OptionalGameObjects[1];
                if (!this.IsFiring && flag)
                {
                    Ray ray = new Ray(this.TurretInstance.FiringLoc.position, this.TurretInstance.FiringLoc.forward);
                    RaycastHit raycastHit = default(RaycastHit);
                    int layerMask = 524289;
                    if (Physics.SphereCast(ray, 1f, out raycastHit, 20000f, layerMask))
                    {
                        this.LaserDist = (raycastHit.point - this.TurretInstance.FiringLoc.position).magnitude * (1f / this.TurretInstance.transform.parent.lossyScale.x);
                    }
                    else
                    {
                        this.LaserDist = 20000f;
                    }
                }
                if (gameObject != null)
                {
                    float num = Mathf.Min(50000f, this.LaserDist);
                    gameObject.transform.localPosition = new Vector3(0f, 0f, num * 0.5f);
                    Mathf.Abs(0.2f);
                    gameObject.transform.localScale = new Vector3(1f, num * 0.5f, 1f);
                    if (gameObject.activeSelf != flag)
                    {
                        gameObject.SetActive(flag);
                    }
                }
            }
        }
        public override bool AutoAimShouldFireUpon(PLSpaceTarget ship)
        {
            return true;
        }
        public override bool AutoAimTrackingIsEnabled()
        {
            return true;
        }
        public override string GetTurretPrefabPath()
        {
            return "NetworkPrefabs/Component_Prefabs/CorruptedLaserTurret";
        }
    }

    class AutoPhase : PLPhaseTurret 
    {
        public AutoPhase(int inLevel = 0, int inSubTypeData = 0)
        {
            this.Name = "Automated Phase Turret";
            this.Desc = "Fires a beam that pierces through hull and armor to damage internal ship systems. It is not particularly effective at dealing damage to hull or shields. This is an automatic version.";
            this.m_Damage = 165f;
            base.SubType = AutoTurretModManager.Instance.GetIDFromName("AutoPhaseTurret");
            this.ActualSlotType = ESlotType.E_COMP_AUTO_TURRET;
            this.m_MarketPrice = 7500;
            this.m_ProjSpeed = 550f;
            this.FireDelay = 3f;
            this.TurretRange = 6000f;
            base.CargoVisualPrefabID = 3;
            base.Level = inLevel;
            this.FireTurretSoundSFX = "play_ship_generic_external_weapon_phaseturret_shoot";
            this.CanHitMissiles = true;
            this.UpdateMaxPowerUsageWatts();
        }
        public new void UpdateMaxPowerUsageWatts()
        {
            base.CalculatedMaxPowerUsage_Watts = 4620f * base.LevelMultiplier(0.2f, 1f);
        }
    }

    class AutoLightning : AutoLaser 
    {
        public AutoLightning(int inLevel = 0, int inSubTypeData = 0) : base(inLevel, inSubTypeData)
        {
            this.Name = "Automated Lightning Turret";
            this.Desc = "Fires a concentrated beam of electricity that deals great damage to shields. It does not have as much of an effect against a ship’s hull, but there is a small chance that hitting the hull will force the target ship to shut down. This is an automatic version";
            this.FireDelay = 6f;
            this.laserTurretExplosionID = 1;
            this.ActualSlotType = ESlotType.E_COMP_AUTO_TURRET;
            this.m_MarketPrice = 15500;
            this.m_Damage = 60f;
            this.PlayShootSFX = "play_ship_generic_external_weapon_lightningturret_shoot";
            this.StopShootSFX = "stop_ship_generic_external_weapon_lightningturret_shoot";
            this.PlayProjSFX = "play_ship_generic_external_weapon_lightningturret_projectile";
            this.StopProjSFX = "stop_ship_generic_external_weapon_lightningturret_projectile";
            this.LaserDamageType = EDamageType.E_LIGHTNING;
            base.SubType = AutoTurretModManager.Instance.GetIDFromName("AutoLightningTurret");
        }
        public override void UpdateMaxPowerUsageWatts()
        {
            base.CalculatedMaxPowerUsage_Watts = 2475f * base.LevelMultiplier(0.2f, 1f);
        }
        public override string GetDamageTypeString()
        {
            return "ENERGY (LTNG)";
        }
        public override string GetTurretPrefabPath()
        {
            return "NetworkPrefabs/Component_Prefabs/LightningTurret";
        }
        public override void Equip()
        {
            base.Equip();
        }
        public override void Tick()
        {
            base.Tick();
            if (this.TurretInstance != null)
            {
                if (this.lightningMat == null)
                {
                    this.lightningMat = this.TurretInstance.OptionalGameObjects[1].GetComponent<Renderer>().material;
                }
                this.TurretInstance.OptionalGameObjects[1].SetActive(this.IsBeamActive);
                if (this.IsBeamActive)
                {
                    this.TurretInstance.OptionalGameObjects[1].transform.localScale = new Vector3(14f, 14f, this.LaserDist * 0.33f);
                    this.lightningMat.mainTextureScale = new Vector2(1f, this.LaserDist * 0.0035f);
                    this.lightningMat.mainTextureOffset += new Vector2(0f, -Time.deltaTime * 5f);
                }
            }
        }
        private Material lightningMat;
    }

    class AutoPlasma : PLBasicTurret
    {
        public AutoPlasma(int inLevel = 0, int inSubTypeData = 0)
        {
            this.Name = "Automated Plasma Turret";
            this.Desc = "A standard armament that can be found on many ships. It is known for its fast firing capability and high damage. This is an automated version.";
            this.ActualSlotType = ESlotType.E_COMP_AUTO_TURRET;
            this.m_SlotType = ESlotType.E_COMP_AUTO_TURRET;
            this.m_Damage = 60f;
            this.SubType = AutoTurretModManager.Instance.GetIDFromName("AutoPlasmaTurret");
            this.m_MarketPrice = 4500;
            this.m_MaxPowerUsage_Watts = 1320f;
            this.m_ProjSpeed = 550f;
            this.FireDelay = 1.5f;
            this.TurretRange = 4000f;
            base.CargoVisualPrefabID = 3;
            base.Level = inLevel;
            this.FireTurretSoundSFX = "play_ship_generic_external_weapon_railgun_shoot";
        }
    }

    class AutoBurst : PLBasicTurret
    {
        public AutoBurst(int inLevel = 0, int inSubTypeData = 0) : base(inLevel, inSubTypeData)
        {
            this.Name = "Automated Burst Turret";
            this.Desc = "As its name suggests, this turret fires a burst of shots at its target. Designed for medium range damage. This is an automated version.";
            this.ActualSlotType = ESlotType.E_COMP_AUTO_TURRET;
            this.m_SlotType = ESlotType.E_COMP_AUTO_TURRET;
            this.m_Damage = 60f;
            base.SubType = AutoTurretModManager.Instance.GetIDFromName("AutoBurstTurret");
            this.m_MarketPrice = 11000;
            this.m_MaxPowerUsage_Watts = 2145f;
            this.m_ProjSpeed = 500f;
            this.FireDelay = 2.7f;
            this.TurretRange = 7000f;
            base.CargoVisualPrefabID = 3;
            base.Level = inLevel;
            this.HeatGeneratedOnFire = 0.125f;
            this.m_BurstShotCount = 4;
            this.m_BurstShotTiming = 0.3f;
            base.CargoVisualPrefabID = 3;
        }
    }

    class AutoSpreadshot : PLSpreadshotTurret
    {
        private float[] HeatLevelsPerBarrel;
        private Material[] GlowingTurretBarrelMaterials;
        public AutoSpreadshot(int inLevel = 0, int inSubTypeData = 0)
        {
            this.Name = "Automated Spreadshot Turret";
            this.Desc = "A turret designed for close range damage. It spews out several projectiles at a time and can cause serious damage to a nearby ship. This is an automated version.";
            this.ActualSlotType = ESlotType.E_COMP_AUTO_TURRET;
            this.m_SlotType = ESlotType.E_COMP_AUTO_TURRET;
            this.m_Damage = 24f;
            this.FireDelay = 1.7f;
            base.SubType = AutoTurretModManager.Instance.GetIDFromName("AutoSpreadshotTurret");
            this.m_MarketPrice = 3800;
            this.m_ProjSpeed = 700f;
            base.CalculatedMaxPowerUsage_Watts = 1188f;
            this.AutoAimPowerUsageRequest = 0.3f;
            this.MinFireDelay = 1f;
            this.TurretRange = 3400f;
            base.CargoVisualPrefabID = 3;
            base.Level = inLevel;
            this.HeatGeneratedOnFire = 0.5f;
            this.ShotsMax = 3;
            this.ProjectilesPerShot = 10;
        }
        public override void OnTurretInstanceCreated()
        {
            this.TurretInstance.TurretLights = this.TurretInstance.gameObject.GetComponentsInChildren<Light>();
            this.HeatLevelsPerBarrel = new float[3];
            this.GlowingTurretBarrelMaterials = new Material[3];
            for (int i = 0; i < 3; i++)
            {
                this.GlowingTurretBarrelMaterials[i] = this.TurretInstance.OptionalGameObjects[i].GetComponent<Renderer>().material;
                this.HeatLevelsPerBarrel[i] = 0f;
            }
        }
        public override void Fire(int inProjID, Vector3 dir)
        {
            /*
			if (ShipStats.Ship.GetTurretAtID(AutoTurretID) != this) AutoTurretID += 10;
            base.Fire(inProjID, dir);
			*/
            if (this.TurretInstance != null && this.ShotsSinceReload < this.ShotsMax)
            {
                this.HeatLevelsPerBarrel[this.ShotsSinceReload] = 4f;
                this.TurretInstance.TurretAnimation.Play("Firebarrel" + (this.ShotsSinceReload + 1).ToString());
            }
            this.ShotsSinceReload++;
            if (this.ShotsSinceReload >= this.ShotsMax)
            {
                PLMusic.PostEvent("play_ship_generic_external_weapon_scattergun_shoot_third", this.TurretInstance.gameObject);
                this.TurretInstance.TurretAnimation.PlayQueued("Reload");
                this.ChargeAmount = 0f;
            }
            else
            {
                PLMusic.PostEvent("play_ship_generic_external_weapon_scattergun_shoot_first_second", this.TurretInstance.gameObject);
            }
            this.LastFireTime = Time.time;
            this.Heat += this.HeatGeneratedOnFire;
            PLRand plrand = new PLRand(inProjID);
            Collider[] array = new Collider[this.ProjectilesPerShot];
            for (int i = 0; i < this.ProjectilesPerShot; i++)
            {
                Vector3 normalized = new Vector3(plrand.Next(-1f, 1f), plrand.Next(-1f, 1f), plrand.Next(-1f, 1f)).normalized;
                GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this.TurretInstance.Proj, this.TurretInstance.FiringLoc.transform.position, this.TurretInstance.FiringLoc.transform.rotation);
                gameObject.GetComponent<Rigidbody>().velocity = base.ShipStats.Ship.Exterior.GetComponent<Rigidbody>().velocity + dir * this.m_ProjSpeed + normalized * this.m_ProjSpeed * 0.25f * (float)plrand.NextDouble() * Mathf.Clamp((float)plrand.NextDouble(), 0.5f, 1f);
                gameObject.GetComponent<PLProjectile>().ProjID = inProjID + i;
                gameObject.GetComponent<PLProjectile>().Damage = this.m_Damage * base.LevelMultiplier(0.15f, 1f) * base.ShipStats.TurretDamageFactor;
                gameObject.GetComponent<PLProjectile>().MaxLifetime = 3f;
                gameObject.GetComponent<PLProjectile>().OwnerShipID = base.ShipStats.Ship.ShipID;
                gameObject.GetComponent<PLProjectile>().TurretID = this.TurretID;
                gameObject.GetComponent<PLProjectile>().ExplodeOnMaxLifetime = false;
                gameObject.GetComponent<PLProjectile>().MyDamageType = EDamageType.E_PHYSICAL;
                if (base.ShipStats.Ship.GetExteriorMeshCollider() != null)
                {
                    Physics.IgnoreCollision(base.ShipStats.Ship.GetExteriorMeshCollider(), gameObject.GetComponent<Collider>());
                }
                array[i] = gameObject.GetComponent<Collider>();
                for (int j = i - 1; j >= 0; j--)
                {
                    Physics.IgnoreCollision(array[i], array[j]);
                }
                PLServer.Instance.m_ActiveProjectiles.Add(gameObject.GetComponent<PLProjectile>());
            }
            this.CurrentCameraShake += 1f;
            if (Time.time - base.ShipStats.Ship.LastCloakingSystemActivatedTime > 2f)
            {
                base.ShipStats.Ship.SetIsCloakingSystemActive(false);
            }
        }

        public override void InnerCheckFire()
        {
            /*
			if (ShipStats.Ship.GetTurretAtID(AutoTurretID) != this) AutoTurretID += 10;
			base.InnerCheckFire();
			*/
            PLServer.Instance.photonView.RPC("AutoTurretFire", PhotonTargets.All, new object[]
            {
            base.ShipStats.Ship.ShipID,
            this.AutoTurretID,
            PLServer.Instance.ServerProjIDCounter,
            Vector3.zero
            });
            PLServer.Instance.ServerProjIDCounter += this.ProjectilesPerShot;

        }
    }

    class AutoFlamelance : PLFlamelanceTurret
    {
        public AutoFlamelance(int inLevel = 0, int inSubTypeData = 0)
        {
            this.Name = "Automated Flamelance Turret";
            this.Desc = "An unusual weapon that uses a custom fuel mix to project a stream of fire into space. The heat applied to a target ship can start onboard fires and overheat its reactor. This is an automatic version.";
            this.ActualSlotType = ESlotType.E_COMP_AUTO_TURRET;
            this.m_SlotType = ESlotType.E_COMP_AUTO_TURRET;
            this.m_Damage = 83f;
            base.SubType = AutoTurretModManager.Instance.GetIDFromName("AutoFlamelanceTurret");
            this.m_MarketPrice = 27800;
            this.m_MaxPowerUsage_Watts = 3300f;
            this.m_ProjSpeed = 360f;
            this.FireDelay = 2.1f;
            this.TurretRange = 2200f;
            base.CargoVisualPrefabID = 3;
            base.Level = inLevel;
            this.FireTurretSoundSFX = "play_ship_generic_external_weapon_flamelance_fire";
            this.CreateFlameHitObjects = true;
            this.MyDamageType = EDamageType.E_FIRE;
        }
    }

    class AutoBioHazard : PLBioHazardTurret
    {
        public AutoBioHazard(int inLevel = 0, int inSubTypeData = 0) : base(0, 0)
        {
            this.Name = "Automated Bio-Hazard Turret";
            this.Desc = "Fires an acidic stream that poisons onboard crew and reduces a ship’s armor effectiveness for a short time. This is an automated version.";
            this.ActualSlotType = ESlotType.E_COMP_AUTO_TURRET;
            this.m_SlotType = ESlotType.E_COMP_AUTO_TURRET;
            this.m_Damage = 53f;
            base.SubType = AutoTurretModManager.Instance.GetIDFromName("AutoBioHazardTurret");
            this.m_MarketPrice = 37800;
            this.m_MaxPowerUsage_Watts = 2739f;
            this.m_ProjSpeed = 180f;
            this.FireDelay = 1.5f;
            this.TurretRange = 2200f;
            base.CargoVisualPrefabID = 3;
            base.Level = inLevel;
            this.MyDamageType = EDamageType.E_BIOHAZARD;
        }
    }


    [HarmonyPatch(typeof(PLTurret),"Tick")]
    class FixNonTargetting 
    {
        static void Postfix(PLTurret __instance) 
        {
            if (__instance.IsEquipped && __instance.TurretInstance != null)
            {
                Vector3 euler = __instance.targetWorldRot.eulerAngles;
                if (float.IsNaN(euler.x) || float.IsNaN(euler.y) || float.IsNaN(euler.z))
                {
                    __instance.targetWorldRot.eulerAngles = new Vector3(0, 0, 0);
                }
            }
        }
    }
}
