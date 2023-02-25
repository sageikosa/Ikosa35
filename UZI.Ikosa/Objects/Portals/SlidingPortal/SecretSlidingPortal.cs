using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uzi.Core;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Senses;
using Uzi.Ikosa.Tactical;
using Uzi.Visualize;
using Uzi.Visualize.Contracts;

namespace Uzi.Ikosa.Objects
{
    [Serializable]
    public class SecretSlidingPortal : SlidingPortal, IPanelShade
    {
        public SecretSlidingPortal(string name, AnchorFace anchorFace, Axis slidingAxis,
            double distance, double zOff, double yOff, double xOff,
            PortalledObjectBase portObjA, PortalledObjectBase portObjB)
            : base(name, anchorFace, slidingAxis, distance, zOff, yOff, xOff, portObjA, portObjB)
        {
            AddAdjunct(new Searchable(new Deltable(20), true));
            _Cells = new uint[2] { 0, 0 };
        }

        #region Cell Material Indexes
        private uint[] _Cells;
        #endregion

        public CellSpace InwardCellSpace
        {
            get => (Setting as LocalMap)?.CellSpaces.FirstOrDefault(_cs => _cs.Index == _Cells[0]);
            set => _Cells[0] = value?.Index ?? _Cells[0];
        }

        public CellSpace OutwardCellSpace
        {
            get => (Setting as LocalMap)?.CellSpaces.FirstOrDefault(_cs => _cs.Index == _Cells[1]);
            set => _Cells[1] = value?.Index ?? _Cells[1];
        }

        public IEnumerable<CellSpace> AvailableCellSpaces
            => (Setting as LocalMap)?.CellSpaces.ToList();

        public Searchable Searchable => Adjuncts.OfType<Searchable>().FirstOrDefault();

        public IEnumerable<PanelShadingInfo> GetPanelShadings(ISensorHost sensorHost, bool insideGroup, ICellLocation location)
        {
            // not aware
            if ((sensorHost?.IsSensorHostActive ?? false)
                && ((sensorHost?.Awarenesses?.GetAwarenessLevel(ID) ?? AwarenessLevel.None) < AwarenessLevel.Aware))
            {
                if (insideGroup)
                {
                    if (OpenState.IsClosed)
                    {
                        var _cell = location.Add(AnchorFace);
                        yield return new PanelShadingInfo
                        {
                            Z = _cell.Z,
                            Y = _cell.Y,
                            X = _cell.X,
                            AnchorFace = AnchorFace.ReverseFace(),
                            CellSpace = (new IndexStruct { Index = InwardCellSpace?.Index ?? 0 }).ID
                        };
                    }
                }
                else
                {
                    if (OpenState.IsClosed)
                    {
                        yield return new PanelShadingInfo
                        {
                            Z = location.Z,
                            Y = location.Y,
                            X = location.X,
                            AnchorFace = AnchorFace,
                            CellSpace = (new IndexStruct { Index = OutwardCellSpace?.Index ?? 0 }).ID
                        };
                    }
                }
            }
            yield break;
        }
    }
}
