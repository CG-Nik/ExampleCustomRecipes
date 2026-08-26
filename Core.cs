using Alta;
using Alta.Inventory;
using MelonLoader;
using MelonLoader.Utils;
using System.Reflection;
using UnityEngine;
using CustomRecipesAPI;
using Alta.Blacksmithing;

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
            MouldDefinition mouldDefinition = assetBundle.LoadAsset<MouldDefinition>("Handle Medium Cool Mould Example.asset");
            // to make this in the inspector, copy an existing MouldItemComponent and modify its values
            // shouldn't be loaded here if the Item already has a MouldItemComponent (if it's a custom item, you should add the MouldItemComponent in the inspector)
            MouldItemComponent mouldItemComponent = assetBundle.LoadAsset<MouldItemComponent>("Handle Medium Cool MIC Example.asset");
            // to make this in the inspector, copy an existing SmeltingRepice and modify its values
            SmeltingRecipe smeltingRecipe = assetBundle.LoadAsset<SmeltingRecipe>("Evinon Steel Decraft Example.asset");

            // instead of having you manually do a bunch of stuff in a specific way, you literally just need to call this one method (for Moulds)
            // itemHash refers to the hash of the item you're making a Mould for
            // mouldDefinition should be the MouldDefinition you made in the inspector for this item
            // mouldItemComponent should be the MouldItemComponent you made in the inspector for this item
                // if you made a custom Item and the MouldItemComponent was added in the inspector, or this Item already has a MouldItemComponent, don't set mouldItemComponent (or make it null)
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
            CustomRecipesAPI.Core.SetUpMould(25450u, mouldDefinition, mouldItemComponent, true, false, new Vector3(0f, -0.2f, -0.7f), null);

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

            // instead of needing to manually do a bunch of stuff, you can just call this to set up a SmeltingRecipe
            // smeltingRecipe is the SmeltingRecipe that you're setting up
            // inputs is an array of the Items that are inputs for the recipe, in the order that they were set in the inspector
                // this is used to overwrite what you set in the inspector, as to not create an entirely new instance
                // this technically means you can just not set these in the inspector, but it's useful to know what your recipes actually do while you're in the inspector
                // you still need the counts of the items set correctly, though, as this JUST overwrites the Items
                // as you can see, I set up an enumerator and a dictionary that you can use to "easily" get the Item for a given ore or ingot that the base game has
            // outputs is the same thing, but for the outputs
            // smelterUpgrades is the SmelterUpgrades that you want the recipe to be unlocked by
                // yes, for whatever reason the actual class is called "SmelterUpgrades", so it looks like its plural even if its singular, and the singular and the plural are the same
                // in the base game, there are 5
                    // there's one for the default upgrades
                    // there's one for each gem from the combat trials
                    // there's "simple server default upgrades", which is for Quest, and includes all of them for free since Quest can't get the gems in normal gameplay
                // you can make this null to not add it to any SmelterUpgrades
            // addToSimpleServerDefaultUpgrades determines whether or not to add this recipe to the simple server default upgrades SmelterUpgrades automatically or not
                // defaults to true, so you can leave it not set to automatically give Quest players the recipe for free once Quest releases
                // you should only make this false if there's some way for Quest players to obtain the SmelterUpgrades that this recipe is on
            CustomRecipesAPI.Core.SetUpSmeltingRecipe(
                smeltingRecipe,
                new Item[]
                {
                    CustomRecipesAPI.Core.VanillaOreAndIngotItems[(int)CustomRecipesAPI.Core.VanillaOreAndIngotIndexers.EvinonSteelIngot],
                    crystalShardBlue
                },
                new Item[]
                {
                    CustomRecipesAPI.Core.VanillaOreAndIngotItems[(int)CustomRecipesAPI.Core.VanillaOreAndIngotIndexers.SilverIngot],
                    CustomRecipesAPI.Core.VanillaOreAndIngotItems[(int)CustomRecipesAPI.Core.VanillaOreAndIngotIndexers.MythrilIngot]
                },
                smelterUpgrades_gem3,
                true
            );
        }
    }
}