using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade;
using static TaleWorlds.Library.MathF;

namespace SmithyStaminaRegen
{
    public class SubModule : MBSubModuleBase
    {
        protected override void OnSubModuleLoad()
        {
            base.OnSubModuleLoad();

        }
        protected override void OnGameStart(Game game, IGameStarter gameStarterObject)
        {
            base.OnGameStart(game, gameStarterObject);

            if (game.GameType is Campaign)
            {
                var campaignStarter = (CampaignGameStarter)gameStarterObject;
                campaignStarter.AddBehavior(new PassiveSmithingStaminaRegenBehavior());
            }
        }

    }
    public class PassiveSmithingStaminaRegenBehavior : CampaignBehaviorBase
    {
        public override void RegisterEvents()
        {
            CampaignEvents.HourlyTickEvent.AddNonSerializedListener(this, OnHourlyTick);
        }

        public override void SyncData(IDataStore dataStore)
        {
        }

        private void OnHourlyTick()
        {
            try
            {
                var campaign = Campaign.Current;
                if (campaign == null) return;

                var craftingBehavior = campaign.GetCampaignBehavior<CraftingCampaignBehavior>();
                if (craftingBehavior == null) return;
                IEnumerable<Hero> allHeroes = campaign.AliveHeroes;
                //int i = 0;
                //int totalCount = campaign.AliveHeroes.Count;
                foreach (Hero hero in allHeroes)
                {
                    //i += 1;
                    //if ((totalCount - 10) <= i)
                    //{
                    //    InformationManager.DisplayMessage(new InformationMessage(string.Format("Hero - {0} Total - {1} - Curr index - {2}", hero.Name, campaign.AliveHeroes.Count, i)));
                    //}
                    if (hero == null || hero.IsDead || hero.IsPrisoner || hero.PartyBelongedTo == null)
                        continue;

                    if (hero.CurrentSettlement != null)//Skip heroes in settlement
                        continue;

                    int currentStamina = craftingBehavior.GetHeroCraftingStamina(hero);
                    int maxStamina = GetMaxHeroCraftingStamina(hero);

                    if (currentStamina < maxStamina)
                    {
                        int regen = GetStaminaHourlyRecoveryRate(hero);
                        int newStamina = Math.Min(currentStamina + regen, maxStamina);
                        craftingBehavior.SetHeroCraftingStamina(hero, newStamina);
                    }
                }
            } catch (Exception ex)
            {
                InformationManager.DisplayMessage(new InformationMessage($"Smithy mod error: {ex.Message}"));
            }
        }
        public int GetMaxHeroCraftingStamina(Hero hero)
        {
            return 100 + TaleWorlds.Library.MathF.Round((float)hero.GetSkillValue(DefaultSkills.Crafting) * 0.5f);
        }

        private int GetStaminaHourlyRecoveryRate(Hero hero)
        {
            int num = 5 + TaleWorlds.Library.MathF.Round((float)hero.GetSkillValue(DefaultSkills.Crafting) * 0.025f);
            if (hero.GetPerkValue(DefaultPerks.Athletics.Stamina))
            {
                num += TaleWorlds.Library.MathF.Round((float)num * DefaultPerks.Athletics.Stamina.PrimaryBonus);
            }
            return num;
        }
    }
}



//namespace SmithyStaminaRegen
//{
//    public class SubModule : MBSubModuleBase
//    {
//        protected override void OnSubModuleLoad()
//        {
//            base.OnSubModuleLoad();

//        }
//        protected override void OnGameStart(Game game, IGameStarter gameStarterObject)
//        {
//            base.OnGameStart(game, gameStarterObject);

//            if (game.GameType is Campaign)
//            {
//                var campaignStarter = (CampaignGameStarter)gameStarterObject;
//                campaignStarter.AddBehavior(new PassiveSmithingStaminaRegenBehavior());
//            }
//        }

//    }

//    public class PassiveSmithingStaminaRegenBehavior : CampaignBehaviorBase
//    {
//        public override void RegisterEvents()
//        {
//            // Tick once per in-game hour for smooth regeneration
//            CampaignEvents.HourlyTickEvent.AddNonSerializedListener(this, OnHourlyTick);
//        }

//        public override void SyncData(IDataStore dataStore) { }

//        private void OnHourlyTick()
//        {
//            Hero hero = Hero.MainHero;
//            if (hero?.GetSkillValue(DefaultSkills.Crafting) == null)
//                return;

//            // Get the default model's regen amount (per day)
//            float dailyRegen = hero.GetDailyEnergyRegenerationAmount();
//            float hourlyRegen = dailyRegen / 24f;

//            // Apply it every hour just like it would in-town
//            var smithing = hero.GetSkillValue(DefaultSkills.Crafting);
//            smithing.CurrentEnergy = MBMath.ClampFloat(smithing.CurrentEnergy + hourlyRegen, 0f, smithing.MaxEnergy);
//        }
//    }
//}