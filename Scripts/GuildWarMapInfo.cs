﻿using System;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG.MMO.GuildWar
{
    [CreateAssetMenu(fileName = "Guild War Map Info", menuName = "Create GameData/Guild War Map Info", order = -4799)]
    public class GuildWarMapInfo : BaseMapInfo
    {
        [Serializable]
        public struct EventTime
        {
            public bool isOn;
            [Range(0, 23)]
            public byte startTime;
            [Range(0, 23)]
            public byte endTime;
        }

        [Header("Event time settings")]
        public EventTime sunday;
        public EventTime monday;
        public EventTime tuesday;
        public EventTime wednesday;
        public EventTime thursday;
        public EventTime friday;
        public EventTime saturday;
        [Tooltip("Battle duration (minutes), if defender can defend castle within this duration, defender will win that round.")]
        public int battleDuration = 15;

        [Header("Announce messages")]
        public string eventStartedMessage = "Guild war started !!";
        public string eventEndedMessage = "Guild war ended !!";
        public string roundEndedMessage = "Current guild war round ended !!";
        [Tooltip("{0} is guild name")]
        public string defenderWinMessage = "{0} can defend the castle and win this round.";
        [Tooltip("{0} is guild name")]
        public string attackerWinMessage = "{0} can occupy the castle and win this round.";

        public override bool AutoRespawnWhenDead { get { return true; } }
        public override bool IsInstanceMap { get { return true; } }

        public bool IsOn
        {
            get
            {
                EventTime eventTime;
                switch (DateTime.Now.DayOfWeek)
                {
                    case DayOfWeek.Sunday:
                        eventTime = sunday;
                        break;
                    case DayOfWeek.Monday:
                        eventTime = monday;
                        break;
                    case DayOfWeek.Tuesday:
                        eventTime = tuesday;
                        break;
                    case DayOfWeek.Wednesday:
                        eventTime = wednesday;
                        break;
                    case DayOfWeek.Thursday:
                        eventTime = thursday;
                        break;
                    case DayOfWeek.Friday:
                        eventTime = friday;
                        break;
                    case DayOfWeek.Saturday:
                        eventTime = saturday;
                        break;
                    default:
                        eventTime = sunday;
                        break;
                }
                return eventTime.isOn && DateTime.Now.Hour >= eventTime.startTime && DateTime.Now.Hour < eventTime.endTime;
            }
        }

        protected override bool IsPlayerAlly(BasePlayerCharacterEntity playerCharacter, BaseCharacterEntity targetCharacter)
        {
            if (targetCharacter == null)
                return false;

            if (targetCharacter is BasePlayerCharacterEntity)
            {
                BasePlayerCharacterEntity targetPlayer = targetCharacter as BasePlayerCharacterEntity;
                return targetPlayer.GuildId != 0 && targetPlayer.GuildId == playerCharacter.GuildId;
            }

            if (targetCharacter is GuildWarMonsterCharacterEntity)
            {
                return BaseGameNetworkManager.Singleton.DefenderGuildId != 0 && BaseGameNetworkManager.Singleton.DefenderGuildId == playerCharacter.GuildId;
            }

            if (targetCharacter is BaseMonsterCharacterEntity)
            {
                // If this character is summoner so it is ally
                BaseMonsterCharacterEntity targetMonster = targetCharacter as BaseMonsterCharacterEntity;
                if (targetMonster.IsSummoned)
                {
                    // If summoned by someone, will have same allies with summoner
                    return targetMonster.Summoner.IsAlly(playerCharacter);
                }
                else
                {
                    // Monster always not player's ally
                    return false;
                }
            }

            return false;
        }

        protected override bool IsMonsterAlly(BaseMonsterCharacterEntity monsterCharacter, BaseCharacterEntity targetCharacter)
        {
            if (targetCharacter == null)
                return false;

            if (monsterCharacter is GuildWarMonsterCharacterEntity)
            {
                if (targetCharacter is BasePlayerCharacterEntity)
                {
                    BasePlayerCharacterEntity targetPlayer = targetCharacter as BasePlayerCharacterEntity;
                    return BaseGameNetworkManager.Singleton.DefenderGuildId != 0 && BaseGameNetworkManager.Singleton.DefenderGuildId == targetPlayer.GuildId;
                }

                if (targetCharacter is BaseMonsterCharacterEntity)
                {
                    // If another monster has same allyId so it is ally
                    BaseMonsterCharacterEntity targetMonster = targetCharacter as BaseMonsterCharacterEntity;
                    if (targetMonster.IsSummoned)
                        return monsterCharacter.IsAlly(targetMonster.Summoner);
                    return false;
                }

                return true;
            }

            if (monsterCharacter.IsSummoned)
            {
                // If summoned by someone, will have same allies with summoner
                return targetCharacter == monsterCharacter.Summoner || monsterCharacter.Summoner.IsAlly(targetCharacter);
            }

            if (targetCharacter is GuildWarMonsterCharacterEntity)
            {
                // Monsters won't attack castle heart
                return true;
            }

            if (targetCharacter is BaseMonsterCharacterEntity)
            {
                // If another monster has same allyId so it is ally
                BaseMonsterCharacterEntity targetMonster = targetCharacter as BaseMonsterCharacterEntity;
                if (targetMonster.IsSummoned)
                    return monsterCharacter.IsAlly(targetMonster.Summoner);
                return targetMonster.CharacterDatabase.AllyId == monsterCharacter.CharacterDatabase.AllyId;
            }

            return false;
        }

        protected override bool IsPlayerEnemy(BasePlayerCharacterEntity playerCharacter, BaseCharacterEntity targetCharacter)
        {
            if (targetCharacter == null)
                return false;

            if (targetCharacter is BasePlayerCharacterEntity)
            {
                BasePlayerCharacterEntity targetPlayer = targetCharacter as BasePlayerCharacterEntity;
                return targetPlayer.GuildId == 0 || targetPlayer.GuildId != playerCharacter.GuildId;
            }

            if (targetCharacter is GuildWarMonsterCharacterEntity)
            {
                return BaseGameNetworkManager.Singleton.DefenderGuildId == 0 || BaseGameNetworkManager.Singleton.DefenderGuildId != playerCharacter.GuildId;
            }

            if (targetCharacter is BaseMonsterCharacterEntity)
            {
                // If this character is not summoner so it is enemy
                BaseMonsterCharacterEntity targetMonster = targetCharacter as BaseMonsterCharacterEntity;
                if (targetMonster.IsSummoned)
                {
                    // If summoned by someone, will have same enemies with summoner
                    return targetMonster.Summoner.IsEnemy(playerCharacter);
                }
                else
                {
                    // Monster always be player's enemy
                    return true;
                }
            }

            return false;
        }

        protected override bool IsMonsterEnemy(BaseMonsterCharacterEntity monsterCharacter, BaseCharacterEntity targetCharacter)
        {
            if (targetCharacter == null)
                return false;

            if (monsterCharacter is GuildWarMonsterCharacterEntity)
            {
                if (targetCharacter is BasePlayerCharacterEntity)
                {
                    BasePlayerCharacterEntity targetPlayer = targetCharacter as BasePlayerCharacterEntity;
                    return BaseGameNetworkManager.Singleton.DefenderGuildId == 0 || BaseGameNetworkManager.Singleton.DefenderGuildId != targetPlayer.GuildId;
                }

                if (targetCharacter is BaseMonsterCharacterEntity)
                {
                    // If another monster has same allyId so it is ally
                    BaseMonsterCharacterEntity targetMonster = targetCharacter as BaseMonsterCharacterEntity;
                    if (targetMonster.IsSummoned)
                        return monsterCharacter.IsEnemy(targetMonster.Summoner);
                    return false;
                }

                return false;
            }

            if (monsterCharacter.IsSummoned)
            {
                // If summoned by someone, will have same enemies with summoner
                return targetCharacter != monsterCharacter.Summoner && monsterCharacter.Summoner.IsEnemy(targetCharacter);
            }

            // Attack only player by default
            return targetCharacter is BasePlayerCharacterEntity;
        }
    }
}
