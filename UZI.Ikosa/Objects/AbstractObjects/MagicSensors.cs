using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Media3D;
using Uzi.Core;
using Uzi.Core.Dice;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Senses;
using Uzi.Ikosa.Skills;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Time;
using Uzi.Visualize;

namespace Uzi.Ikosa.Objects
{
    [Serializable]
    public class MagicSensors : MagicEffectHolder, ISensorHost
    {
        protected MagicSensors(string name, Size size, SensorySet senses, IGeometricSize geomSize, string iconKey,
            bool isVisible, bool isMagicDarkBlind)
            : base(name, size, geomSize, iconKey, isVisible)
        {
            // senses bind and unbind
            _Senses = senses;

            // sounds are connected to the sensor host, so can keep state
            _Sounds = new SoundAwarenessSet(this);

            _MagicDarkBlind = isMagicDarkBlind;

            // randomize the starting heading
            _Heading = DieRoller.CreateRoller(StandardDieType.d8).GetRollerLog().Total - 1;

            // NOTE: all other state is formed and removed when the adjunct is added/removed
        }

        #region state
        private RemoteSenseTarget _Target;
        private SensorySet _Senses;
        private AwarenessSet _Awareness;
        private ExtraInfoSet _DivMarks;
        private RoomAwarenessSet _Rooms;
        private SoundAwarenessSet _Sounds;
        private double _AimRelLong = 0d;
        private double _AimLat = 0d;
        private double _AimDist = 0d;
        private Point3D _AimPt = new Point3D();
        private int _ThirdRelHeading = 4;
        private int _ThirdIncline = 1;
        private Point3D _ThirdPt = new Point3D();
        private int _Heading = 1;
        private int _Incline = 0;
        private bool _MagicDarkBlind;
        #endregion

        /// <summary>
        /// Create a sensors holder in context with planar presence.  Call in ApplySpell (generally).
        /// SpellMode should implement IDurableAnchorMode and Destroy the MagicSensors during OnEndAnchor.
        /// </summary>
        public static MagicSensors CreateSensors(string name, Size size, SensorySet senses, IGeometricSize geometricSize,
            string iconKey, string modelKey, bool ethereal, ICellLocation location, MapContext mapContext,
            bool isVisible, bool isMagicDarkBlind)
        {
            var _sensors = new MagicSensors(name, size, senses, geometricSize, iconKey, isVisible, isMagicDarkBlind);
            if (ethereal)
            {
                // if actor was ethereal, the effect will be ethereal (even if the actor ceases to be)
                _sensors.AddAdjunct(new EtherealEffect(_sensors, null));
            }
            new ObjectPresenter(_sensors, mapContext, modelKey, _sensors.GeometricSize, new Cubic(location, _sensors.GeometricSize));
            return _sensors;
        }

        #region bool AddAdjunct(Adjunct adjunct)
        public override bool AddAdjunct(Adjunct adjunct)
        {
            if (adjunct is RemoteSenseTarget _target)
            {
                if (_Target == null)
                {
                    if (base.AddAdjunct(_target))
                    {
                        _Target = _target;
                        Senses.BindTo(Creature);
                        _Awareness = new AwarenessSet(Creature);
                        _DivMarks = new ExtraInfoSet(Creature);
                        _Rooms = new RoomAwarenessSet(Creature);

                        // once the senses are turned on, refresh awareness
                        // awareness is tracked in the map, so necessary to prime
                        // normally an active sensor is added to the map
                        RoomAwarenesses.RecalculateAwareness(this);
                        Awarenesses.RecalculateAwareness(this);
                        return true;
                    }
                }
                return false;
            }
            else if (_MagicDarkBlind && adjunct is DarknessShrouded _shroud)
            {
                if (base.AddAdjunct(adjunct))
                {
                    foreach (var _sense in Senses.AllSenses.Where(_s => _s.UsesSight).ToList())
                    {
                        _sense.IsActive = false;
                    }
                    return true;
                }
                return false;
            }
            else
            {
                return base.AddAdjunct(adjunct);
            }
        }
        #endregion

