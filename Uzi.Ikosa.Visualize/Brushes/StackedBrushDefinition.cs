using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using Uzi.Visualize.Packaging;
using System.Windows.Controls;
using System.Windows;

namespace Uzi.Visualize
{
    public class StackedBrushDefinition : BrushDefinition
    {
        #region construction
        public StackedBrushDefinition()
        {
        }

        public StackedBrushDefinition(StackedBrushDefinition source)
            : base(source)
        {
            foreach (var _c in source.Components)
                _Components.Add(_c.Clone());
        }

        public StackedBrushDefinition(BrushCollectionPart owner)
        {
            Owner = owner;
        }
        #endregion

        #region public BrushCollection Components { get; set; }
        private BrushCollection _Components = new BrushCollection();
        public BrushCollection Components
        {
            get { return _Components; }
            set
            {
                _Components = value;
                ClearCache();
            }
        }
        #endregion

        #region public override void ClearCache()
        public override void ClearCache()
        {
            base.ClearCache();
            _Components.RefreshAll();
            PreGenerateBrushes();
        }
        #endregion

        /// <summary>Only if all components have alpha channel (since this brush pretty much needs Alpha Channels for masking</summary>
        public override bool IsAlphaChannel
        {
            get { return _Components.All(_c => _c.IsAlphaChannel); }
            set { }
        }

        protected override bool NeedsPreGenerate { get { return true; } }

        #region protected override Brush OnGetBrush(VisualEffect effect)
        protected override Brush OnGetBrush(VisualEffect effect)
        {
            // initialize
            var _brush = new DrawingBrush { Stretch = Stretch.Uniform };
            var _group = new DrawingGroup();

            // draw all brushes on overlapping rectangles (assumes alpha will take care of blending)
            // NOTE: first brush is on the top, others are underneath ".Reverse()"
            // NOTE: ... also, had to use ".Select(...), "
            // NOTE: ... since built-in "List.Reverse()" re-orders in place and returns void
            foreach (var _def in _Components.Select(_d => _d).Reverse())
            {
                _group.Children.Add(
                    new GeometryDrawing(_def.GetBrush(effect), null, new RectangleGeometry(new Rect(0, 0, 1, 1))));
            }

            // finalize
            _brush.Drawing = _group;
            if (!IsAlphaChannel)
            {
                // TODO: ¿ alt brush Opacity ?
                _brush.Opacity = Opacity;
            }
            _brush.Freeze();
            return _brush;
        }
        #endregion

        public override BrushDefinition Clone()
            => new StackedBrushDefinition(this);
    }
}
