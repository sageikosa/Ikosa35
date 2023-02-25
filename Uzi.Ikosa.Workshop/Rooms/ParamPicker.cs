using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using Uzi.Ikosa.Tactical;
using Uzi.Visualize;

namespace Uzi.Ikosa.Workshop
{
    public static class ParamPicker
    {
        public static UserControl GetParamControl(IParamCellSpace cellSpace)
        {
            if (cellSpace.Template is SlopeCellSpace)
            {
                return new SlopeParams();
            }
            else if (cellSpace.Template is SliverCellSpace)
            {
                return new SliverParams();
            }
            else if (cellSpace.Template is CylinderSpace)
            {
                return new CylinderParams();
            }
            else if (cellSpace.Template is SmallCylinderSpace)
            {
                return new SmallCylinderParams();
            }
            else if (cellSpace.Template is WedgeCellSpace)
            {
                return new WedgeParams();
            }
            else if (cellSpace.Template is Stairs)
            {
                return new StairsParams();
            }
            else if (cellSpace.Template is LFrame)
            {
                return new LFrameParams();
            }
            else if (cellSpace.Template is PanelCellSpace)
            {
                return new PanelSpaceParams();
            }
            return null;
        }

        public static UserControl GetParamControl(IParamCellSpace cellSpace, uint paramData)
        {
            if (cellSpace.Template is SlopeCellSpace)
            {
                return new SlopeParams(paramData);
            }
            else if (cellSpace.Template is SliverCellSpace)
            {
                return new SliverParams(paramData);
            }
            else if (cellSpace.Template is CylinderSpace)
            {
                return new CylinderParams(paramData);
            }
            else if (cellSpace.Template is SmallCylinderSpace)
            {
                return new SmallCylinderParams(paramData);
            }
            else if (cellSpace.Template is WedgeCellSpace)
            {
                return new WedgeParams(paramData);
            }
            else if (cellSpace.Template is Stairs)
            {
                return new StairsParams(paramData);
            }
            else if (cellSpace.Template is LFrame)
            {
                return new LFrameParams(paramData);
            }
            else if (cellSpace.Template is PanelCellSpace)
            {
                return new PanelSpaceParams(paramData);
            }
            return null;
        }

        public static uint ParamData(ContentControl ccParams)
        {
            if (ccParams.Content is IParamControl)
                return ((IParamControl)ccParams.Content).ParamData;
            return 0;
        }
    }
}
