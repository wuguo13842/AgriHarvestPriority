namespace AgriHarvestPriority
{
    // ★ 从 namespace STRINGS 改为独立类，避免和游戏自带的 STRINGS 类冲突。
    //   LocString key 前缀由 Mod.Localization_Initialize_Patch 中的
    //   LocString.CreateLocStringKeys(typeof(STRINGS), null) 决定。
    //   传 null 时前缀 = type.FullName，即 "AgriHarvestPriority.STRINGS"，
    //   最终生成的 key 形如：
    //     AgriHarvestPriority.STRINGS.UI.TOOLS.FILTERLAYERS.UPROOT.NAME
    //   因此 translations/*.po 中的 msgctxt 必须带 "AgriHarvestPriority." 前缀，
    //   两边才能匹配。
    //
    //   注：农业（AGRICULTURE）使用游戏原生 STRINGS.UI.TOOLS.FILTERLAYERS.AGRICULTURE
    //   的翻译，无需在本类中重新定义。
    public static class STRINGS
    {
        public static class UI
        {
            public static class TOOLS
            {
                public static class FILTERLAYERS
                {
                    public static class UPROOT
                    {
                        public static LocString NAME = "Uproot";
                        public static LocString TOOLTIP = "Mark selected plants for uprooting";
                    }

                    // ---------- 新增：取消拔除 ----------
                    public static class CANCEL_UPROOT
                    {
                        public static LocString NAME = "Cancel Uproot";
                        public static LocString TOOLTIP = "Cancel uproot marking on selected plants";
                    }
				
					// // ---------- 新增：农业 ----------
                    // ⚠️ 注意：PriorityToolPatch 用的是字符串常量 "AGRICULTURE"，
                    //    ToolParameterMenu 会去查游戏原生 key
                    //    STRINGS.UI.TOOLS.FILTERLAYERS.AGRICULTURE.*，
                    //    所以下面这份定义实际不会生效（游戏本体已有该翻译）。
                    //    保留仅为将来可能的扩展，需要真正生效时得改为
                    //    游戏原生 key 名才能覆盖。
					// public static class AGRICULTURE
					// {
						// public static LocString NAME = "Agriculture";
						// public static LocString TOOLTIP = "Show only agriculture buildings";
					// }
					
					public static class TRAVELTUBE
					{
						public static LocString NAME = "Transit Tube";
						public static LocString TOOLTIP = "Mark transit tubes";
					}
				}
            }
        }
    }
}