using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Uzi.Visualize
{
    public static class LFrameSpaceFaces
    {
        public static uint GetParam(AnchorFace thick, AnchorFace frame1, AnchorFace frame2)
        {
            uint _param = 0;

            #region thick face
            switch (thick)
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

            #region frame1 face
            switch (frame1)
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

            #region frame2 face
            switch (frame2)
            {
                case AnchorFace.YLow:
                    _param += 64;
                    break;
                case AnchorFace.ZLow:
                    _param += 128;
                    break;
                case AnchorFace.XHigh:
                    _param += 256;
                    break;
                case AnchorFace.YHigh:
                    _param += 320;
                    break;
                case AnchorFace.ZHigh:
                    _param += 384;
                    break;
            }
            #endregion

            return _param;
        }

        #region public static AnchorFace GetThickFace(uint param)
        public static AnchorFace GetThickFace(uint param)
        {
            if ((param & 4) == 4)
            {
                if ((param & 2) == 2)
                    return AnchorFace.ZHigh;
                else if ((param & 1) == 1)
                    return AnchorFace.YHigh;
                else
                    return AnchorFace.XHigh;
            }
            else if ((param & 2) == 2)
                return AnchorFace.ZLow;
            else if ((param & 1) == 1)
                return AnchorFace.YLow;
            else
                return AnchorFace.XLow;
        }
        #endregion

        #region public static AnchorFace GetFrame1Face(uint param)
        public static AnchorFace GetFrame1Face(uint param)
        {
            if ((param & 32) == 32)
            {
                if ((param & 16) == 16)
                    return AnchorFace.ZHigh;
                else if ((param & 8) == 8)
                    return AnchorFace.YHigh;
                else
                    return AnchorFace.XHigh;
            }
            else if ((param & 16) == 16)
                return AnchorFace.ZLow;
            else if ((param & 8) == 8)
                return AnchorFace.YLow;
            else
                return AnchorFace.XLow;
        }
        #endregion

        #region public static AnchorFace GetFrame2Face(uint param)
        public static AnchorFace GetFrame2Face(uint param)
        {
            if ((param & 256) == 256)
            {
                if ((param & 128) == 128)
                    return AnchorFace.ZHigh;
                else if ((param & 64) == 64)
                    return AnchorFace.YHigh;
                else
                    return AnchorFace.XHigh;
            }
            else if ((param & 128) == 128)
                return AnchorFace.ZLow;
            else if ((param & 64) == 64)
                return AnchorFace.YLow;
            else
                return AnchorFace.XLow;
        }
        #endregion

        public static bool OccludesFace(uint param, IPlusCellSpace plusSpace, AnchorFace outwardFace)
        {
            var _thickFace = GetThickFace(param);
            if (_thickFace.ReverseFace() == outwardFace)
                return !plusSpace.IsPlusInvisible;

            // every other face has some part of both materials
            return !plusSpace.IsPlusInvisible && !plusSpace.IsInvisible;
        }

        public static bool ShowFace(uint param, IPlusCellSpace plusSpace, AnchorFace outwardFace)
        {
            var _thickFace = GetThickFace(param);
            if (_thickFace.ReverseFace() == outwardFace)
                return !plusSpace.IsPlusInvisible;

            // every other face has some part of both materials
            return !plusSpace.IsPlusInvisible || !plusSpace.IsInvisible;
        }
    }
}
