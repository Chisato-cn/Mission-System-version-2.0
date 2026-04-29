using System;
using UnityEngine.UIElements;

namespace Tomoe.MissionSystem.Editor
{
    /// <summary>
    /// 赋予ui元素【打开菜单】的交互控制器
    /// </summary>
    public class DropdownMenuManipulator : Clickable
    {
        private DropdownMenuHandler menuHandler;
        private bool showWhitMouse;

        /// <param name="menuBuilder">菜单构建逻辑事件</param>
        /// <param name="mouseButton">指定Manipulator响应对的button</param>
        /// <param name="showWhitMouse">是否跟随鼠标位置显示</param>
        /// <param name="handler">？？？</param>
        public DropdownMenuManipulator(Action<DropdownMenu> menuBuilder, MouseButton mouseButton, bool showWhitMouse,
            Action<EventBase> handler = null) : base(handler)
        {
            menuHandler = new DropdownMenuHandler(menuBuilder);
            this.showWhitMouse = showWhitMouse;

            // 清除筛选器，重新为筛选器添加mouseButton的筛选条件
            activators.Clear();
            activators.Add(new ManipulatorActivationFilter { button = mouseButton });

            // 由于Handler中需要判断鼠标点击时的事件类型，所以使用带参数的鼠标点击事件
            clickedWithEventInfo += e =>
            {
                if (this.showWhitMouse) menuHandler.ShowMenu(e);
                else menuHandler.ShowMenu(target);
            };
        }

        public DropdownMenuManipulator(Action<DropdownMenu> menuBuilder, MouseButton mouseButton,
            Action<EventBase> handler = null) : this(menuBuilder, mouseButton, false, handler)
        {

        }
    }
}