using System;
using System.Collections.ObjectModel;
using Uzi.Core.Contracts;

namespace Uzi.Core
{
    /// <summary>Allows actor to fulfill aiming mode prerequisites for steps.</summary>
    [Serializable]
    public class AimTargetPrerequisite<ModeType, TargetType> : StepPrerequisite
        where ModeType : AimingMode
        where TargetType : AimTarget
    {
        #region construction
        public AimTargetPrerequisite(ModeType aimMode, Qualifier workSet, string key, string name, bool serial)
            : base(aimMode, workSet, key, name)
        {
            _AimTargets = [];
            _IsSerial = serial;
        }

        public AimTargetPrerequisite(ModeType aimMode, CoreActor actor, object iSource, IInteract interactTarget, string key, string name, bool serial)
            : base(aimMode, actor, iSource, interactTarget, key, name)
        {
            _AimTargets = [];
            _IsSerial = serial;
        }
        #endregion

        #region private data
        private Collection<TargetType> _AimTargets;
        private bool _IsSerial;
        #endregion

        public ModeType AimingMode { get { return Source as ModeType; } }
        public Collection<TargetType> AimingTargets { get { return _AimTargets; } }
        public override bool IsSerial { get { return _IsSerial; } }

        public override bool IsReady { get { return _AimTargets.Count >= 1; } } // TODO: base this on MinRange...
        public override bool FailsProcess { get { return false; } }

        public override CoreActor Fulfiller
        {
            get { return Qualification.Actor; }
        }

        public override Contracts.PrerequisiteInfo ToPrerequisiteInfo(CoreStep step)
        {
            var _info = ToInfo<AimTargetPrerequisiteInfo>(step);
            // TODO: conversion of mode and target
            return _info;
        }

        public override void MergeFrom(Contracts.PrerequisiteInfo info)
        {
            // TODO:
            throw new NotImplementedException();
        }
    }
}