        #region RemoveAdjunct(Adjunct adjunct)
        public override bool RemoveAdjunct(Adjunct adjunct)
        {
            var _return = base.RemoveAdjunct(adjunct);
            if (_return)
            {
                if (_Target == adjunct)
                {
                    _Sounds.Cleanup();
                    _Rooms.ClearAwarenesses(this, Creature.Setting as LocalMap);
                    _Rooms = null;
                    _DivMarks = null;
                    _Awareness.Clear();
                    _Awareness = null;
                    _Target = null;
                    Senses.UnbindFromCreature();
                }
                else if (adjunct is DarknessShrouded _shroud
                    && !Adjuncts.OfType<DarknessShrouded>().Any(_ds => _ds.ID != _shroud.ID && _ds.IsActive))
                {
                    foreach (var _sense in Senses.AllSenses.Where(_s => _s.UsesSight).ToList())
                    {
                        _sense.IsActive = true;
                    }
                }
            }
            return _return;
        }
        #endregion

        public string SensorHostName => Name;

        public bool IsSensorHostActive => _Senses.Creature != null;

        public SensorySet Senses => _Senses;

        public Creature Creature => _Target?.Creature;
        public AwarenessSet Awarenesses => _Awareness;
        public ExtraInfoSet ExtraInfoMarkers => _DivMarks;
        public RoomAwarenessSet RoomAwarenesses => _Rooms;
        public SoundAwarenessSet SoundAwarenesses => _Sounds;
        public SkillSet Skills => _Target?.Creature?.Skills;

        #region public double ZOffset { get; }
        public double ZOffset
        {
            get
            {
                var _loc = this.GetLocated();
                if (_loc != null)
                {
                    var _locator = _loc.Locator;
                    var _region = _locator.GeometricRegion;
                    var _extent = (_region.UpperZ - _region.LowerZ + 1);
                    var _off = _locator.IntraModelOffset.Z;
                    return _extent * 2.5 + _off;
                }
                return 2.5d;
            }
        }
        #endregion

        #region public double YOffset { get; }
        public double YOffset
        {
            get
            {
                var _loc = this.GetLocated();
                if (_loc != null)
                {
                    var _locator = _loc.Locator;
                    var _region = _locator.GeometricRegion;
                    var _extent = (_region.UpperY - _region.LowerY + 1);
                    var _off = _locator.IntraModelOffset.Y;
                    return _extent * 2.5 + _off;
                }
                return 2.5d;
            }
        }
        #endregion

        #region public double XOffset { get; }
        public double XOffset
        {
            get
            {
                var _loc = this.GetLocated();
                if (_loc != null)
                {
                    var _locator = _loc.Locator;
                    var _region = _locator.GeometricRegion;
                    var _extent = (_region.UpperX - _region.LowerX + 1);
                    var _off = _locator.IntraModelOffset.X;
                    return _extent * 2.5 + _off;
                }
                return 2.5d;
            }
        }
        #endregion

        #region public int Heading { get; set; }
        public int Heading
        {
            get => _Heading;
            set
            {
                _Heading = value;
                var _loc = this.GetLocated();
                if (_loc != null)
                {
                    _AimPt = _loc.Locator.ResyncTacticalPoint(this, AimPointRelativeLongitude, AimPointLatitude, AimPointDistance);
                    _ThirdPt = _loc.Locator.ResyncTacticalPoint(this, ThirdCameraRelativeHeading * 45d, ThirdCameraIncline * 45d, ThirdCameraDistance);
                }
            }
        }
        #endregion

        public int Incline { get => _Incline; set => _Incline = value; }

        public double AimPointRelativeLongitude { get => _AimRelLong; set => _AimRelLong = value; }
        public double AimPointLatitude { get => _AimLat; set => _AimLat = value; }
        public double AimPointDistance { get => _AimDist; set => _AimDist = value; }

        public Point3D AimPoint { get => _AimPt; set => _AimPt = value; }

        public int ThirdCameraRelativeHeading { get => _ThirdRelHeading; set => _ThirdRelHeading = value; }
        public int ThirdCameraIncline { get => _ThirdIncline; set => _ThirdIncline = value; }

        #region public double ThirdCameraDistance { get; }
        public double ThirdCameraDistance
        {
            get
            {
                var _loc = this.GetLocated();
                if (_loc != null)
                {
                    var _zExt = _loc.Locator.ZFit;
                    var _yExt = _loc.Locator.YFit;
                    var _xExt = _loc.Locator.XFit;
                    return 0.5d * Math.Sqrt((_zExt * _zExt) + (_yExt * _yExt) + (_xExt * _xExt));
                }
                return AimPointDistance;
            }
        }
        #endregion

        public Point3D ThirdCameraPoint { get => _ThirdPt; set => _ThirdPt = value; }

        public double Resolution => _Target?.Creature?.Resolution ?? Round.UnitFactor;

        public void TrackTime(double timeVal, TimeValTransition direction)
        {
        }
    }
}
