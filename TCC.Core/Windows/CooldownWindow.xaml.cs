﻿namespace TCC.Windows
{
    public partial class CooldownWindow 
    {
        public CooldownWindow()
        {
            InitializeComponent();
            ButtonsRef = Buttons;
            MainContent = WindowContent;
            Init(Settings.CooldownWindowSettings, ignoreSize: true, undimOnFlyingGuardian: false);

        }
    }
}

