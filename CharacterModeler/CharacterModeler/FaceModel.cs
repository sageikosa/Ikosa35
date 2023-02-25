using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace CharacterModeler
{
    public class FaceModel
    {
        public FaceModel()
        {
            UnderTone = Colors.LightSalmon;
            SkinTone = Colors.PeachPuff;
            BrowColor = Colors.Brown;
            BrowThickness = 2;
            BrowLength = 16;
            BrowElevation = 16;
            BrowSeparation = 8;
            BrowArc = 0;
            BrowRotation = 0;

            // eyes
            ScleraColor = Colors.White;
            EyeElevation = 24;
            EyeSeparation = 4;
            EyeWidth = 24;
            EyeTopArc = 4;
            EyeBottomArc = 8;
            EyeRotation = 0;
            PupilColor = Colors.Black;
            PupilHeight = 4;
            PupilWidth = 4;
            IrisColor = Colors.DarkCyan;
            IrisHeight = 8;
            IrisWidth = 8;
            IrisElevation = 24;
            IrisSeparation = 12;
            IrisRotation = 0;

            // nose
            NoseColor = Colors.Peru;
            NoseThickness = 2;
            NoseElevation = 36;
            NoseHeight = 20;
            NoseTopWidth = 8;
            NoseBottomWidth = 12;
            NoseLeft = true;
            NoseRight = true;
            NoseBottomStyle = BridgeEndStyle.Arc;
            NoseCenterOffset = 2;

            // mouth
            MouthFill = Colors.Black;
            MouthElevation = 64;
            MouthHeight = 12;
            MouthTopWidth = 48;
            MouthBottomWidth = 44;
            MouthTopArc = 0;
            MouthBottomArc = 0;

            // dental
            DentalColor = Colors.WhiteSmoke;
            FangLeft = false;
            FangRight = false;
            TuskLeft = false;
            TuskRight = false;

            // fangs
            FangsWidth = 4;
            FangsHeight = 8;
            FangsElevation = 64;
            FangsSeparation = 16;

            // tusks
            TusksWidth = 4;
            TusksHeight = 8;
            TusksElevation = 76;
            TusksSeparation = 8;
        }

        // skin
        public Color UnderTone { get; set; }
        public Color SkinTone { get; set; }

        // brow
        public Color BrowColor { get; set; }
        public double BrowThickness { get; set; }
        public double BrowLength { get; set; }
        public double BrowElevation { get; set; }
        public double BrowSeparation { get; set; }
        public double BrowArc { get; set; }
        public double BrowRotation { get; set; }

        // Eyes
        public Color ScleraColor { get; set; }
        public double EyeElevation { get; set; }
        public double EyeSeparation { get; set; }
        public double EyeWidth { get; set; }
        public double EyeTopArc { get; set; }
        public double EyeBottomArc { get; set; }
        public double EyeRotation { get; set; }

        // pupil
        public Color PupilColor { get; set; }
        public double PupilWidth { get; set; }
        public double PupilHeight { get; set; }

        // iris
        public Color IrisColor { get; set; }
        public double IrisWidth { get; set; }
        public double IrisHeight { get; set; }
        public double IrisElevation { get; set; }
        public double IrisSeparation { get; set; }
        public double IrisRotation { get; set; }

        // bridge of nose
        public Color NoseColor { get; set; }
        public double NoseThickness { get; set; }
        public double NoseElevation { get; set; }
        public double NoseHeight { get; set; }
        public double NoseTopWidth { get; set; }
        public double NoseBottomWidth { get; set; }
        public bool NoseLeft { get; set; }
        public bool NoseRight { get; set; }
        public BridgeEndStyle NoseBottomStyle { get; set; }
        public double NoseCenterOffset { get; set; }

        // mouth
        public Color MouthFill { get; set; }
        public double MouthElevation { get; set; }
        public double MouthHeight { get; set; }
        public double MouthTopWidth { get; set; }
        public double MouthBottomWidth { get; set; }
        public double MouthTopArc { get; set; }
        public double MouthBottomArc { get; set; }

        // dental
        public Color DentalColor { get; set; }
        public bool FangLeft { get; set; }
        public bool FangRight { get; set; }
        public bool TuskLeft { get; set; }
        public bool TuskRight { get; set; }

        // fangs
        public double FangsWidth { get; set; }
        public double FangsHeight { get; set; }
        public double FangsElevation { get; set; }
        public double FangsSeparation { get; set; }

        // tusks
        public double TusksWidth { get; set; }
        public double TusksHeight { get; set; }
        public double TusksElevation { get; set; }
        public double TusksSeparation { get; set; }
    }
}
