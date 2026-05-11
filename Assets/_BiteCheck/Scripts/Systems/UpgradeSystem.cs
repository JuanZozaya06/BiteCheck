using System;
using UnityEngine;

namespace BiteCheck.Systems
{
    public class UpgradeSystem : MonoBehaviour
    {
        public const int BetterScannerCost = 50;
        public const int ReinforcedGateCost = 75;
        public const int PublicTrustCost = 75;

        private const float ReinforcedGateMultiplier = 0.8f;
        private const float PublicTrustMultiplier = 0.8f;

        [SerializeField] private StatsManager statsManager;
        [SerializeField] private bool betterScannerOwned;
        [SerializeField] private bool reinforcedGateOwned;
        [SerializeField] private bool publicTrustOwned;

        public event Action OnUpgradesChanged;

        public bool BetterScannerOwned => betterScannerOwned;
        public bool ReinforcedGateOwned => reinforcedGateOwned;
        public bool PublicTrustOwned => publicTrustOwned;

        public int ExtraVisibleClues => betterScannerOwned ? 1 : 0;

        private void Awake()
        {
            if (statsManager == null)
            {
                statsManager = FindFirstObjectByType<StatsManager>();
            }
        }

        public void ResetUpgrades()
        {
            betterScannerOwned = false;
            reinforcedGateOwned = false;
            publicTrustOwned = false;
            OnUpgradesChanged?.Invoke();
        }

        public bool CanBuyBetterScanner()
        {
            return !betterScannerOwned && CanSpend(BetterScannerCost);
        }

        public bool CanBuyReinforcedGate()
        {
            return !reinforcedGateOwned && CanSpend(ReinforcedGateCost);
        }

        public bool CanBuyPublicTrust()
        {
            return !publicTrustOwned && CanSpend(PublicTrustCost);
        }

        public bool BuyBetterScanner()
        {
            return TryBuy(ref betterScannerOwned, BetterScannerCost);
        }

        public bool BuyReinforcedGate()
        {
            return TryBuy(ref reinforcedGateOwned, ReinforcedGateCost);
        }

        public bool BuyPublicTrust()
        {
            return TryBuy(ref publicTrustOwned, PublicTrustCost);
        }

        public int ApplySecurityPenaltyReduction(int penalty)
        {
            return ApplyReduction(penalty, reinforcedGateOwned ? ReinforcedGateMultiplier : 1f);
        }

        public int ApplyMoralePenaltyReduction(int penalty)
        {
            return ApplyReduction(penalty, publicTrustOwned ? PublicTrustMultiplier : 1f);
        }

        private bool CanSpend(int cost)
        {
            return statsManager != null && statsManager.CanSpendResources(cost);
        }

        private bool TryBuy(ref bool owned, int cost)
        {
            if (owned || statsManager == null || !statsManager.TrySpendResources(cost))
            {
                return false;
            }

            owned = true;
            OnUpgradesChanged?.Invoke();
            return true;
        }

        private int ApplyReduction(int penalty, float multiplier)
        {
            return Mathf.Max(0, Mathf.CeilToInt(Mathf.Max(0, penalty) * multiplier));
        }
    }
}
