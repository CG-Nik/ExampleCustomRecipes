using Alta;
using Alta.Inventory;
using MelonLoader;
using MelonLoader.Utils;
using System.Reflection;
using UnityEngine;

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
            CustomRecipesAPI.Core.SetUpMoulds += SetUpMoulds;
        }

        private void SetUpMoulds()
        {
            // load assetbundle containing needed gameobjects made in the inspector, and then load those gameobjects
            AssetBundle assetBundle = AssetBundle.LoadFromFile(Path.Combine(MelonEnvironment.ModsDirectory, "ExampleCustomRecipes/AssetBundles/!examplecustomrecipes"));
            // copy an existing MouldDefinition and modify its values
            // the graphic will need to have its values of position, rotation, and scale set manually
            // to easily do this, keep some placeholders in the asset at first, go ingame with the unfinished asset and tweak the graphic position with UnityExplorer, then rebuild the asset with those new graphic positioning values
            MouldDefinition mouldDefinition = assetBundle.LoadAsset<MouldDefinition>("Handle Medium Cool Mould.asset");
            // copy an existing MouldItemComponent and modify its value
            MouldItemComponent mouldItemComponent = assetBundle.LoadAsset<MouldItemComponent>("Handle Medium Cool MIC.asset");

            // grab the HandleMediumCool Item for later use
            Item handleMediumCool = Item.All.Where(item => item.Hash == 25450u).First();

            // grab the components list from HandleMediumCool and add our MouldItemComponent to it, so the game actually knows that this item can be turned into a mould
            List<ItemComponent> components = (List<ItemComponent>)typeof(Item).GetField("components", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(handleMediumCool);
            components.Add(mouldItemComponent);
            typeof(Item).GetField("components", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(handleMediumCool, components);

            // grab the AxeHeadCurve Mould MouldDefinition for later use
            MouldDefinition axeHeadCurveMould = MouldDefinition.All.Where(mould => mould.Hash == 22952u).First();

            // here, we'll set some values that we already set in the inspector again
            // the reason why is because the game creates new instances of some objects, instead of using the existing ones
            // this isn't really ideal, as it can break certain things within the game, as well as other mods
            // the values you would need to set again include everything that is an instance

            // set the allowedMaterials again, which is the field that determines what items you can use to make the item in the Mould
            // every Mould uses the "All Ingots" set, so you can use any Mould to copy the values with
            typeof(MouldDefinition).GetField("allowedMaterials", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(mouldDefinition,
                typeof(MouldDefinition).GetField("allowedMaterials", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(axeHeadCurveMould)
            );

            // you could set the graphic again, but this just isn't worth it in my opinion
            // the other stuff is really easy to find something to copy for, but it's going to be really hard to find some specific mesh
            // additionally, you don't really lose much by not setting this again, as I find it unlikely that another mod will be modifying the LOD0 mesh for some random item, and even if they did, it'd just be a visual issue
            /*
            typeof(MouldDefinition).GetField("graphic", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(mouldDefinition,
                null
            );
            */

            // set the item that the mould will actually produce again
            typeof(MouldDefinition).GetField("product", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(mouldDefinition,
                handleMediumCool
            );

            // uses an API method that automatically adds the MouldDefinition to the list of MouldDefinitions that the game has
            CustomRecipesAPI.Core.RegisterMouldDefinition(mouldDefinition);

            // adds the Item to a list of Items that CustomRecipesAPI has
            // this list is used to automatically add any items within it to the filter that the input dock of the Mould Press uses, allowing players to actually use the Mould Press with this item
            // you don't need to do this if the Item you're making a Mould for already has the Weapon Part Standard tag (and doesn't have the Weapon Part Hebios tag), which causes the Mould Press to automatically allow it anyways
            // there exists another list that CustomRecipesAPI has that will instead add the Item to the Hebios Mould Press
            // similarly, that Mould Press will automatically allow any Items with the Weapon Part Hebios tag, meaning you don't need to manually add it if it has that tag
            // do note that other mods can add items to these lists as well
            CustomRecipesAPI.Core.itemsToAddToStandardMouldPress.Add(handleMediumCool);

            // adds the Item('s Prefab's Hash) to a dictionary that CustomRecipesAPI has
            // this dictionary is used to automatically offset the position that an item spawns at when creating one with the Smelter
            // the purpose of this is to prevent certain items from getting stuck in the Smelter
            // the Smelter assumes that items will either be small (like an Ingot) or have a pivot in a way where they'll face away from the Smelter (like a blade)
            // this assumption (which is always true for all vanilla Moulds), leads to some items (like handles, which is what this example is using) getting stuck, as they don't follow that set of assumptions
            // like itemsToAddToStandardMouldPress, your mod and other mods adding stuff to this dictionary can overlap
            // to prevent issues with overlapping, always use "dictionary[key] = value" to set pairs in this
            // also, the smelter's rotation isn't aligned with the X and Z axis, so this isn't directly applying to world space
            // instead, it does some math and rotates this position offset to be local to the spawn's rotation, which allows you to act like this is world space anyways
            // the point is, (0,0,1) is 1 unit backwards from the Smelter's point of view, but not actually 1 unit backwards in world space
            // subtract from the X axis to make the item go right, add to make it go left
            // subtract from the Z axis to make the item go forward, add to make it go backward
            // Y axis is normal
            CustomRecipesAPI.Core.smelterSpawnPositionOffsets[handleMediumCool.Prefab.Hash] = new Vector3(0f, -0.2f, -0.7f);
            // same thing, but with the rotation
            // another thing done to prevent overlapping is always setting both values if you set either value, to prevent one mod's position for an item from combining with another mod's rotation for an item
            // as you can see, we set this to 0f,0f,0f, to have it not modify the rotation at all
            CustomRecipesAPI.Core.smelterSpawnRotationOffsets[handleMediumCool.Prefab.Hash] = new Vector3(0f, 0f, 0f);
            // you could also just remove the key
            // this TECHNICALLY improves performance, but its literally one += that doesn't run, it doesn't matter
            // CustomRecipesAPI.Core.smelterSpawnRotationOffsets.Remove(handleMediumCool.Prefab.Hash);
        }
    }
}