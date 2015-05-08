﻿#region

using System;
using System.Runtime.Serialization;
using System.Windows.Forms;
using LeagueSharp.CommonEx.Core.Enumerations;
using LeagueSharp.CommonEx.Core.Extensions.SharpDX;
using LeagueSharp.CommonEx.Core.UI.Abstracts;
using LeagueSharp.CommonEx.Core.UI.Skins;
using LeagueSharp.CommonEx.Core.UI.Skins.Default;
using LeagueSharp.CommonEx.Core.Utils;
using SharpDX;

#endregion

namespace LeagueSharp.CommonEx.Core.UI.Values
{
    /// <summary>
    ///     Menu KeyBind.
    /// </summary>
    [Serializable]
    public class MenuKeyBind : AMenuValue, ISerializable
    {
        private bool _interacting;

        /// <summary>
        ///     Menu KeyBind Constructor
        /// </summary>
        /// <param name="key">The Key to bind</param>
        /// <param name="type">Key bind type</param>
        public MenuKeyBind(Keys key, KeyBindType type)
        {
            Key = key;
            Type = type;
        }

        public MenuKeyBind(SerializationInfo info, StreamingContext context)
        {
            Key = (Keys) info.GetValue("key", typeof(Keys));
        }

        /// <summary>
        ///     Menu KeyBind Constructor
        /// </summary>
        public MenuKeyBind() {}

        /// <summary>
        ///     KeyBind Item Width.
        /// </summary>
        public override int Width
        {
            get
            {
                return
                    (int)
                        (DefaultSettings.ContainerHeight + ThemeManager.Current.CalcWidthText("[" + Key + "]") +
                         DefaultSettings.ContainerTextOffset);
            }
        }

        public bool Interacting
        {
            get { return _interacting; }
            set
            {
                _interacting = value;
                MenuInterface.Instance.ForcedOpen = value;
            }
        }

        /// <summary>
        ///     Returns if the key is active.
        /// </summary>
        public bool Active { get; set; }

        /// <summary>
        ///     KeyBind Type.
        /// </summary>
        public KeyBindType Type { get; set; }

        /// <summary>
        ///     KeyBind Key Value.
        /// </summary>
        public Keys Key { get; set; }

        /// <summary>
        ///     KeyBind Item Position.
        /// </summary>
        public override Vector2 Position { get; set; }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("key", Key, typeof(Keys));
        }

        /// <summary>
        ///     KeyBind Item Draw callback.
        /// </summary>
        /// <param name="component">Parent Component</param>
        /// <param name="position">Position</param>
        /// <param name="index">Item Index</param>
        public override void OnDraw(AMenuComponent component, Vector2 position, int index)
        {
            if (!Position.Equals(position))
            {
                Position = position;
            }

            Theme.Animation animation = ThemeManager.Current.Boolean.Animation;

            if (animation != null && animation.IsAnimating())
            {
                animation.OnDraw(component, position, index);

                return;
            }

            ThemeManager.Current.KeyBind.OnDraw(component, position, index);
        }

        /// <summary>
        ///     KeyBind Item Windows Process Messages callback.
        /// </summary>
        /// <param name="args">
        ///     <see cref="WindowsKeys" />
        /// </param>
        public override void OnWndProc(WindowsKeys args)
        {
            if (!MenuGUI.IsChatOpen)
            {
                switch (args.Msg)
                {
                    case WindowsMessages.KEYDOWN:
                        if (args.Key == Key && Type == KeyBindType.Press)
                        {
                            Active = true;
                            FireEvent(Active, false);
                        }
                        break;
                    case WindowsMessages.KEYUP:
                        if (Interacting && args.SingleKey != Keys.ShiftKey)
                        {
                            Key = args.SingleKey == Keys.Escape ? Keys.None : args.Key;
                            Interacting = false;
                            Container.ResetWidth();
                        }
                        else if (args.Key == Key && Type == KeyBindType.Press)
                        {
                            Active = false;
                            FireEvent(Active, true);
                        }
                        else if (args.Key == Key && Type == KeyBindType.Toggle)
                        {
                            Active = !Active;
                            FireEvent(Active, !Active);
                        }
                        break;
                    case WindowsMessages.LBUTTONDOWN:

                        if (Position.IsValid())
                        {
                            Rectangle container = ThemeManager.Current.KeyBind.AdditionalBoundries(Position, Container);
                            Rectangle content = ThemeManager.Current.KeyBind.Bounding(Position, Container);

                            if (args.Cursor.IsUnderRectangle(
                                container.X, container.Y, container.Width, container.Height))
                            {
                                Active = !Active;
                            }
                            else if (args.Cursor.IsUnderRectangle(content.X, content.Y, content.Width, content.Height))
                            {
                                Interacting = !Interacting;
                            }
                        }
                        break;
                }
            }
        }

        public override void Extract(AMenuValue value)
        {
            var keybind = ((MenuKeyBind) value);
            Key = keybind.Key;
        }
    }
}