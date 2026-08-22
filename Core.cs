using Alta;
using Alta.Inventory;
using MelonLoader;
using MelonLoader.Utils;
using System.Reflection;
using UnityEngine;
using CustomRecipesAPI;

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
            // load our gameobjects that were made in the inspector
            // to make this in the inspector, copy an existing MouldDefinition and modify its values
            // the graphic will need to have its values of position, rotation, and scale set manually
            // to easily do this, keep some placeholders in the asset at first, go ingame with the unfinished asset and tweak the graphic position with UnityExplorer, then rebuild the asset with those new graphic positioning values
            MouldDefinition mouldDefinition = assetBundle.LoadAsset<MouldDefinition>("Handle Medium Cool Mould.asset");
            // to make this in the inspector, copy an existing MouldItemComponent and modify its value
            // shouldn't be loaded here if the Item already has a MouldItemComponent (if it's a custom item, you should add the MouldItemComponent in the inspector)
            MouldItemComponent mouldItemComponent = assetBundle.LoadAsset<MouldItemComponent>("Handle Medium Cool MIC.asset");

            // instead of having you manually do a bunch of stuff in a specific way, you literally just need to call this one method
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
        }
    }
}