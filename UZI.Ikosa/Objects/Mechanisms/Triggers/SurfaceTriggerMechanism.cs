using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uzi.Core;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Items.Materials;
using Uzi.Ikosa.Tactical;
using Uzi.Visualize;

namespace Uzi.Ikosa.Objects
{
    [Serializable]
    public class SurfaceTriggerMechanism : TriggerMechanism
    {
        #region ctor()
        public SurfaceTriggerMechanism(string name, Material material, int disableDifficulty,
            PostTriggerState postState, AnchorFaceList surfaces, double tripWeight, MechanismMount mount)
            : base(name, material, disableDifficulty, postState)
        {
            _Surfaces = surfaces;
            _TripWeight = tripWeight;

            // this mechanism is the master of a group
            var _stGroup = new SurfaceTriggerGroup();
            var _stMaster = new SurfaceTriggerMaster(_stGroup);
            AddAdjunct(_stMaster);

            // connect to a mechanism mount...
            var _stTarget = new SurfaceTriggerTarget(_stGroup);
            mount?.AddAdjunct(_stTarget);
            ObjectSizer.NaturalSize = Size.Small;
        }
        #endregion

        public SurfaceTriggerMaster SurfaceTriggerMaster
            => Adjuncts.OfType<SurfaceTriggerMaster>().FirstOrDefault();

        #region public MechanismMount MechanismMount { get; set; }
        public MechanismMount MechanismMount
        {
            get => SurfaceTriggerMaster?.SurfaceTriggerGroup.SurfaceTriggerTarget?.MechanismMount;
            set
            {
                var _target = SurfaceTriggerMaster?.SurfaceTriggerGroup.SurfaceTriggerTarget;
                if (_target?.MechanismMount != null)
                {
                    // stop monitoring the old one, and de-link it
                    _target.Eject();
                }

                // link the new one
                value?.AddAdjunct(_target);
            }
        }
        #endregion

        #region data
        private double _TripWeight;
        private AnchorFaceList _Surfaces;
        #endregion

        public AnchorFaceList Surfaces
        {
            get => _Surfaces;
            set
            {
                _Surfaces = value;
                DoPropertyChanged(nameof(Surfaces));
            }
        }

        public double TripWeight
        {
            get => _TripWeight;
            set
            {
                _TripWeight = value;
                DoPropertyChanged(nameof(TripWeight));
            }
        }

        protected override string ClassIconKey => nameof(SurfaceTriggerMechanism);
    }

    [Serializable]
    public class SurfaceTriggerGroup : TriggerMountGroup<SurfaceTriggerMechanism>
    {
        public SurfaceTriggerGroup()
            : base(typeof(SurfaceTriggerGroup))
        {
        }

        public SurfaceTriggerMaster SurfaceTriggerMaster => Master as SurfaceTriggerMaster;
        public SurfaceTriggerTarget SurfaceTriggerTarget => Target as SurfaceTriggerTarget;

        public override void ValidateGroup()
            => this.ValidateMasteredPlanarLink();
    }

    [Serializable]
    public class SurfaceTriggerMaster : TriggerMountMaster<SurfaceTriggerMechanism>
    {
        public SurfaceTriggerMaster(SurfaceTriggerGroup group)
            : base(group)
        {
        }

        public override bool CanAnchor(IAdjunctable newAnchor)
            => (newAnchor is SurfaceTriggerMechanism) && base.CanAnchor(newAnchor);

        public SurfaceTriggerGroup SurfaceTriggerGroup
            => TriggerGroup as SurfaceTriggerGroup;

        public override object Clone()
            => new SurfaceTriggerMaster(SurfaceTriggerGroup);
    }

