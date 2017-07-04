﻿using System;
using System.Windows.Threading;

namespace TCC.ViewModels
{
    public class TooltipInfo : TSPropertyChanged
    {
        private string name;
        public string Name
        {
            get => name; set
            {
                if (name == value) return;
                name = value;
                NotifyPropertyChanged(nameof(Name));
                NotifyPropertyChanged(nameof(BlockLabelText));
                NotifyPropertyChanged(nameof(ShowAddFriend));
                NotifyPropertyChanged(nameof(ShowWhisper));
            }
        }
        private string info;
        public string Info
        {
            get => info; set
            {
                if (info == value) return; info = value;
                NotifyPropertyChanged(nameof(Info));
            }
        }
        private int level;
        public int Level
        {
            get => level; set
            {
                if (level == value) return; level = value; NotifyPropertyChanged(nameof(Level));
            }
        }
        private Class charClass;
        public Class Class
        {
            get => charClass;
            set
            {
                if (charClass == value) return;
                charClass = value;
                NotifyPropertyChanged(nameof(Class));
            }
        }
        private bool showPartyInvite;
        public bool ShowPartyInvite
        {
            get { return showPartyInvite; }
            set
            {
                if (showPartyInvite == value) return;
                showPartyInvite = value;
                NotifyPropertyChanged(nameof(ShowPartyInvite));
            }
        }
        private bool showGuildInvite;
        public bool ShowGuildInvite
        {
            get { return showGuildInvite; }
            set
            {
                if (showGuildInvite == value) return;
                showGuildInvite = value;
                NotifyPropertyChanged(nameof(ShowGuildInvite));
            }
        }

        public bool ShowAddFriend
        {
            get { return !IsBlocked; }
        }
        public bool ShowWhisper
        {
            get { return !IsBlocked; }
        }
        public string BlockLabelText
        {
            get
            {
                if (IsBlocked) return "Unblock";
                else return "Block";
            }
        }
        public string FriendLabelText
        {
            get
            {
                if (IsFriend) return "Remove friend";
                else return "Add friend";
            }
        }

        public bool IsBlocked
        {
            get => ChatWindowViewModel.Instance.BlockedUsers.Contains(name);
        }
        public bool IsFriend
        {
            get => ChatWindowViewModel.Instance.Friends.Contains(name);
        }

        public TooltipInfo(string n, string i, int l)
        {
            _dispatcher = Dispatcher.CurrentDispatcher;
            Name = n;
            Info = i;
            Level = l;
        }

        public void Refresh()
        {
            NotifyPropertyChanged(nameof(ShowPartyInvite));
            NotifyPropertyChanged(nameof(ShowGuildInvite));
            NotifyPropertyChanged(nameof(ShowAddFriend));
            NotifyPropertyChanged(nameof(ShowWhisper));
            NotifyPropertyChanged(nameof(BlockLabelText));
            NotifyPropertyChanged(nameof(FriendLabelText));
            NotifyPropertyChanged(nameof(IsBlocked));
            NotifyPropertyChanged(nameof(IsFriend));
        }

        public void SetInfo(uint model)
        {
            var c = (model % 100) - 1;
            Class = (Class)c;
        }
    }
}