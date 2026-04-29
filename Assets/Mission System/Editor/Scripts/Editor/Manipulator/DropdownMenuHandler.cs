using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Tomoe.MissionSystem.Editor
{
    /// <summary>
    /// 用于菜单内容的构建类，封装的菜单内容要如何显示的逻辑
    /// </summary>
    public class DropdownMenuHandler
    {
        private Action<DropdownMenu> menuBuilder;

        public DropdownMenuHandler(Action<DropdownMenu> menuBuilder)
        {
            this.menuBuilder = menuBuilder;
        }

        #region 根据目标ui元素的实际位置打开菜单

        /// <summary>
        /// 提供给外部的显示菜单的逻辑之一
        /// 此函数逻辑为通过目标ui元素的实际位置显示菜单
        /// </summary>
        /// <param name="target">目标ui元素</param>
        public void ShowMenu(VisualElement target)
        {
            // 通过外部提供的菜单构建Action，将新菜单对象传入，调用Action为其初始化
            DropdownMenu dropdownMenu = new DropdownMenu();
            menuBuilder?.Invoke(dropdownMenu);

            // 初始化后的菜单如果没有任何菜单项，则不需要显示菜单，因为空白菜单那没有意义
            if (!dropdownMenu.MenuItems().Any()) return;

            // 判断目标ui元素是否存在，不存在那根据目标ui元素的实际位置打开菜单也无从谈起
            if (target == null) return;

            // 函数所需要的两个参数：1.菜单对象 2.目标ui元素 都已经齐全，可以开始显示菜单
            DisplayMenu(dropdownMenu, target.worldBound);
        }

        /// <summary>
        /// 由于DropdownMenu是uitoolkit的抽象菜单模型，是无法直接显示在unity上的，所以需要转换成unity支持的菜单模型
        /// GenericMenu是unity的原生菜单模型，支持接收文本，图片，点击回调事件等
        /// </summary>
        /// <param name="dropdownMenu">需要显示的菜单模型</param>
        /// <param name="rect">目标ui元素的Rect</param>
        private void DisplayMenu(DropdownMenu dropdownMenu, Rect rect)
        {
            GenericMenu menu = PrepareMenu(dropdownMenu);
            // DropDown：指定的屏幕坐标矩形区域（Rect）上方或附近显示一个菜单。用于当你有一个UI控件（比如按钮、下拉框等），想在该控件的位置弹出菜单时使用
            // Unity会根据这个Rect的位置，以及屏幕边界，自动计算菜单位置（通常在 Rect 下方显示）
            menu.DropDown(rect);
        }

        #endregion

        #region 根据鼠标位置打开菜单

        /// <summary>
        /// 提供给外部的显示菜单的逻辑之一
        /// 此函数逻辑为通过鼠标的位置显示菜单，同时做了兜底处理
        /// 如果不是鼠标事件但调用了此函数，则获取ui元素的位置来显示菜单
        /// </summary>
        /// <param name="e">所有事件的基类</param>
        public void ShowMenu(EventBase e)
        {
            // 通过外部提供的菜单构建Action，将新菜单对象传入，调用Action为其初始化
            DropdownMenu dropdownMenu = new DropdownMenu();
            menuBuilder?.Invoke(dropdownMenu);

            // 初始化后的菜单如果没有任何菜单项，则不需要显示菜单，因为空白菜单那没有意义
            if (!dropdownMenu.MenuItems().Any()) return;

            // 函数所需要的两个参数：1.菜单对象 2.事件元素 都已经齐全，可以开始显示菜单
            DisplayMenu(dropdownMenu, e);
        }

        /// <summary>
        /// 由于DropdownMenu是uitoolkit的抽象菜单模型，是无法直接显示在unity上的，所以需要转换成unity支持的菜单模型
        /// GenericMenu是unity的原生菜单模型，支持接收文本，图片，点击回调事件等
        /// </summary>
        /// <param name="dropdownMenu">需要显示的菜单模型</param>
        /// <param name="triggerEvent">触发的事件基类</param>
        private void DisplayMenu(DropdownMenu dropdownMenu, EventBase triggerEvent)
        {
            GenericMenu menu = PrepareMenu(dropdownMenu);

            Vector2 position = Vector2.zero;
            if (triggerEvent is IMouseEvent mouseEvent) position = mouseEvent.mousePosition; // 鼠标事件
            else if (triggerEvent is IPointerEvent pointerEvent) position = pointerEvent.position; // 更通用的指针事件
            else if (triggerEvent.target is VisualElement target)
                position = target.worldBound.center; // 以上都不是，但有 eventBase.target，且它是一个 VisualElement

            menu.DropDown(new Rect(position, Vector2.zero));
        }

        #endregion

        /// <summary>
        /// 通过传入的dropdownMenu数据，遍历其中的每一项，根据每一项的状态，构建GenericMenu
        /// </summary>
        /// <param name="dropdownMenu">需要显示的菜单模型</param>
        /// <param name="triggerEvent">触发的事件</param>
        private GenericMenu PrepareMenu(DropdownMenu dropdownMenu, EventBase triggerEvent = null)
        {
            GenericMenu menu = new GenericMenu();

            // 让菜单根据当前的事件（比如鼠标事件）更新内部状态，
            // 虽然我们最终不直接显示这个 DropdownMenu，但某些菜单项的状态（比如是否选中、禁用）可能是动态的，依赖于当前上下文
            // 调用 PrepareForDisplay(triggerEvent)可以确保这些状态在转换成 GenericMenu之前是最新的、正确的
            dropdownMenu.PrepareForDisplay(triggerEvent);

            // 开始遍历每一个菜单项
            foreach (DropdownMenuItem option in dropdownMenu.MenuItems())
            {
                // 菜单项的类型主要有两种：1.DropdownMenuAction 2.DropdownMenuSeparator
                // DropdownMenuAction→ 普通可点击的菜单项
                // DropdownMenuSeparator→ 分隔线
                if (option is DropdownMenuAction action)
                {
                    // 菜单项的状态有五种
                    // 1.None：无状态/默认状态，某些实现中，None可能会被视为和Normal等价，但严格来说它是“未设置状态”的默认值
                    // 2.Normal：普通/可点击状态，菜单项处于正常、可交互、未被选中的状态，
                    // 3.Checked：菜单项当前是“选中”状态，通常用于表示开关类菜单项或单选/多选状态，会在菜单项前面显示一个勾选框（✓）
                    // 4.Disabled：禁用状态，单项不可点击、不可交互，通常用于表示当前条件下该选项不可用，菜单项通常会变灰显示，用户无法点击它，点击无反应
                    // 5.Hidden：菜单项不会显示出来，完全隐藏，用户看不到也点不到

                    // 枚举位运算
                    // None = 0 二进制表示为 0000 即2^0
                    // Hidden = 8 二进制表示为 1000 即2^3
                    // 操作符 << 为左移；>> 右移
                    // 操作符右边代表移动的位数
                    // 例如：
                    // 设置第0位：value = 1 << 0 即 value = 1
                    // 设置第 1 位：value = 1 << 1 即 value = 2
                    // 操作符 & 为“按位与”运算，即二进制中，如果相同位值都为1，则结果中的相同位也为1
                    // 操作符 | 为“按位或”运算，即二进制中，如果相同位值只要有一个为0，则结果中的相同位也为0

                    // 如果菜单项中的组合状态包含Hidden或者未设置状态，则不予理会，不加入到菜单内容中
                    if ((action.status & DropdownMenuAction.Status.Hidden) == DropdownMenuAction.Status.Hidden ||
                        action.status == DropdownMenuAction.Status.None) continue;

                    // 检查菜单项中是否包含了选中状态，包含了就代表已经选中，用bool记录下来
                    bool selected = (action.status & DropdownMenuAction.Status.Checked) ==
                                    DropdownMenuAction.Status.Checked;

                    // 如果菜单项包含了“禁用”状态，则将其加入到菜单的【无效项】中，此时菜单项会在菜单面板中显示
                    if ((action.status & DropdownMenuAction.Status.Disabled) == DropdownMenuAction.Status.Disabled)
                    {
                        // bool参数是代表即使为失效项，只要能显示出来就可以指定当前选项是否已经【选中】
                        menu.AddDisabledItem(new GUIContent(action.name), selected);
                        continue;
                    }

                    // 经过以上筛选，只剩正常可互动菜单项，相对于失效项，该项还需要绑定点击回调的执行逻辑
                    menu.AddItem(new GUIContent(action.name), selected, () => action.Execute());
                }
                // 将分割线添加到菜单内容中
                else if (option is DropdownMenuSeparator separator) menu.AddSeparator(separator.subMenuPath);
            }

            return menu;
        }
    }
}