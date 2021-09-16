
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace WordOfMouth
{
    public class ModEntry : Mod
    {
        private Dictionary<string, int> _StartOfDayFriendShip;
        private RelationMapper _friendsMap;
        private RelationMapper _familyMap;
        private ModConfig _Config;
        public override void Entry(IModHelper helper)
        {           
            this.Helper.Events.GameLoop.SaveLoaded += SetBaseDailyFriendShipLevel;
            this.Helper.Events.GameLoop.DayEnding += ApplyWordOfMouth;
            _StartOfDayFriendShip = new Dictionary<string, int>();
            _familyMap = this.Helper.Data.ReadJsonFile<RelationMapper>("FamilyMapping.json");
            _friendsMap = this.Helper.Data.ReadJsonFile<RelationMapper>("FriendsMapping.json");
            _Config = this.Helper.ReadConfig<ModConfig>();
        }

        private void ApplyWordOfMouth(object sender, DayEndingEventArgs e)
        {
            Farmer farmer = Game1.MasterPlayer;
            ApplyWordOfMouthForFarmer(farmer);
        }

        private void ApplyWordOfMouthForFarmer(Farmer farmer)
        {
            Dictionary<string, int> secondHandFriendshipValues = new Dictionary<string, int>();

            AddPeopleThatNeedSecondHandValues(farmer, secondHandFriendshipValues);
            ApplyWordOfMouthValues(farmer, secondHandFriendshipValues);
        }

        private void AddPeopleThatNeedSecondHandValues(Farmer farmer, Dictionary<string, int> secondHandFriendshipValues)
        {
            foreach (string npcName in _StartOfDayFriendShip.Keys)
            {
                var friendShipLevel = farmer.getFriendshipLevelForNPC(npcName);
                int OldLevel = _StartOfDayFriendShip[npcName];
                if (OldLevel != friendShipLevel)
                {
                    CheckForPairs(secondHandFriendshipValues, npcName, friendShipLevel, OldLevel, _friendsMap.Relationships);
                    CheckForPairs(secondHandFriendshipValues, npcName, friendShipLevel, OldLevel, _familyMap.Relationships);
                }

            }
        }

        private void CheckForPairs(Dictionary<string, int> secondHandFriendshipValues, 
                                string npcName,
                                int friendShipLevel,
                                int OldLevel,
                                IEnumerable<Relationship> relationships)
        {
            foreach (Relationship relationship in relationships)
            {

                if (relationship.Pair[0] == npcName)
                {
                    AddToWordOfMouth(secondHandFriendshipValues, friendShipLevel, OldLevel, relationship.Pair[1]);
                }
                else if (relationship.Pair[1] == npcName)
                {
                    AddToWordOfMouth(secondHandFriendshipValues, friendShipLevel, OldLevel, relationship.Pair[0]);
                }
            }
        }

        private void AddToWordOfMouth(Dictionary<string, int> secondHandFriendshipValues, int friendShipLevel, int OldLevel, string NPCToAddTo)
        {
            var WordOfMouthValue = GetWordOfMouthValue(friendShipLevel, OldLevel);
            if (secondHandFriendshipValues.ContainsKey(NPCToAddTo))
            {
                secondHandFriendshipValues[NPCToAddTo] = (secondHandFriendshipValues[NPCToAddTo] + WordOfMouthValue) / 2;
            }
            else
            {
                secondHandFriendshipValues.Add(NPCToAddTo, WordOfMouthValue);
            }
            
        }

        private static void ApplyWordOfMouthValues(Farmer farmer, Dictionary<string, int> secondHandFriendshipValues)
        {
            foreach (string npcName in secondHandFriendshipValues.Keys)
            {
                farmer.changeFriendship(secondHandFriendshipValues[npcName], Game1.getCharacterFromName(npcName));
            }
        }

        private int GetWordOfMouthValue(int friendShipLevel, int oldLevel)
        {
            return (int)((double)(friendShipLevel - oldLevel) * _Config.PercentageOfFriendship);
        }

        private void SetBaseDailyFriendShipLevel(object sender, SaveLoadedEventArgs e)
        {
            Farmer farmer = Game1.MasterPlayer;        
            UpdateFriendShipDictionary(farmer);
        }


        private void UpdateFriendShipDictionary(Farmer farmer)
        {
            DisposableList<NPC> NPCS = Utility.getAllCharacters();
            foreach(NPC npc in NPCS)
            {

                var friendshipLevel = farmer.getFriendshipLevelForNPC(npc.name);
               if( _StartOfDayFriendShip.ContainsKey(npc.name))
                {
                    _StartOfDayFriendShip[npc.name] = friendshipLevel;
                }
               else
                {    
                    _StartOfDayFriendShip.Add(npc.name, friendshipLevel);
                }
            }
                        
        }
      
    }
}
