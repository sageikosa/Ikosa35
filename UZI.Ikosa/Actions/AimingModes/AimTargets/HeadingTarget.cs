using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Visualize;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Actions
{
    [Serializable]
    public class HeadingTarget : StepDestinationTarget
    {
        #region construction
        public HeadingTarget(string key, int heading, int upDownAdjust)
            : base(key)
        {
            _Heading = heading;
            _UpDownAdjust = upDownAdjust;
        }
        #endregion

        #region private data
        private int _Heading;
        private int _UpDownAdjust;
        private const short CENTER = 13;
        private const short XLOW = -1;
        private const short XHI = 1;
        private const short YLOW = -3;
        private const short YHI = 3;
        private const short ZLOW = -9;
        private const short ZHI = 9;
        #endregion

        public int Heading => _Heading;
        public int UpDownAdjust => _UpDownAdjust;

        public override int GetHeading(AnchorFace gravity, int stepIndex) => _Heading;
        public override UpDownAdjustment GetUpDownAdjustment(AnchorFace gravity, int index) => (UpDownAdjustment)_UpDownAdjust;

        /// <summary>Reports how many steps are in the destination</summary>
        public override int StepCount => 1;

        /// <summary>Gets the crossing faces for a particular step in the sequence</summary>
        public override AnchorFace[] CrossingFaces(AnchorFace gravity, int stepIndex)
            => AnchorFaceHelper.MovementFaces(gravity, Heading, UpDownAdjust).ToArray();

        #region public IEnumerable<AnchorFace[]> AdjacentCrossings(AnchorFace gravity, int stepIndex)
        /// <summary>Used to find testing near paths when projected path is blocked</summary>
        public IEnumerable<AnchorFace[]> AdjacentCrossings(AnchorFace gravity, int stepIndex)
        {
            switch (GetByteValue(gravity, stepIndex))
            {
                case CENTER + XHI:
                    yield return new AnchorFace[] { AnchorFace.XHigh, AnchorFace.YHigh };
                    yield return new AnchorFace[] { AnchorFace.XHigh, AnchorFace.YLow };
                    yield return new AnchorFace[] { AnchorFace.XHigh, AnchorFace.ZHigh };
                    yield return new AnchorFace[] { AnchorFace.XHigh, AnchorFace.ZLow };
                    break;
                case CENTER + XLOW:
                    yield return new AnchorFace[] { AnchorFace.XLow, AnchorFace.YHigh };
                    yield return new AnchorFace[] { AnchorFace.XLow, AnchorFace.YLow };
                    yield return new AnchorFace[] { AnchorFace.XLow, AnchorFace.ZHigh };
                    yield return new AnchorFace[] { AnchorFace.XLow, AnchorFace.ZLow };
                    break;
                case CENTER + YHI:
                    yield return new AnchorFace[] { AnchorFace.YHigh, AnchorFace.XHigh };
                    yield return new AnchorFace[] { AnchorFace.YHigh, AnchorFace.XLow };
                    yield return new AnchorFace[] { AnchorFace.YHigh, AnchorFace.ZHigh };
                    yield return new AnchorFace[] { AnchorFace.YHigh, AnchorFace.ZLow };
                    break;
                case CENTER + YHI + XLOW:
                    yield return new AnchorFace[] { AnchorFace.XLow };
                    yield return new AnchorFace[] { AnchorFace.YHigh };
                    yield return new AnchorFace[] { AnchorFace.YHigh, AnchorFace.XLow, AnchorFace.ZHigh };
                    yield return new AnchorFace[] { AnchorFace.YHigh, AnchorFace.XLow, AnchorFace.ZLow };
                    break;
                case CENTER + YHI + XHI:
                    yield return new AnchorFace[] { AnchorFace.XHigh };
                    yield return new AnchorFace[] { AnchorFace.YHigh };
                    yield return new AnchorFace[] { AnchorFace.YHigh, AnchorFace.XHigh, AnchorFace.ZHigh };
                    yield return new AnchorFace[] { AnchorFace.YHigh, AnchorFace.XHigh, AnchorFace.ZLow };
                    break;
                case CENTER + YLOW:
                    yield return new AnchorFace[] { AnchorFace.YLow, AnchorFace.XHigh };
                    yield return new AnchorFace[] { AnchorFace.YLow, AnchorFace.XLow };
                    yield return new AnchorFace[] { AnchorFace.YLow, AnchorFace.ZHigh };
                    yield return new AnchorFace[] { AnchorFace.YLow, AnchorFace.ZLow };
                    break;
                case CENTER + YLOW + XLOW:
                    yield return new AnchorFace[] { AnchorFace.XLow };
                    yield return new AnchorFace[] { AnchorFace.YLow };
                    yield return new AnchorFace[] { AnchorFace.YLow, AnchorFace.XLow, AnchorFace.ZHigh };
                    yield return new AnchorFace[] { AnchorFace.YLow, AnchorFace.XLow, AnchorFace.ZLow };
                    break;
                case CENTER + YLOW + XHI:
                    yield return new AnchorFace[] { AnchorFace.XHigh };
                    yield return new AnchorFace[] { AnchorFace.YLow };
                    yield return new AnchorFace[] { AnchorFace.YLow, AnchorFace.XHigh, AnchorFace.ZHigh };
                    yield return new AnchorFace[] { AnchorFace.YLow, AnchorFace.XHigh, AnchorFace.ZLow };
                    break;
                case CENTER + ZHI:
                    yield return new AnchorFace[] { AnchorFace.ZHigh, AnchorFace.XHigh };
                    yield return new AnchorFace[] { AnchorFace.ZHigh, AnchorFace.XLow };
                    yield return new AnchorFace[] { AnchorFace.ZHigh, AnchorFace.YHigh };
                    yield return new AnchorFace[] { AnchorFace.ZHigh, AnchorFace.YLow };
                    break;
                case CENTER + ZHI + XHI:
                    yield return new AnchorFace[] { AnchorFace.XHigh };
                    yield return new AnchorFace[] { AnchorFace.ZHigh };
                    yield return new AnchorFace[] { AnchorFace.ZHigh, AnchorFace.XHigh, AnchorFace.YHigh };
                    yield return new AnchorFace[] { AnchorFace.ZHigh, AnchorFace.XHigh, AnchorFace.YLow };
                    break;
                case CENTER + ZHI + YHI:
                    yield return new AnchorFace[] { AnchorFace.YHigh };
                    yield return new AnchorFace[] { AnchorFace.ZHigh };
                    yield return new AnchorFace[] { AnchorFace.ZHigh, AnchorFace.YHigh, AnchorFace.XHigh };
                    yield return new AnchorFace[] { AnchorFace.ZHigh, AnchorFace.YHigh, AnchorFace.XLow };
                    break;
                case CENTER + ZHI + YHI + XLOW:
                    yield return new AnchorFace[] { AnchorFace.ZHigh, AnchorFace.YHigh };
                    yield return new AnchorFace[] { AnchorFace.ZHigh, AnchorFace.XLow };
                    yield return new AnchorFace[] { AnchorFace.YHigh, AnchorFace.XLow };
                    break;
                case CENTER + ZHI + YHI + XHI:
                    yield return new AnchorFace[] { AnchorFace.ZHigh, AnchorFace.YHigh };
                    yield return new AnchorFace[] { AnchorFace.ZHigh, AnchorFace.XHigh };
                    yield return new AnchorFace[] { AnchorFace.YHigh, AnchorFace.XHigh };
                    break;
                case CENTER + ZHI + XLOW:
                    yield return new AnchorFace[] { AnchorFace.XLow };
                    yield return new AnchorFace[] { AnchorFace.ZHigh };
                    yield return new AnchorFace[] { AnchorFace.ZHigh, AnchorFace.XLow, AnchorFace.YHigh };
                    yield return new AnchorFace[] { AnchorFace.ZHigh, AnchorFace.XLow, AnchorFace.YLow };
                    break;
                case CENTER + ZHI + YLOW:
                    yield return new AnchorFace[] { AnchorFace.YLow };
                    yield return new AnchorFace[] { AnchorFace.ZHigh };
                    yield return new AnchorFace[] { AnchorFace.ZHigh, AnchorFace.YLow, AnchorFace.XHigh };
                    yield return new AnchorFace[] { AnchorFace.ZHigh, AnchorFace.YLow, AnchorFace.XLow };
                    break;
                case CENTER + ZHI + YLOW + XLOW:
                    yield return new AnchorFace[] { AnchorFace.ZHigh, AnchorFace.YLow };
                    yield return new AnchorFace[] { AnchorFace.ZHigh, AnchorFace.XLow };
                    yield return new AnchorFace[] { AnchorFace.YLow, AnchorFace.XLow };
                    break;
                case CENTER + ZHI + YLOW + XHI:
                    yield return new AnchorFace[] { AnchorFace.ZHigh, AnchorFace.YLow };
                    yield return new AnchorFace[] { AnchorFace.ZHigh, AnchorFace.XHigh };
                    yield return new AnchorFace[] { AnchorFace.YLow, AnchorFace.XHigh };
                    break;
                case CENTER + ZLOW:
                    yield return new AnchorFace[] { AnchorFace.ZLow, AnchorFace.XHigh };
                    yield return new AnchorFace[] { AnchorFace.ZLow, AnchorFace.XLow };
                    yield return new AnchorFace[] { AnchorFace.ZLow, AnchorFace.YHigh };
                    yield return new AnchorFace[] { AnchorFace.ZLow, AnchorFace.YLow };
                    break;
                case CENTER + ZLOW + XHI:
                    yield return new AnchorFace[] { AnchorFace.XHigh };
                    yield return new AnchorFace[] { AnchorFace.ZLow };
                    yield return new AnchorFace[] { AnchorFace.ZLow, AnchorFace.XHigh, AnchorFace.YHigh };
                    yield return new AnchorFace[] { AnchorFace.ZLow, AnchorFace.XHigh, AnchorFace.YLow };
                    break;
                case CENTER + ZLOW + YHI:
                    yield return new AnchorFace[] { AnchorFace.YHigh };
                    yield return new AnchorFace[] { AnchorFace.ZLow };
                    yield return new AnchorFace[] { AnchorFace.ZLow, AnchorFace.YHigh, AnchorFace.XHigh };
                    yield return new AnchorFace[] { AnchorFace.ZLow, AnchorFace.YHigh, AnchorFace.XLow };
                    break;
                case CENTER + ZLOW + YHI + XLOW:
                    yield return new AnchorFace[] { AnchorFace.ZLow, AnchorFace.YHigh };
                    yield return new AnchorFace[] { AnchorFace.ZLow, AnchorFace.XLow };
                    yield return new AnchorFace[] { AnchorFace.YHigh, AnchorFace.XLow };
                    break;
                case CENTER + ZLOW + YHI + XHI:
                    yield return new AnchorFace[] { AnchorFace.ZLow, AnchorFace.YHigh };
                    yield return new AnchorFace[] { AnchorFace.ZLow, AnchorFace.XHigh };
                    yield return new AnchorFace[] { AnchorFace.YHigh, AnchorFace.XHigh };
                    break;
                case CENTER + ZLOW + XLOW:
                    yield return new AnchorFace[] { AnchorFace.XLow };
                    yield return new AnchorFace[] { AnchorFace.ZLow };
                    yield return new AnchorFace[] { AnchorFace.ZLow, AnchorFace.XLow, AnchorFace.YHigh };
                    yield return new AnchorFace[] { AnchorFace.ZLow, AnchorFace.XLow, AnchorFace.YLow };
                    break;
                case CENTER + ZLOW + YLOW:
                    yield return new AnchorFace[] { AnchorFace.YLow };
                    yield return new AnchorFace[] { AnchorFace.ZLow };
                    yield return new AnchorFace[] { AnchorFace.ZLow, AnchorFace.YLow, AnchorFace.XHigh };
                    yield return new AnchorFace[] { AnchorFace.ZLow, AnchorFace.YLow, AnchorFace.XLow };
                    break;
                case CENTER + ZLOW + YLOW + XLOW:
                    yield return new AnchorFace[] { AnchorFace.ZLow, AnchorFace.YLow };
                    yield return new AnchorFace[] { AnchorFace.ZLow, AnchorFace.XLow };
                    yield return new AnchorFace[] { AnchorFace.YLow, AnchorFace.XLow };
                    break;
                case CENTER + ZLOW + YLOW + XHI:
                    yield return new AnchorFace[] { AnchorFace.ZLow, AnchorFace.YLow };
                    yield return new AnchorFace[] { AnchorFace.ZLow, AnchorFace.XHigh };
                    yield return new AnchorFace[] { AnchorFace.YLow, AnchorFace.XHigh };
                    break;
            }
            yield break;
        }
        #endregion

        #region private byte GetByteValue(AnchorFace gravity, int stepIndex)
        /// <summary>Provides a linear representation of face vector in range of 0 to 26</summary>
        private byte GetByteValue(AnchorFace gravity, int stepIndex)
        {
            short _result = CENTER;
            var _crossings = CrossingFaces(gravity, stepIndex);
            // X
            if (_crossings.Contains(AnchorFace.XLow))
            {
                _result += XLOW;
            }
            else if (_crossings.Contains(AnchorFace.XHigh))
            {
                _result += XHI;
            }
            // Y
            if (_crossings.Contains(AnchorFace.YLow))
            {
                _result += YLOW;
            }
            else if (_crossings.Contains(AnchorFace.YHigh))
            {
                _result += YHI;
            }
            // Z
            if (_crossings.Contains(AnchorFace.ZLow))
            {
                _result += ZLOW;
            }
            else if (_crossings.Contains(AnchorFace.ZHigh))
            {
                _result += ZHI;
            }

            return (byte)_result;
        }
        #endregion

        public override AimTargetInfo GetTargetInfo()
        {
            return new HeadingTargetInfo
            {
                Heading = this.Heading,
                UpDownAdjust = this.UpDownAdjust,
                Key = this.Key,
                TargetID = (this.Target != null) ? (Guid?)this.Target.ID : null
            };
        }
    }
}
