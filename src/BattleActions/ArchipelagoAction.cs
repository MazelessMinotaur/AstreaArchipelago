using Astrea;
using Astrea.BattleActions;
using Astrea.GameElements;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace AstreaArchipelago.src.BattleActions
{
    internal class ArchipelagoAction : BattleAction
    {
        public override void SetupModdedBattleAction()
        {
            base.SetupModdedBattleAction("ArchipelagoAction",
                "Grant a player a archipelago item",
                Astrea.BattleActionInteractionEnum.NONE,
                DiceActionLogicsList.DiceActionLogicEnum.DiceActionLogic_NoTarget,
                ModHelper.Instance.GetModSprite(Assembly.GetExecutingAssembly(),
                "archipelago.png"
                ));
        }

        public override void Cast(GameObject source)
        {
            base.Cast(source);
        }


    }
}
