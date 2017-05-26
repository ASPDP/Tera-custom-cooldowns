﻿using System.Windows.Threading;

namespace TCC.ViewModels
{
    public class StatTracker : TSPropertyChanged
    {
        private int val = 0;
        public int Val
        {
            get { return val; }
            set
            {
                if (val == value) return;
                val = value;

                NotifyPropertyChanged("Val");
                NotifyPropertyChanged("Factor");
            }
        }

        private int max = 1;
        public int Max
        {
            get => max;
            set
            {
                if (max == value) return;
                max = value;
                if (max == 0) max = 1;
                NotifyPropertyChanged("Max");
                NotifyPropertyChanged("Factor");
            }
        }

        public double Factor
        {
            get
            {
                return (double)val / max;
            }
        }
        public StatTracker()
        {
            _dispatcher = Dispatcher.CurrentDispatcher;
        }
    }
}