using HarmonyLib;
using KMod;
using System.IO;
using System.Reflection;
using UnityEngine;
using AgriHarvestPriority.Patches;
using static Localization;

namespace AgriHarvestPriority
{
    public class Mod : UserMod2
    {
        public static Sprite UprootIconSprite { get; private set; }
        public static Sprite NotUprootIconSprite { get; private set; }

        // ============================================================
        // 替代 [PLibMethod(RunAt.BeforeDbInit)]
        // PLib 内部即 patch Db.Initialize 的 Prefix
        // ============================================================
        [HarmonyPatch(typeof(Db), "Initialize")]
        public static class Db_Initialize_Patch
        {
            public static void Prefix()
            {
                BeforeDbInit();
            }
        }

        // ============================================================
        // 替代 [PLibMethod(RunAt.OnStartGame)]
        // PLib 内部即 patch Game.OnPrefabInit 的 Postfix
        // ============================================================
        [HarmonyPatch(typeof(Game), "OnPrefabInit")]
        public static class Game_OnPrefabInit_Patch
        {
            public static void Postfix()
            {
                // 每次进入游戏世界（新游戏或读档）时清空 HarvestToolPatch 缓存
                HarvestToolPatch.InvalidateCache();
            }
        }

        // ============================================================
        // 替代 new PLocalization().Register()
        // 加载 translations/*.po 并注册 LocString keys
        // 完全照抄 MoveThisHere 的自包含本地化方案
        // ============================================================
        [HarmonyPatch(typeof(Localization), "Initialize")]
        public static class Localization_Initialize_Patch
        {
            public static void Postfix()
            {
                // 注册 STRINGS 中的 LocString 字段
                RegisterForTranslation(typeof(STRINGS));

                // 从 mod 文件夹加载 .po 翻译
                LoadStrings();

                // ★ 修复：第二参数传 null（与 MoveThisHere 一致）。
                //   CreateLocStringKeys 内部会用 type.FullName 作为默认前缀，
                //   对 typeof(AgriHarvestPriority.STRINGS) 而言即
                //     "AgriHarvestPriority.STRINGS"
                //   再拼上嵌套类型路径与字段名，最终生成的 key 形如：
                //     AgriHarvestPriority.STRINGS.UI.TOOLS.FILTERLAYERS.UPROOT.NAME
                //
                //   ⚠️ 因此 .po 文件中的 msgctxt 也必须改成带
                //   "AgriHarvestPriority." 前缀的版本，两边才能匹配。
                LocString.CreateLocStringKeys(typeof(STRINGS), null);
            }

            private static void LoadStrings()
            {
                string localeCode = GetLocale()?.Code;
                if (string.IsNullOrEmpty(localeCode))
                    return;

                // mod 文件夹 = 当前 DLL 所在目录
                string modPath = Path.GetDirectoryName(
                    Assembly.GetExecutingAssembly().Location);
                string path = Path.Combine(modPath, "translations", localeCode + ".po");

                if (File.Exists(path))
                    OverloadStrings(LoadStringsFile(path, false));
            }
        }

        // ============================================================
        // 原 [PLibMethod(RunAt.BeforeDbInit)] 方法体（保留原名与顺序）
        // 由 Db_Initialize_Patch.Prefix 调用
        // ============================================================
        internal static void BeforeDbInit()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourcePrefix = $"{assembly.GetName().Name}.ModAssets.assets.";

            UprootIconSprite = Utilities.CreateSpriteDxt5(
                assembly.GetManifestResourceStream(resourcePrefix + "uproot_icon.dds"),
                128, 128);
            UprootIconSprite.name = "uproot_icon";

            if (Assets.Sprites.ContainsKey(UprootIconSprite.name))
                Assets.Sprites.Remove(UprootIconSprite.name);
            Assets.Sprites.Add(UprootIconSprite.name, UprootIconSprite);
			
			// ★ 新增：加载"未拔除"图标
			NotUprootIconSprite = Utilities.CreateSpriteDxt5(
				assembly.GetManifestResourceStream(resourcePrefix + "not_uproot_icon.dds"),
				128, 128
			);
			NotUprootIconSprite.name = "not_uproot_icon";
			if (Assets.Sprites.ContainsKey(NotUprootIconSprite.name))
				Assets.Sprites.Remove(NotUprootIconSprite.name);
			Assets.Sprites.Add(NotUprootIconSprite.name, NotUprootIconSprite);
        }

        public override void OnLoad(Harmony harmony)
        {
            base.OnLoad(harmony);
            // 已移除：PUtil.InitLibrary()（仅打印日志）
            // 已移除：new PPatchManager(harmony).RegisterPatchClass(typeof(Mod))
            //         改用原生 [HarmonyPatch] 特性，UserMod2.OnLoad 会自动 PatchAll
            // 已移除：new PLocalization().Register()
            //         改用 Localization_Initialize_Patch
        }
    }

    public static class Utilities
    {
        public static Sprite CreateSpriteDxt5(System.IO.Stream inputStream, int width, int height)
        {
            if (inputStream == null) return null;

            byte[] buffer = new byte[inputStream.Length - 128];
            inputStream.Seek(128, System.IO.SeekOrigin.Current);
            inputStream.Read(buffer, 0, buffer.Length);

            Texture2D texture = new Texture2D(width, height, TextureFormat.DXT5, false);
            texture.LoadRawTextureData(buffer);
            texture.Apply(false, true);
            return Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f));
        }
    }
}