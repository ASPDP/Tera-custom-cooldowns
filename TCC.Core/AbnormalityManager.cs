﻿using System;
using System.Linq;
using System.Windows;
using TCC.Data;
using TCC.Parsing.Messages;
using TCC.ViewModels;

namespace TCC
{
    public delegate void UpdateAbnormalityEventHandler(ulong target, Abnormality ab, int duration, int stacks);

    public static class AbnormalityManager
    {
        public const double PLAYER_AB_SIZE = 32;
        public const double PARTY_AB_SIZE = 28;
        public const double RAID_AB_SIZE = 24;
        public const double BOSS_AB_SIZE = 30;
        public const double PLAYER_AB_LEFT_MARGIN = 2;
        public const double PARTY_AB_LEFT_MARGIN = 1;
        public const double RAID_AB_LEFT_MARGIN = -9;
        public const double BOSS_AB_LEFT_MARGIN = 2;

        public static void BeginAbnormality(uint id, ulong target, uint duration, int stacks)
        {
            if (AbnormalityDatabase.Abnormalities.TryGetValue(id, out Abnormality ab))
            {
                if (!Filter(ab)) return;
                if (target == SessionManager.CurrentPlayer.EntityId)
                {
                    BeginPlayerAbnormality(ab, stacks, duration, target);
                    GroupWindowManager.Instance.BeginOrRefreshUserAbnormality(ab, stacks, duration, SessionManager.CurrentPlayer.PlayerId, SessionManager.CurrentPlayer.ServerId);
                }
                else
                {
                    BeginNPCAbnormality(ab, stacks, duration, target);
                }
            }
        }
        public static void EndAbnormality(ulong target, uint id)
        {
            if (AbnormalityDatabase.Abnormalities.TryGetValue(id, out Abnormality ab))
            {
                if (target == SessionManager.CurrentPlayer.EntityId)
                {
                    EndPlayerAbnormality(ab);
                    GroupWindowManager.Instance.EndUserAbnormality(ab, SessionManager.CurrentPlayer.PlayerId, SessionManager.CurrentPlayer.ServerId);

                }
                //else if (EntitiesManager.TryGetBossById(target, out Boss b))
                //{
                //    b.EndBuff(ab);
                //}
                else
                {
                    BossGageWindowManager.Instance.EndNpcAbnormality(target, ab);
                }
            }

        }
        static void BeginPlayerAbnormality(Abnormality ab, int stacks, uint duration, ulong target)
        {
            if (ab.Type == AbnormalityType.Buff)
            {
                if (ab.Infinity)
                {
                    BuffBarWindowManager.Instance.Player.AddOrRefreshInfBuff(ab, duration, stacks, PLAYER_AB_SIZE, PLAYER_AB_LEFT_MARGIN);

                }
                else
                {
                    BuffBarWindowManager.Instance.Player.AddOrRefreshBuff(ab, duration, stacks, PLAYER_AB_SIZE, PLAYER_AB_LEFT_MARGIN);
                }
            }
            else
            {
                BuffBarWindowManager.Instance.Player.AddOrRefreshDebuff(ab, duration, stacks, PLAYER_AB_SIZE, PLAYER_AB_LEFT_MARGIN);
                CharacterWindowManager.Instance.Player.AddToDebuffList(ab);
                ClassManager.SetStatus(ab, true);

            }
        }
        static void EndPlayerAbnormality(Abnormality ab)
        {
            if (ab.Type == AbnormalityType.Buff)
            {
                if (ab.Infinity)
                {
                    BuffBarWindowManager.Instance.Player.RemoveInfBuff(ab);
                }
                else
                {

                    BuffBarWindowManager.Instance.Player.RemoveBuff(ab);
                }
            }
            else
            {
                BuffBarWindowManager.Instance.Player.RemoveDebuff(ab);
                CharacterWindowManager.Instance.Player.RemoveFromDebuffList(ab);
                ClassManager.SetStatus(ab, false);
            }
        }

        static void BeginNPCAbnormality(Abnormality ab, int stacks, uint duration, ulong target)
        {
            //if (EntitiesManager.TryGetBossById(target, out Boss b))
            //{
            //    b.AddorRefresh(ab, duration, stacks, BOSS_AB_SIZE, BOSS_AB_LEFT_MARGIN);
            //}
            BossGageWindowManager.Instance.AddOrRefreshNpcAbnormality(ab, stacks, duration, target, BOSS_AB_SIZE, BOSS_AB_LEFT_MARGIN);
        }
        static bool Filter(Abnormality ab)
        {
            if (ab.Name.Contains("BTS") || ab.ToolTip.Contains("BTS") || !ab.IsShow) return false;
            if (ab.Name.Contains("(Hidden)") || ab.Name.Equals("Unknown") || ab.Name.Equals(string.Empty)) return false;
            return true;
        }

        public static void BeginOrRefreshPartyMemberAbnormality(uint playerId, uint serverId, uint id, uint duration, int stacks)
        {
            if (AbnormalityDatabase.Abnormalities.TryGetValue(id, out Abnormality ab))
            {
                if (!Filter(ab)) return;
                GroupWindowManager.Instance.BeginOrRefreshUserAbnormality(ab, stacks, duration, playerId, serverId);
            }
        }

        internal static void EndPartyMemberAbnormality(uint playerId, uint serverId, uint id)
        {
            if (AbnormalityDatabase.Abnormalities.TryGetValue(id, out Abnormality ab))
            {
                if (!Filter(ab)) return;
                GroupWindowManager.Instance.EndUserAbnormality(ab, playerId, serverId);
            }
        }
    }
}
