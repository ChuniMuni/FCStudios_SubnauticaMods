﻿using AE.SeaCooker.Buildable;
using AE.SeaCooker.Enumerators;
using AE.SeaCooker.Helpers;
using AE.SeaCooker.Mono;
using FCSCommon.Utilities;
using System;
using FCSTechFabricator.Components;
using UnityEngine;

namespace AE.SeaCooker.Managers
{
    internal class GasManager
    {
        private SeaCookerController _mono;
        private float _fuelLevel;
        private float _fuelCapacity;
        private Equipment _equipment;
        private bool _fuelInserted;
        private int _tank;
        private const int NoTank = 0;
        internal Action OnGasUpdate { get; set; }
        internal void Initialize(SeaCookerController mono)
        {
            _mono = mono;

            var equipmentRoot = new GameObject("SCEquipmentRoot");
            equipmentRoot.transform.SetParent(_mono.transform, false);
            equipmentRoot.AddComponent<ChildObjectIdentifier>();
            equipmentRoot.SetActive(false);

            _equipment = new Equipment(mono.gameObject, equipmentRoot.transform);
            _equipment.SetLabel(SeaCookerBuildable.GasContainerLabel());
            _equipment.isAllowedToAdd += IsAllowedToAdd;
            _equipment.isAllowedToRemove += IsAllowedToRemove;
            _equipment.onEquip += OnEquipmentAdded;
            _equipment.onUnequip += OnEquipmentRemoved;
            _equipment.AddSlot(Configuration.Configuration.SlotIDs[0]);

            _tank = Animator.StringToHash("Tank");
        }

        private void OnEquipmentRemoved(string slot, InventoryItem item)
        {
            QuickLogger.Debug("Removing Current Fuel");
            CurrentFuel = FuelType.None;
        }

        private void OnEquipmentAdded(string slot, InventoryItem item)
        {
            if (item.item.GetTechType() == TechTypeHelpers.GasTankTechType())
            {
                AddFuel(FuelType.Gas);
                return;
            }

            if (item.item.GetTechType() == TechTypeHelpers.AlienFecesTechType())
            {
                AddFuel(FuelType.AlienFeces);
                return;
            }
        }

        private void UpdateTank(FuelType value)
        {
            _mono.AnimationManager.SetIntHash(_tank, (int)value);
        }

        private bool IsAllowedToRemove(Pickupable pickupable, bool verbose)
        {
            return false;
        }

        private bool IsAllowedToAdd(Pickupable pickupable, bool verbose)
        {
            bool flag = false;
            if (pickupable != null)
            {
                var filter = pickupable.gameObject.GetComponent<FCSTechFabricatorTag>();
                if (filter != null)
                {
                    if (pickupable.GetTechType() == TechTypeHelpers.AlienFecesTechType())
                    {
                        flag = true;
                    }
                    if (pickupable.GetTechType() == TechTypeHelpers.GasTankTechType())
                    {
                        flag = true;
                    }
                }

            }

            QuickLogger.Debug($"Adding Item {flag} || {verbose}");

            if (!flag && verbose)
                ErrorMessage.AddMessage("Alterra Refrigeration Freon allowed only");
            return flag;
        }

        public void AddFuel(FuelType value)
        {
            QuickLogger.Debug("Adding Fuel");

            switch (value)
            {
                case FuelType.Gas:
                    CurrentFuel = FuelType.Gas;
                    break;
                case FuelType.AlienFeces:
                    CurrentFuel = FuelType.AlienFeces;
                    break;
                default:
                    CurrentFuel = FuelType.None;
                    throw new ArgumentOutOfRangeException(nameof(value), value, null);
            }
        }

        private FuelType _currentFuel;

        public FuelType CurrentFuel
        {
            get
            {
                if (_fuelLevel <= 0)
                {
                    QuickLogger.Info(SeaCookerBuildable.NoFuel(), true);
                }

                return _currentFuel;

            }
            set
            {
                _currentFuel = value;
                UpdateTank(value);
                _fuelInserted = value != FuelType.None;
                switch (value)
                {
                    case FuelType.None:
                        _fuelCapacity = _fuelLevel = 0f;
                        break;
                    case FuelType.Gas:
                        _fuelCapacity = _fuelLevel = QPatch.Configuration.Config.GasTankCapacity;
                        break;
                    case FuelType.AlienFeces:
                        _fuelCapacity = _fuelLevel = QPatch.Configuration.Config.AlienFecesTankCapacity;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(value), value, null);
                }
                OnGasUpdate?.Invoke();
                QuickLogger.Debug($"Fuel Level Set To: {_fuelLevel}");
            }
        }

        internal bool IsFuelInserted()
        {
            return _fuelInserted;
        }

        internal void RemoveGas(float amount)
        {

            _fuelLevel = Mathf.Clamp(_fuelLevel - amount, 0, _fuelCapacity);

            if (_fuelLevel <= 0)
            {
                if (!_equipment.GetFreeSlot(EquipmentType.Tank, out var result))
                {
                    QuickLogger.Debug("Clearing Fuel Slot");
                    _equipment.ClearItems();
                    CurrentFuel = FuelType.None;
                    _mono.AnimationManager.SetIntHash(_tank, NoTank);
                }
            }

            QuickLogger.Debug($"Removing Gas {amount} || Fuel Level {_fuelLevel} || Fuel Capacity {_fuelCapacity}");

            OnGasUpdate?.Invoke();
        }

        internal void SetTankLevel(float amount)
        {
            QuickLogger.Debug($"Setting Tank: {amount}");
            _fuelLevel = Mathf.Clamp(amount, 0, _fuelCapacity);
            _mono.DisplayManager.UpdateFuelPercentage();
        }

        internal float GetTankLevel()
        {
            return _fuelLevel;
        }

        internal float GetTankPercentage()
        {
            return Mathf.RoundToInt(_fuelLevel / _fuelCapacity * 100);
        }

        internal void OpenFuelTank()
        {
            QuickLogger.Debug("Modules Door Opened", true);
            PDA pda = Player.main.GetPDA();
            if (!pda.isInUse)
            {
                if (_equipment == null)
                {
                    QuickLogger.Error("Equipment returned null");
                    return;
                }
                QuickLogger.Debug("0");
                Inventory.main.SetUsedStorage(_equipment, false);
                QuickLogger.Debug("1");
                pda.Open(PDATab.Inventory, _mono.transform, null, 4f);
                QuickLogger.Debug("2");
            }
        }

        internal void SetEquipment(FuelType tank)
        {
            var techType = TechTypeHelpers.GetTechType(tank);

            if (techType == TechType.None) return;

            var getTank = CraftData.GetPrefabForTechType(techType);

#if SUBNAUTICA
            _equipment.AddItem(Configuration.Configuration.SlotIDs[0],
                new InventoryItem(getTank.GetComponent<Pickupable>().Pickup(false)));
#elif BELOWZERO
            Pickupable pickupable = getTank.GetComponent<Pickupable>();
            pickupable.Pickup(false);
            _equipment.AddItem(Configuration.Configuration.SlotIDs[0],
                new InventoryItem(pickupable));
#endif
        }

    }
}
