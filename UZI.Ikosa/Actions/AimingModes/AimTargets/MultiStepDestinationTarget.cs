using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Media.Media3D;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Contracts;
using Uzi.Visualize;
using static System.Math;

namespace Uzi.Ikosa.Actions
{
    [Serializable]
    public class MultiStepDestinationTarget : StepDestinationTarget
    {
        public MultiStepDestinationTarget(string key, ICellLocation offset)
            : base(key)
        {
            _Offset = new CellPosition(offset);
            _Steps = [];

            // iterations
            var _count = StepCount;
            var _dCount = (decimal)_count;
            for (var _sx = 0; _sx < _count; _sx++)
            {
                _Steps.Add(AnchorFaceList.None);
            }

            if (_count > 0)
            {
                // how many of each step
                var _zlCount = _Offset.Z < 0 ? Abs(_Offset.Z) : 0m;
                var _ylCount = _Offset.Y < 0 ? Abs(_Offset.Y) : 0m;
                var _xlCount = _Offset.X < 0 ? Abs(_Offset.X) : 0m;
                var _zhCount = _Offset.Z > 0 ? _Offset.Z : 0m;
                var _yhCount = _Offset.Y > 0 ? _Offset.Y : 0m;
                var _xhCount = _Offset.X > 0 ? _Offset.X : 0m;

                // how often for each step
                var _zlStep = (_zlCount > 0) ? _dCount / _zlCount : 0m;
                var _ylStep = (_ylCount > 0) ? _dCount / _ylCount : 0m;
                var _xlStep = (_xlCount > 0) ? _dCount / _xlCount : 0m;
                var _zhStep = (_zhCount > 0) ? _dCount / _zhCount : 0m;
                var _yhStep = (_yhCount > 0) ? _dCount / _yhCount : 0m;
                var _xhStep = (_xhCount > 0) ? _dCount / _xhCount : 0m;

                int _index(int sx, decimal step) => Convert.ToInt32(Round(sx * step));
                for (var _sx = 0; _sx < _zlCount; _sx++)
                {
                    _Steps[_index(_sx, _zlStep)] |= AnchorFaceList.ZLow;
                }

                for (var _sx = 0; _sx < _ylCount; _sx++)
                {
                    _Steps[_index(_sx, _ylStep)] |= AnchorFaceList.YLow;
                }

                for (var _sx = 0; _sx < _xlCount; _sx++)
                {
                    _Steps[_index(_sx, _xlStep)] |= AnchorFaceList.XLow;
                }

                for (var _sx = 0; _sx < _zhCount; _sx++)
                {
                    _Steps[_index(_sx, _zhStep)] |= AnchorFaceList.ZHigh;
                }

                for (var _sx = 0; _sx < _yhCount; _sx++)
                {
                    _Steps[_index(_sx, _yhStep)] |= AnchorFaceList.YHigh;
                }

                for (var _sx = 0; _sx < _xhCount; _sx++)
                {
                    _Steps[_index(_sx, _xhStep)] |= AnchorFaceList.XHigh;
                }
            }
            // TODO: need path bias, (eg, source cell exit and longest offset ordinate)
            // TODO: for when their are alternate valid paths
            // TODO: there is only one path if the ratio of any non-zero ordinate to any other is 1
        }

        #region private data
        private CellPosition _Offset;
        private List<AnchorFaceList> _Steps;
        #endregion

        public ICellLocation Offset
            => _Offset;

        /// <summary>Reports how many steps are in the destination</summary>
        public override int StepCount
            => Max(Max(Abs(_Offset.Z), Abs(_Offset.Y)), Abs(_Offset.X));

        /// <summary>Gets the crossing faces for a particular step in the sequence</summary>
        public override AnchorFace[] CrossingFaces(AnchorFace gravity, int stepIndex)
            => _Steps[stepIndex].ToAnchorFaces().ToArray();

        #region public override int GetHeading(AnchorFace gravity, int stepIndex)
        public override int GetHeading(AnchorFace gravity, int stepIndex)
        {
            var _step = _Steps[stepIndex];

            #region heading mapper
            int _heading(AnchorFace front, AnchorFace left)
            {
                if (_step.Contains(front))
                {
                    if (_step.Contains(left))
                    {
                        return 1;
                    }
                    else if (_step.Contains(left.ReverseFace()))
                    {
                        return 7;
                    }

                    return 0;
                }
                else if (_step.Contains(front.ReverseFace()))
                {
                    if (_step.Contains(left))
                    {
                        return 3;
                    }
                    else if (_step.Contains(left.ReverseFace()))
                    {
                        return 5;
                    }

                    return 4;
                }
                if (_step.Contains(left))
                {
                    return 2;
                }
                else if (_step.Contains(left.ReverseFace()))
                {
                    return 6;
                }

                return 8;
            };
            #endregion

            switch (gravity)
            {
                case AnchorFace.ZLow:
                    return _heading(AnchorFace.XHigh, AnchorFace.YHigh);
                case AnchorFace.ZHigh:
                    return _heading(AnchorFace.XHigh, AnchorFace.YLow);
                case AnchorFace.YLow:
                    return _heading(AnchorFace.XHigh, AnchorFace.ZLow);
                case AnchorFace.YHigh:
                    return _heading(AnchorFace.XHigh, AnchorFace.ZHigh);
                case AnchorFace.XLow:
                    return _heading(AnchorFace.ZLow, AnchorFace.YHigh);
                case AnchorFace.XHigh:
                    return _heading(AnchorFace.ZHigh, AnchorFace.YHigh);
            }
            return 8;
        }
        #endregion

        #region public override UpDownAdjustment GetUpDownAdjustment(AnchorFace gravity, int stepIndex)
        public override UpDownAdjustment GetUpDownAdjustment(AnchorFace gravity, int stepIndex)
        {
            var _step = _Steps[stepIndex];

            if (_step.Contains(gravity))
            {
                // going down
                if (_step.Count() == 1)
                {
                    // and only down
                    return UpDownAdjustment.StraightDown;
                }
                return UpDownAdjustment.Downward;
            }
            else if (_step.Contains(gravity.ReverseFace()))
            {
                // going up
                if (_step.Count() == 1)
                {
                    // and only up
                    return UpDownAdjustment.StraightUp;
                }
                return UpDownAdjustment.Upward;
            }

            // no gravity adjustment
            return UpDownAdjustment.Level;
        }
        #endregion

        public override AimTargetInfo GetTargetInfo()
        {
            return new MultiStepDestinationInfo
            {
                XSteps = _Offset.X,
                YSteps = _Offset.Y,
                ZSteps = _Offset.Z,
                Key = Key,
                TargetID = (Target != null) ? (Guid?)Target.ID : null
            };
        }
    }
}
