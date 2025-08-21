using System;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Uzi.Visualize
{
    public class ToolTipHelper
    {
        /// <summary>ctor</summary>
        public ToolTipHelper(ResourceDictionary resources)
        {
            _ToolTip = new ToolTip
            {
                Resources = resources,
                Content = new ContentControl()
            };
            _Timer = new Timer { AutoReset = false };
            _Timer.Elapsed += ShowToolTip;
        }

        #region state
        private readonly ToolTip _ToolTip;
        private readonly Timer _Timer;
        #endregion

        /// <summary>Gets or sets the content for the tooltip.</summary>
        public object ToolTipContent
        {
            get => (_ToolTip.Content as ContentControl).Content;
            set => (_ToolTip.Content as ContentControl).Content = value;
        }

        /// <summary>Mouse enters element.</summary>
        public void OnMouseEnter(object sender, MouseEventArgs e)
        {
            _Timer.Interval = 100;
            _Timer.Start();
        }

        private void ShowToolTip(object sender, ElapsedEventArgs e)
        {
            _Timer.Stop();
            _ToolTip?.Dispatcher.Invoke(new Action(() => { _ToolTip.IsOpen = true; }));
        }

        /// <summary>Mouse leaves element.</summary>
        public void OnMouseLeave(object sender, MouseEventArgs e)
        {
            _Timer.Stop();
            if (_ToolTip != null)
            {
                _ToolTip.IsOpen = false;
            }
        }
    }
}
