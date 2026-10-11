using Alta;
using Alta.Inventory;
using MelonLoader;
using MelonLoader.Utils;
using UnityEngine;
using CustomRecipesAPI;
using Alta.Blacksmithing;
using Alta.Carpentry;

[assembly: MelonInfo(typeof(ExampleCustomRecipes.Core), "ExampleCustomRecipes", "1.0.0", "CGNik", null)]
[assembly: MelonGame("Alta", "A Township Tale")]

namespace ExampleCustomRecipes
{
    public class Core : MelonMod
    {
        public override void OnInitializeMelon()
        {
            LoggerInstance.Msg("Initialized.");
            // subscribe to the hook where we set up our moulds
            CustomRecipesAPI.Core.SetUpRecipes += SetUpRecipes;
        }

        private void SetUpRecipes()
        {
            // load assetbundle containing needed gameobjects made in the inspector, and then load those gameobjects
            AssetBundle assetBundle = AssetBundle.LoadFromFile(Path.Combine(MelonEnvironment.ModsDirectory, "ExampleCustomRecipes/AssetBundles/!examplecustomrecipes"));
            // load our gameobjects that were made in the inspector
            // to make this in the inspector, copy an existing MouldDefinition and modify its values
            // the graphic will need to have its values of position, rotation, and scale set manually
            // to easily do this, keep some placeholders in the asset at first, go ingame with the unfinished asset and tweak the graphic position with UnityExplorer, then rebuild the asset with those new graphic positioning values
            // you also need to take note of any values that you set in the inspector that reference objects from the base game, and set those to None, then instead set them in C#
            // the only exception is the graphic of the MouldDefinition, since you can't reasonably set that in C#
            // the reasoning behind this is because the game doesn't maintain that reference, so it creates an entirely new instance of that object
            // in order to prevent it from USING the new instance, it sets the field to the correct object, but to prevent creating the object to begin with, you need to not reference the object in the inspector
            // do note, you CAN safely reference objects from the same asset bundle
            MouldDefinition mouldDefinition = assetBundle.LoadAsset<MouldDefinition>("Handle Medium Cool Mould Example.asset");

            // instead of having you manually do a bunch of stuff in a specific way, you literally just need to call this one method (for Moulds)
            // item refers to the Item the mould is for
            // mouldDefinition should be the MouldDefinition you made in the inspector for this item
            // addMouldItemComponent, if true, will automatically add a MouldItemComponent to the item you're making the Mould for
            // if you made a custom Item and the MouldItemComponent was added in the inspector, or this Item already has a MouldItemComponent, don't add another MouldItemComponent
            // addToStandardPress is whether or not this item will be added to the Included Items for the filter that the Standard Mould Press uses, making it usable on the Standard Mould Press
            // if this item would already be on the filter (having the Weapon Part Standard tag already), or shouldn't be allowed to be turned into a Standard Mould, don't set addToStandardPress (or make it false)
            // the specifics for the default filter for the Standard Mould Press are that the item must have the Weapon Part Standard tag and not the Weapon Part Hebios tag
            // addToHebiosPress is the same thing but for the Hebios Mould Press
            // the specifics for the default filter for the Hebios Mould Press are that the item must have the Weapon Part Hebios tag
            // positionOffset is a Vector3 that determines the offset in position that the item will spawn with out of the Smelter
            // some items are set up in a way that causes them to spawn inside the Smelter if their spawn position isn't changed
            // if you aren't modifying the item's position, don't set positionOffset (or make it null)
            // rotationOffset is a Vector3 that determines the offset in rotation that the item will spawn with out of the Smelter
            // if you aren't modifying the item's position, don't set rotationOffset (or make it null)
            
            CustomRecipesAPI.Core.SetUpMould(Item.All.Where(item => item.Hash == 25450u).First(), mouldDefinition, true, true, false, new Vector3(0f, -0.2f, -0.7f), null);

            // grab the 3rd Combat Trial's SmelterUpgrades for later use, i'd suggest doing this for any SmelterUpgrades you plan to add a SmeltingRecipe to
            SmelterUpgrades smelterUpgrades_gem3 = SmelterUpgrades.All.Where(upgrade => upgrade.Hash == 33428u).First();
            // these are the hashes for the other two SmelterUpgrades for the Gems
            // SmelterUpgrades smelterUpgrades_gem1 = SmelterUpgrades.All.Where(upgrade => upgrade.Hash == 23872u).First();
            // SmelterUpgrades smelterUpgrades_gem2 = SmelterUpgrades.All.Where(upgrade => upgrade.Hash == 28650u).First();
            // you can also grab the default upgrades SmelterUpgrades, but I don't have the hash here

            // also grabbing Crystal Shard Blue for later use
            // since this recipe uses a non-ore and non-ingot item, being Crystal Shard Blue, we need a quick reference to it
            Item crystalShardBlue = Item.All.Where(item => item.Hash == 7824u).First();

            // adds Crystal Shard Blue to the filter that the Smelter uses for inputs
            // the smelter's filter is usually just for items tagged as either Ores or Ingots
            // since this recipe uses an item that isn't either of those, we need to manually add it to the filter
            // this list is used by CustomRecipesAPI to automatically to add Items to the Smelter's filter, and is similar to another list called itemsToAddToStandardMouldPress which is kind of internally used by SetUpMould
            CustomRecipesAPI.Core.itemsToAddToSmelter.Add(crystalShardBlue);

            // creates a SmeltingRecipe through code
            // there's an old, outdated method involving AssetBundles, which has been replaced by this code-only alternative
            // you can still do the old method, but there's no benefit to it
            // name is the name that you'll give the SmeltingRecipe
            // hash is the hash that you'll give the SmeltingRecipe
            // duration is the time (in seconds) that the recipe takes
            // inputs is an array of the ItemCounts that are inputs for the recipe
            // you need to set both the Item and the amount of that Item that you need
            // as you can see, I set up an enumerator and a dictionary that you can use to easily get the Item for a given ore or ingot that the base game has
            // outputs is the same thing, but for the outputs
            // smelterUpgrades is the SmelterUpgrades that you want the recipe to be unlocked by
            // in the base game, there are 5
            // there's one for the default upgrades
            // there's one for each gem from the combat trials
            // there's "simple server default upgrades", which is for Quest, and includes all of them for free since Quest can't get the gems in normal gameplay
            // you can make this null to not add it to any SmelterUpgrades
            // addToSimpleServerDefaultUpgrades determines whether or not to add this recipe to the simple server default upgrades SmelterUpgrades automatically or not
            // defaults to true, so you can leave it not set to automatically give Quest players the recipe for free once Quest releases
            // you should only make this false if there's some way for Quest players to obtain the SmelterUpgrades that this recipe is on
            CustomRecipesAPI.Core.SetUpSmeltingRecipeThroughCode(
                "Evinon Steel Decraft",
                18062,
                10f,
                new ItemCount[]
                {
                    CustomRecipesAPI.Core.CreateItemCount(
                        CustomRecipesAPI.Core.VanillaOreAndIngotItems[(int)CustomRecipesAPI.Core.VanillaOreAndIngotIndexers.EvinonSteelIngot],
                        1
                    ),
                    CustomRecipesAPI.Core.CreateItemCount(
                        crystalShardBlue,
                        1
                    ),
                },
                new ItemCount[]
                {
                    CustomRecipesAPI.Core.CreateItemCount(
                        CustomRecipesAPI.Core.VanillaOreAndIngotItems[(int)CustomRecipesAPI.Core.VanillaOreAndIngotIndexers.SilverIngot],
                        1
                    ),
                    CustomRecipesAPI.Core.CreateItemCount(
                        CustomRecipesAPI.Core.VanillaOreAndIngotItems[(int)CustomRecipesAPI.Core.VanillaOreAndIngotIndexers.MythrilIngot],
                        2
                    ),
                },
                smelterUpgrades_gem3,
                true
            );

            // grabbing the Wooden Stirring Spoon item to use for its Glyph
            Item stirringSpoon = Item.All.Where(item => item.Hash == 41010u).First();
            // creates and sets up a ChiselDefinition for you, entirely through code
            // there was an old method involving AssetBundles, which is now outdated
                // it can still be done, but it's highly unrecommended due to having no real purpose and just being harder to do
            // hash is the hash you're giving to this ChiselDefinition
            // glyph is the glyph that the ChiselDefinition will have
            // size is the amount of wood you need for the recipe
                // use the ChiselBlockSize enum for this
                // Small is 6 wood wedges, Medium is 12 wood wedges, Large is 18 wood wedges
            // chiselOrder is the order of pieces that you chisel to do the recipe
                // this is from 0-11, NOT 1-12
                // try to ensure that this is unique from chisel orders defined by other mods
            // prefabs is the list of prefabs and their spawn positions that this ChiselDefinition will yield
                // you should try to make the prefabs spawn in a reasonable place
            CustomRecipesAPI.Core.SetUpChiselDefinitionThroughCode(
                "Stirring Spoon Recipe",
                12521,
                stirringSpoon.Glyph,
                ChiselBlockSize.Medium,
                [11, 10, 0, 1, 5, 3, 7, 2, 9, 4, 6, 8],
                [
                    CustomRecipesAPI.Core.CreatePrefabSpawnPosition(23950u, Vector3.zero, Quaternion.identity)
                ]
            );
        }
    }
}