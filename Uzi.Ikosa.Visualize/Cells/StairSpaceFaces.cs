using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Uzi.Visualize
{
    public static class StairSpaceFaces
    {
        #region public static AnchorFace GetClimbOpening(uint param)
        /// <summary>000=XL, 001=YL, 010=ZL, 100=XH, 101=YH, 110=ZH</summary>
        public static AnchorFace GetClimbOpening(uint param)
        {
            bool _hiFlag = (param & 4) == 4;
            switch (param & 3)
            {
                case 0:
                    return _hiFlag ? AnchorFace.XHigh : AnchorFace.XLow;
                case 1:
                    return _hiFlag ? AnchorFace.YHigh : AnchorFace.YLow;
                default:
                    return _hiFlag ? AnchorFace.ZHigh : AnchorFace.ZLow;
            }
        }
        #endregion

        #region public static AnchorFace GetTravelOpening(uint param)
        /// <summary>000...=XL, 001...=YL, 010...=ZL, 100...=XH, 101...=YH, 110...=ZH</summary>
        public static AnchorFace GetTravelOpening(uint param)
        {
            bool _hiFlag = (param & 32) == 32;
            if ((param & 16) == 16)
                return _hiFlag ? AnchorFace.ZHigh : AnchorFace.ZLow;
            else if ((param & 8) == 8)
                return _hiFlag ? AnchorFace.YHigh : AnchorFace.YLow;
            else
                return _hiFlag ? AnchorFace.XHigh : AnchorFace.XLow;
        }
        #endregion

        #region public static uint GetParam(AnchorFace climbOpen, AnchorFace travelOpen)
        /// <summary>
        /// <para>climbOpen: ...000=XL, ...001=YL, ...010=ZL, ...100=XH, ...101=YH, ...110=ZH</para>
        /// <para>travelOpen: 000...=XL, 001...=YL, 010...=ZL, 100...=XH, 101...=YH, 110...=ZH</para>
        /// </summary>
        public static uint GetParam(AnchorFace climbOpen, AnchorFace travelOpen)
        {
            uint _param = 0;

            #region climbOpen
            switch (climbOpen)
            {
                case AnchorFace.YLow:
                    _param = 1;
                    break;
                case AnchorFace.ZLow:
                    _param = 2;
                    break;
                case AnchorFace.XHigh:
                    _param = 4;
                    break;
                case AnchorFace.YHigh:
                    _param = 5;
                    break;
                case AnchorFace.ZHigh:
                    _param = 6;
                    break;
            }
            #endregion

            #region travelOpen
            switch (travelOpen)
            {
                case AnchorFace.YLow:
                    _param += 8;
                    break;
                case AnchorFace.ZLow:
                    _param += 16;
                    break;
                case AnchorFace.XHigh:
                    _param += 32;
                    break;
                case AnchorFace.YHigh:
                    _param += 40;
                    break;
                case AnchorFace.ZHigh:
                    _param += 48;
                    break;
            }
            #endregion

            return _param;
        }
        #endregion

        #region public static uint WedgeParallelParam(params AnchorFace[] openings)
        public static uint WedgeParallelParam(params AnchorFace[] openings)
        {
            uint _axis = 0;
            if (!openings.Any(_o => _o.GetAxis() == Axis.X))
            {
                if (openings.Any(_o => _o == AnchorFace.YLow))
                    _axis += 8; // invert primary
                if (openings.Any(_o => _o == AnchorFace.ZLow))
                    _axis += 16; // invert secondary
            }
            else if (!openings.Any(_o => _o.GetAxis() == Axis.Y))
            {
                _axis = 1;
                if (openings.Any(_o => _o == AnchorFace.ZLow))
                    _axis += 8; // invert primary
                if (openings.Any(_o => _o == AnchorFace.XLow))
                    _axis += 16; // invert secondary
            }
            else if (!openings.Any(_o => _o.GetAxis() == Axis.Z))
            {
                _axis = 2;
                if (openings.Any(_o => _o == AnchorFace.XLow))
                    _axis += 8; // invert primary
                if (openings.Any(_o => _o == AnchorFace.YLow))
                    _axis += 16; // invert secondary
            }
            return _axis;
        }
        #endregion

        /// <summary>True if all surface materials are not invisible</summary>
        public static bool OccludesFace(uint param, IPlusCellSpace plusSpace, AnchorFace outwardFace)
        {
            var _climbOpen = GetClimbOpening(param);
            var _travelOpen = GetTravelOpening(param);
            if ((outwardFace == _climbOpen) || (outwardFace == _travelOpen))
                return !plusSpace.IsPlusInvisible;
            if ((outwardFace == _climbOpen.ReverseFace()) || (outwardFace == _travelOpen.ReverseFace()))
                return !plusSpace.IsInvisible;
            return !plusSpace.IsInvisible && !plusSpace.IsPlusInvisible;
        }

        /// <summary>True if any material at surface is not invisible </summary>
        public static bool ShowFace(uint param, IPlusCellSpace plusSpace, AnchorFace outwardFace)
        {
            var _climbOpen = GetClimbOpening(param);
            var _travelOpen = GetTravelOpening(param);
            if ((outwardFace == _climbOpen) || (outwardFace == _travelOpen))
                return !plusSpace.IsPlusInvisible;
            if ((outwardFace == _climbOpen.ReverseFace()) || (outwardFace == _travelOpen.ReverseFace()))
                return !plusSpace.IsInvisible;
            return !plusSpace.IsInvisible || !plusSpace.IsPlusInvisible;
        }
    }
}
