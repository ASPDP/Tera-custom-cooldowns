﻿using System.ComponentModel;
using System.Threading;
using System.Windows;
using TCC.Data;
using TCC.ViewModels;

namespace TCC.Controls
{
    /// <summary>
    /// Logica di interazione per EdgeControl.xaml
    /// </summary>
    public partial class EdgeControl
    {
        public EdgeControl()
        {
            InitializeComponent();
            //baseBorder.Background = (SolidColorBrush)App.Current.FindResource("DefaultBackgroundBrush");

        }
        private int _currentEdge;

        private void SetEdge(int newEdge)
        {
            var diff = newEdge - _currentEdge;

            if (diff == 0) return;
            if (diff > 0)
            {
                if (newEdge == 10)
                {
                    foreach (FrameworkElement child in EdgeContainer.Children) { child.Opacity = 1; }
                }
                for (var i = 0; i < diff; i++)
                {
                    if (_currentEdge + i < EdgeContainer.Children.Count - 1) EdgeContainer.Children[_currentEdge + i].Opacity = 1;
                }
            }
            else
            {
                //baseBorder.Background = (SolidColorBrush)App.Current.FindResource("DefaultBackgroundBrush");
                MaxBorder.Opacity = 0;

                for (var i = EdgeContainer.Children.Count - 1; i >= 0; i--)
                {
                    EdgeContainer.Children[i].Opacity = 0;
                }
            }
            _currentEdge = newEdge;
        }

        private Counter _context;

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (DesignerProperties.GetIsInDesignMode(this)) return;
            //lazy way of making sure that DataContext is not null
            var classMgr = (ClassWindowViewModel.Instance.CurrentManager as WarriorBarManager);
            _context = classMgr?.EdgeCounter;
            while (_context == null)
            {
                _context = (Counter)DataContext;
                Thread.Sleep(500);
            }
            _context.PropertyChanged += _context_PropertyChanged;
            _context.Maxed += OnMaxed;
        }

        private void OnMaxed()
        {
            MaxBorder.Opacity = 1;
        }

        private void _context_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Counter.Val)) SetEdge(_context.Val);
        }

    }
}