    [Serializable]
    public class SurfaceTriggerTarget : TriggerMountTarget<SurfaceTriggerMechanism>, IInteractHandler,
        IMonitorChange<Physical>
    {
        public SurfaceTriggerTarget(SurfaceTriggerGroup group)
            : base(group)
        {
            _PhysCtrlrs = [];
        }

        #region data
        private List<IControlChange<Physical>> _PhysCtrlrs;
        #endregion

        public override bool CanAnchor(IAdjunctable newAnchor)
            => (newAnchor is MechanismMount) && base.CanAnchor(newAnchor);

        #region OnActivate/OnDeactivate: watch for "captured" locator weight changes
        protected override void OnActivate(object source)
        {
            MechanismMount?.AddIInteractHandler(this);
            base.OnActivate(source);
        }

        protected override void OnDeactivate(object source)
        {
            MechanismMount?.RemoveIInteractHandler(this);

            // stop monitoring change controllers
            foreach (var _pc in _PhysCtrlrs.ToList())
            {
                _pc.RemoveChangeMonitor(this);
                _PhysCtrlrs.Remove(_pc);
            }

            base.OnDeactivate(source);
        }
        #endregion

        public MechanismMount MechanismMount
            => Anchor as MechanismMount;

        public SurfaceTriggerGroup SurfaceTriggerGroup
            => TriggerGroup as SurfaceTriggerGroup;

        public override object Clone()
            => new SurfaceTriggerTarget(SurfaceTriggerGroup);

        #region public override void CheckTriggering()
        /// <summary>Check to see if triggering should occur, and if so, call DoTrigger() for the mechanism</summary>
        public override void CheckTriggering()
        {
            var _mech = SurfaceTriggerGroup.Master?.TriggerMechanism;
            if (_mech != null)
            {
                var _loc = _mech.GetLocated()?.Locator;
                if (_loc != null)
                {
                    var _all = (from _l in _loc.MapContext.LocatorsInRegion(_loc.GeometricRegion, _loc.PlanarPresence)
                                where (_l?.ActiveMovement?.SurfacePressure ?? false)
                                && _mech.Surfaces.Contains(_l.BaseFace)
                                from _o in _l.ICoreAs<ICoreObject>()
                                select new { Loc = _l, _o.Weight }).ToList();

                    var _weight = _all.Sum(_a => _a.Weight);
                    if (_weight >= _mech.TripWeight)
                    {
                        _mech.DoTrigger(_all.Select(_a => _a.Loc).ToList());
                    }
                }
            }
        }
        #endregion

        #region public void HandleInteraction(Interaction workSet)
        public void HandleInteraction(Interaction workSet)
        {
            if (workSet?.InteractData is LocatorMove _locMove)
            {
                switch (_locMove.LocatorMoveState)
                {
                    case LocatorMoveState.TargetDeparture:
                        {
                            // NOTE: if it is leaving context, just unhook it
                            if (_locMove.Locator.ICore is ICoreObject _obj)
                            {
                                if (_PhysCtrlrs.Contains(_obj))
                                {
                                    _PhysCtrlrs.Remove(_obj);
                                }

                                _obj.RemoveChangeMonitor(this);
                            }
                        }
                        break;

                    case LocatorMoveState.TargetPassedBy:
                        {
                            var _rgn = SurfaceTriggerGroup.Master?.TriggerMechanism?.GetLocated()?.Locator.GeometricRegion;
                            if (_locMove.Locator.GeometricRegion.ContainsGeometricRegion(_rgn))
                            {
                                // hook monitors if needed
                                if (_locMove.Locator.ICore is ICoreObject _obj)
                                {
                                    if (!_PhysCtrlrs.Contains(_obj))
                                    {
                                        _PhysCtrlrs.Add(_obj);
                                    }

                                    _obj.AddChangeMonitor(this);
                                }
                                CheckTriggering();
                            }
                            else
                            {
                                // unhook monitors
                                (_locMove.Locator.ICore as ICoreObject)?.RemoveChangeMonitor(this);
                            }
                        }
                        break;

                    case LocatorMoveState.TargetArrival:
                        {
                            // start watching it for physical changes that might cause triggering
                            if (_locMove.Locator.ICore is ICoreObject _obj)
                            {
                                if (!_PhysCtrlrs.Contains(_obj))
                                {
                                    _PhysCtrlrs.Add(_obj);
                                }

                                _obj.AddChangeMonitor(this);
                            }
                            CheckTriggering();
                        }
                        break;
                }
            }
        }
        #endregion

        public IEnumerable<Type> GetInteractionTypes()
        {
            yield return typeof(LocatorMove);
            yield break;
        }

        public bool LinkBefore(Type interactType, IInteractHandler existingHandler)
        {
            if (interactType == typeof(LocatorMove))
            {
                return true;
            }

            return false;
        }

        // IMonitorChange<Physical>
        public void PreTestChange(object sender, AbortableChangeEventArgs<Physical> args)
        {
        }

        public void PreValueChanged(object sender, ChangeValueEventArgs<Physical> args)
        {
        }

        public void ValueChanged(object sender, ChangeValueEventArgs<Physical> args)
        {
            CheckTriggering();
        }
    }
}
