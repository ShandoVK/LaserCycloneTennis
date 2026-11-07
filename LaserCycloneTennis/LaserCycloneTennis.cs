using BepInEx;
using BepInEx.Configuration;
using EntityStates.Huntress.HuntressWeapon;
using MonoMod.Cil;
using RoR2;
using System;
using UnityEngine;

namespace LaserCycloneTennis
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]

    public class LaserCycloneTennis : BaseUnityPlugin
    {
        public const string PluginGUID = "ShandoIsBad.LaserCycloneTennis";
        public const string PluginAuthor = "ShandoVK";
        public const string PluginName = "LaserCycloneTennis";
        public const string PluginVersion = "1.0.0";

        // Vanilla has no cap to the combo so it ends up hilariously powerful if you just endlessly juggle it
        private const int comboCap = 20;
        private static ConfigEntry<bool> uncapCombo;

        public void Awake()
        {
            Log.Init(Logger);

            uncapCombo = Config.Bind(
                "No Chocolate", 
                "Uncap combo", 
                false,
                $"If true, removes the {comboCap} combo restriction back to vanilla behavior. Vanilla has no upper limit so this may make the skill quite unbalanced if spammed.");

            IL.EntityStates.Huntress.HuntressWeapon.CatchCyclone.FixedUpdate += StockGraceWindow;
            On.EntityStates.Huntress.HuntressWeapon.ThrowCycloneKickable.FireLaserCyclone += RefundSecondary;
        }

        private static void StockGraceWindow(ILContext il)
        {
            var c = new ILCursor(il);
            int count = 0;
            while (c.TryGotoNext(x => x.MatchCallOrCallvirt<GenericSkill>("get_stock")))
            {
                c.Index++;
                c.EmitDelegate<Func<int, int>>(stock => Mathf.Max(stock, 1));
                count++;
            }
        }

        private static void RefundSecondary(On.EntityStates.Huntress.HuntressWeapon.ThrowCycloneKickable.orig_FireLaserCyclone orig, ThrowCycloneKickable self)
        {
            var secondary = self.skillLocator ? self.skillLocator.secondary : null;
            int prevStock = secondary ? secondary.stock : 0;

            byte realerCombo = self.comboCounter;
            if (!uncapCombo.Value && realerCombo > comboCap)
                self.comboCounter = (byte)comboCap;

            orig(self);

            self.comboCounter = realerCombo;

            if (!self.isAuthority || !self.isKick || !secondary) return;

            if (secondary.stock < prevStock) secondary.AddOneStock();
        }
    }
}
// 69, I'll probably punt ILSpy for ruining this joke if it bumps off the 69th line