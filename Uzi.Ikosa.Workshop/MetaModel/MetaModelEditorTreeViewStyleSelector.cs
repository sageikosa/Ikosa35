using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using Uzi.Visualize;
using System.Windows;

namespace Uzi.Ikosa.Workshop
{
    public class MetaModelEditorTreeViewStyleSelector : StyleSelector
    {
        public MetaModelEditorTreeViewStyleSelector(MetaModelEditor editor)
        {
            _Editor = editor;
        }

        private MetaModelEditor _Editor;

        public override System.Windows.Style SelectStyle(object item, System.Windows.DependencyObject container)
        {
            //var itemsControl = ItemsControl.ItemsControlFromItemContainer(container);
            //TreeViewItem _item = container as TreeViewItem;
            if (item is MetaModelRootNode)
            {
                return _Editor.Resources[@"styleRootRefTreeViewItem"] as Style;
            }
            else if (item is MetaModelFragmentNode)
            {
                return _Editor.Resources[@"styleFragmentRefTreeViewItem"] as Style;
            }
            else if (item is BrushCrossRefNode)
            {
                return _Editor.Resources[@"styleBrushRefTreeViewItem"] as Style;
            }
            return _Editor.Resources[@"styleNullMenuTreeViewItem"] as Style;
        }
    }
}
