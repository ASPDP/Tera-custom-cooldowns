﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media.Animation;

namespace TCC.Windows
{
    public class TccWindow : Window
    {
        protected IntPtr _handle;
        protected bool clickThru;

        public bool ClickThru
        {
            get { return clickThru; }
            set
            {
                if (clickThru == value) return;
                clickThru = value;

                if (clickThru) FocusManager.MakeTransparent(_handle);
                else FocusManager.UndoTransparent(_handle);

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ClickThru"));
            }
        }

        WindowSettings _settings;

        protected void InitWindow(WindowSettings ws)
        {
            _handle = new WindowInteropHelper(this).Handle;
            FocusManager.MakeUnfocusable(_handle);
            FocusManager.HideFromToolBar(_handle);
            Topmost = true;
            ContextMenu = new ContextMenu();

            var HideButton = new MenuItem() { Header = "Hide" };
            HideButton.Click += (s, ev) =>
            {
                SetVisibility(Visibility.Hidden);
            };
            ContextMenu.Items.Add(HideButton);

            var ClickThruButton = new MenuItem() { Header = "Click through" };
            ClickThruButton.Click += (s, ev) =>
            {
                SetClickThru(true);
            };
            ContextMenu.Items.Add(ClickThruButton);

            _settings = ws;

            Left = ws.X;
            Top = ws.Y;
            Visibility = ws.Visibility;
            SetClickThru(ws.ClickThru);
            oldClickThru = ws.ClickThru;
            ((FrameworkElement)this.Content).Opacity = 0;

            WindowManager.TccVisibilityChanged += OpacityChange;
            WindowManager.TccDimChanged += OpacityChange;
        }

        private void OpacityChange(object sender, PropertyChangedEventArgs e)
        {
            if(e.PropertyName == "IsTccVisible")
            {
                if (WindowManager.IsTccVisible)
                {
                    if (WindowManager.IsTccDim)
                    {
                        AnimateContentOpacity(SettingsManager.DimOpacity);
                    }
                    else
                    {
                        AnimateContentOpacity(1);
                    }
                }
                else
                {
                    AnimateContentOpacity(0);
                }
            }

            if(e.PropertyName == "IsTccDim")
            {
                if (!WindowManager.IsTccVisible) return;

                if (WindowManager.IsTccDim)
                {
                    AnimateContentOpacity(SettingsManager.DimOpacity);
                    if (!SettingsManager.ClickThruWhenDim) return;
                    FocusManager.MakeTransparent(_handle);
                }
                else
                {
                    AnimateContentOpacity(1);
                    if (!SettingsManager.ClickThruWhenDim) return;
                    if(!clickThru) FocusManager.UndoTransparent(_handle);

                }
            }
        }
        protected bool oldClickThru;
        public void SetClickThru(bool t)
        {
            ClickThru = t;
        }

        public void SetVisibility(Visibility v)
        {
            this.Dispatcher.Invoke(() =>
            {
                Visibility = v;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Visibility"));
            });
        }
        public event PropertyChangedEventHandler PropertyChanged;
        public void AnimateContentOpacity(double opacity)
        {
            Dispatcher.InvokeIfRequired(() =>
            {
                ((FrameworkElement)this.Content).BeginAnimation(OpacityProperty, new DoubleAnimation(opacity, TimeSpan.FromMilliseconds(250)));
            }, System.Windows.Threading.DispatcherPriority.DataBind);
        }
        public void RefreshTopmost()
        {
            Dispatcher.InvokeIfRequired(() => { Topmost = false; Topmost = true; }, System.Windows.Threading.DispatcherPriority.DataBind);
        }
    }
}