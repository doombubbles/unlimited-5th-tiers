using BTD_Mod_Helper.Api.Data;
using BTD_Mod_Helper.Api.Enums;
using BTD_Mod_Helper.Api.ModOptions;

namespace Unlimited5thTiers;

public class Settings : ModSettings
{
    public static readonly ModSettingBool AllowUnlimited5thTiers = new(true)
    {
        displayName = "Allow Unlimited 5th Tiers",
        icon = GetTextureGUID<Unlimited5thTiersMod>("Icon")
    };

    public static readonly ModSettingBool AllowUnlimitedParagons = new(true)
    {
        displayName = "Allow Unlimited Paragons",
        icon = VanillaSprites.ParagonIcon
    };

    public static readonly ModSettingBool AllowUnlimitedVTSGs = new(true)
    {
        displayName = "Allow Unlimited VTSGs",
        icon = VanillaSprites.SuperMonkey555
    };
        
    public static readonly ModSettingBool AllowNonMaxedVTSGs = new(true)
    {
        displayName = "Allow Non Maxed VTSGs",
        icon = VanillaSprites.SuperMonkey555
    };

    public static readonly ModSettingBool AllowUnlimitedHeroes = new(false)
    {
        displayName = "Allow Unlimited Heroes",
        icon = VanillaSprites.HeroesIcon
    };

    public static readonly ModSettingBool ShowAllHeroesInGame = new(false)
    {
        displayName = "Show All Heroes in Game",
        icon = VanillaSprites.HeroesIcon
    };

}