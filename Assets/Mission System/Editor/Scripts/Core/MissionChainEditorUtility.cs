using System;
using System.Reflection;
using UnityEngine.UIElements;

namespace Tomoe.MissionSystem.Editor
{
    public static class MissionChainEditorUtility
    {
        #region 右键菜单辅助函数

        public static void RemoveMenuItem(UnityEngine.UIElements.DropdownMenu menu, string itemName)
        {
            var items = menu.MenuItems();
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i] is DropdownMenuAction action && action.name == itemName)
                {
                    menu.RemoveItemAt(i);
                    break;
                }
            }
        }
        
        public static void RemoveSeparator(UnityEngine.UIElements.DropdownMenu menu)
        {
            var items = menu.MenuItems();
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i] is DropdownMenuSeparator)
                {
                    menu.RemoveItemAt(i);
                    break;
                }
            }
        }
        
        /// <summary>
        /// 修改指定菜单项的回调，添加额外逻辑
        /// </summary>
        /// <param name="menu">下拉菜单</param>
        /// <param name="actionName">菜单项名称</param>
        /// <param name="extraLogic">要添加的额外逻辑</param>
        /// <returns>是否成功修改</returns>
        public static bool ModifyActionCallback(this DropdownMenu menu, 
            string actionName, Action<DropdownMenuAction> extraLogic)
        {
            if (menu == null || string.IsNullOrEmpty(actionName) || extraLogic == null)
                return false;

            // 获取菜单项列表
            var items = GetMenuItems(menu);
            if (items == null) return false;

            // 查找目标菜单项
            var targetAction = FindMenuItem(items, actionName);
            if (targetAction == null) return false;

            // 修改回调
            return ModifyCallback(targetAction, extraLogic);
        }
        
        /// <summary>
        /// 获取菜单项列表
        /// </summary>
        private static System.Collections.IList GetMenuItems(DropdownMenu menu)
        {
            var property = typeof(DropdownMenu).GetProperty("MenuItems",
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            return property?.GetValue(menu) as System.Collections.IList;
        }

        /// <summary>
        /// 查找指定名称的菜单项
        /// </summary>
        private static DropdownMenuAction FindMenuItem(System.Collections.IList items, string actionName)
        {
            foreach (var item in items)
            {
                if (item is DropdownMenuAction action && action.name == actionName)
                {
                    return action;
                }
            }
            return null;
        }

        /// <summary>
        /// 修改菜单项的回调
        /// </summary>
        private static bool ModifyCallback(DropdownMenuAction action, Action<DropdownMenuAction> extraLogic)
        {
            // 获取actionCallback字段（Unity内部字段名）
            var callbackField = typeof(DropdownMenuAction).GetField("actionCallback",
                BindingFlags.Instance | BindingFlags.NonPublic);

            if (callbackField == null) return false;

            // 获取原始回调
            var originalCallback = callbackField.GetValue(action) as Action<DropdownMenuAction>;
            if (originalCallback == null) return false;

            // 创建新的组合回调：先执行原始逻辑，再执行额外逻辑
            Action<DropdownMenuAction> newCallback = (a) =>
            {
                originalCallback(a);
                extraLogic(a);
            };

            // 设置新的回调
            callbackField.SetValue(action, newCallback);
            return true;
        }
        
        #endregion
    }
}