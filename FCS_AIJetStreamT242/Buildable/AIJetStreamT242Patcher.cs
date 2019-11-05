﻿using FCS_AIJetStreamT242.Mono;
using FCS_AIMarineTurbine.Configuration;
using FCS_AIMarineTurbine.Display.Patching;
using FCS_AIMarineTurbine.Mono;
using FCSCommon.Components;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using FCSTechFabricator.Models;
using Oculus.Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace FCS_AIMarineTurbine.Buildable
{
    using SMLHelper.V2.Assets;
    using SMLHelper.V2.Crafting;

    internal partial class AIJetStreamT242Buildable : Buildable
    {
        private static readonly AIJetStreamT242Buildable Singleton = new AIJetStreamT242Buildable();
        public override TechGroup GroupForPDA { get; } = TechGroup.ExteriorModules;
        public override TechCategory CategoryForPDA { get; } = TechCategory.ExteriorModule;
        public override string AssetsFolder { get; } = $"FCSAIMarineTurbine/Assets";
        public static void PatchSMLHelper()
        {
            if (!Singleton.GetPrefabs())
            {
                throw new FileNotFoundException($"Failed to retrieve the {Singleton.FriendlyName} prefab from the asset bundle");
            }

            Singleton.Patch();

            string savedDataJson = File.ReadAllText(Path.Combine(AssetHelper.GetConfigFolder($"FCSAIMarineTurbine"), $"{Singleton.ClassID}.json")).Trim();
            JetStreamT242Config = JsonConvert.DeserializeObject<JetStreamT242Config>(savedDataJson);
            QuickLogger.Debug($"Biome Speeds Count {JetStreamT242Config.BiomeSpeeds.Count}");
        }
        public static JetStreamT242Config JetStreamT242Config { get; set; }

        public AIJetStreamT242Buildable() : base(FCSTechFabricator.Configuration.AIJetStreamT242ClassID,
            "AI JetStreamT242",
            "The Jet Stream T242 provides power by using the water current. The faster the turbine spins the more power output.")
        {
            OnFinishedPatching += DisplayLanguagePatching.AdditionPatching;
        }

        public override GameObject GetGameObject()
        {
            GameObject prefab = null;

            try
            {
                QuickLogger.Debug("Making GameObject");

                QuickLogger.Debug("Instantiate GameObject");

                prefab = GameObject.Instantiate(_Prefab);
                var transmitter = GameObject.Instantiate(CraftData.GetPrefabForTechType(TechType.PowerTransmitter));


                // Add large world entity ALLOWS YOU TO SAVE ON TERRAIN
                var lwe = prefab.AddComponent<LargeWorldEntity>();
                lwe.cellLevel = LargeWorldEntity.CellLevel.Far;

                var model = prefab.FindChild("model");

                SkyApplier skyApplier = prefab.AddComponent<SkyApplier>();
                skyApplier.renderers = model.GetComponentsInChildren<MeshRenderer>();
                skyApplier.anchorSky = Skies.Auto;

                //Shader shader = Shader.Find("MarmosetUBER");
                //Renderer[] renderers = prefab.GetComponentsInChildren<Renderer>();
                //foreach (Renderer renderer in renderers)
                //{
                //    renderer.material.shader = shader;
                //}

                //========== Allows the building animation and material colors ==========// 

                QuickLogger.Debug("Adding Constructible");

                // Add constructible
                var constructable = prefab.AddComponent<Constructable>();
                constructable.allowedOnWall = false;
                constructable.allowedOnGround = true;
                constructable.allowedInSub = false;
                constructable.allowedInBase = false;
                constructable.allowedOnCeiling = false;
                constructable.allowedOutside = true;
                constructable.model = model;
                constructable.rotationEnabled = true;
                constructable.techType = TechType;

                prefab.AddComponent<PowerPlug>();

                prefab.AddComponent<LiveMixin>();

                PrefabIdentifier prefabID = prefab.AddComponent<PrefabIdentifier>();
                prefabID.ClassId = this.ClassID;

                prefab.AddComponent<TechTag>().type = TechType;

                prefab.AddComponent<BeaconController>();

                prefab.AddComponent<AIJetStreamT242PowerManager>();

                prefab.AddComponent<AIJetStreamT242HealthManager>();

                prefab.AddComponent<AIJetStreamT242AnimationManager>();

                prefab.AddComponent<AIJetStreamT242Controller>();
            }
            catch (Exception e)
            {
                QuickLogger.Error(e.Message);
            }

            return prefab;
        }

        protected override TechData GetBlueprintRecipe()
        {
            QuickLogger.Debug($"Creating recipe...");
            // Create and associate recipe to the new TechType
            var customFabRecipe = new TechData()
            {
                craftAmount = 1,
                Ingredients = new List<Ingredient>()
                {
                    new Ingredient(TechTypeHelpers.GetTechType("JetStreamT242Kit_MT"), 1)
                }
            };

            QuickLogger.Debug(ModTechTypes.JetStreamKit.ToString());
            QuickLogger.Debug($"Created Ingredients");
            return customFabRecipe;
        }
    }
}
