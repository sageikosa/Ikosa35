using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Uzi.Core;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Senses;
using Uzi.Ikosa.Tactical;
using Uzi.Visualize;
using Uzi.Visualize.Contracts;

namespace Uzi.Ikosa.Objects
{
    [Serializable]
    public class SecretCornerPivotPortal : CornerPivotPortal, IPanelShade
    {
        public SecretCornerPivotPortal(string name, AnchorFace anchorClose, AnchorFace anchorOpen,
            PortalledObjectBase portObjA, PortalledObjectBase portObjB)
            : base(name, anchorClose, anchorOpen, portObjA, portObjB)
        {
            AddAdjunct(new Searchable(new Deltable(20), true));
            _Cells = new uint[4] { 0, 0, 0, 0 };
        }

        #region Cell Material Indexes
        private uint[] _Cells;
        #endregion

        public CellSpace AnchorCloseIn
        {
            get => (Setting as LocalMap)?.CellSpaces.FirstOrDefault(_cs => _cs.Index == _Cells[0]);
            set => _Cells[0] = value?.Index ?? _Cells[0];
        }

        public CellSpace AnchorCloseOut
        {
            get => (Setting as LocalMap)?.CellSpaces.FirstOrDefault(_cs => _cs.Index == _Cells[1]);
            set => _Cells[1] = value?.Index ?? _Cells[1];
        }

        public CellSpace AnchorOpenIn
        {
            get => (Setting as LocalMap)?.CellSpaces.FirstOrDefault(_cs => _cs.Index == _Cells[2]);
            set => _Cells[2] = value?.Index ?? _Cells[2];
        }

        public CellSpace AnchorOpenOut
        {
            get => (Setting as LocalMap)?.CellSpaces.FirstOrDefault(_cs => _cs.Index == _Cells[3]);
            set => _Cells[3] = value?.Index ?? _Cells[3];
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
                        var _cell = location.Add(AnchorClose);
                        yield return new PanelShadingInfo
                        {
                            Z = _cell.Z,
                            Y = _cell.Y,
                            X = _cell.X,
                            AnchorFace = AnchorClose.ReverseFace(),
                            CellSpace = (new IndexStruct { Index = AnchorCloseIn?.Index ?? 0 }).ID
                        };
                    }
                    else
                    {
                        var _cell = location.Add(AnchorOpen);
                        yield return new PanelShadingInfo
                        {
                            Z = _cell.Z,
                            Y = _cell.Y,
                            X = _cell.X,
                            AnchorFace = AnchorOpen.ReverseFace(),
                            CellSpace = (new IndexStruct { Index = AnchorOpenIn?.Index ?? 0 }).ID
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
                            AnchorFace = AnchorClose,
                            CellSpace = (new IndexStruct { Index = AnchorCloseOut?.Index ?? 0 }).ID
                        };
                    }
                    else
                    {
                        yield return new PanelShadingInfo
                        {
                            Z = location.Z,
                            Y = location.Y,
                            X = location.X,
                            AnchorFace = AnchorOpen,
                            CellSpace = (new IndexStruct { Index = AnchorOpenOut?.Index ?? 0 }).ID
                        };
                    }
                }
            }
            yield break;
        }
    }
}
