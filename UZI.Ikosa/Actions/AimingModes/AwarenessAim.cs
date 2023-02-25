using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Contracts;
using Uzi.Core.Contracts;
using Uzi.Visualize;

namespace Uzi.Ikosa.Actions
{
    /// <summary>
    /// Aim at anything of which the caster is aware (and in range), 
    /// target types will be checked after targets are selected, but before action is performed.
    /// Yields AwarenessTargets
    /// </summary>
    [Serializable]
    public class AwarenessAim : RangedAim, IEnumerable<ITargetType>
    {
        #region Construction
        /// <summary>
        /// Aim at anything of which the caster is aware (and in range), 
        /// target types will be checked after targets are selected, but before action is performed.
        /// Yields AwarenessTargets
        /// </summary>
        public AwarenessAim(string key, string displayName, Range minModes, Range maxModes, Range range, ITargetType validTargetTypes)
            : base(key, displayName, minModes, maxModes, range)
        {
            ValidTargetTypes = new ITargetType[] { validTargetTypes };
            AllowDuplicates = false;
        }

        /// <summary>
        /// Aim at anything of which the caster is aware (and in range), 
        /// target types will be checked after targets are selected, but before action is performed.
        /// Yields AwarenessTargets
        /// </summary>
        public AwarenessAim(string key, string displayName, Range minModes, Range maxModes, Range range, IEnumerable<ITargetType> validTargetTypes)
            : base(key, displayName, minModes, maxModes, range)
        {
            ValidTargetTypes = validTargetTypes;
            AllowDuplicates = false;
        }
        #endregion

        public IEnumerable<ITargetType> ValidTargetTypes { get; private set; }

        public bool AllowDuplicates { get; set; }

        #region IEnumerable<TargetType> Members
        public IEnumerator<ITargetType> GetEnumerator()
        {
            foreach (var _tType in ValidTargetTypes)
            {
                yield return _tType;
            }
        }
        #endregion

        #region IEnumerable Members
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            foreach (var _tType in ValidTargetTypes)
            {
                yield return _tType;
            }
        }
        #endregion

        #region public override IEnumerable<AimTarget> GetTargets(CoreActor actor, CoreAction action, Core.Services.AimTargetInfo[] infos, IInteractProvider provider)
        public override IEnumerable<AimTarget> GetTargets(CoreActor actor, CoreAction action, AimTargetInfo[] infos, IInteractProvider provider)
        {
            var _aLoc = actor?.GetLocated()?.Locator;
            if ((_aLoc != null) && (actor is Creature _critter))
            {
                foreach (var _aware in SelectedTargets<AwarenessTargetInfo>(actor, action, infos))
                {
                    // get the actual target and make sure aware of it
                    var _interact = provider.GetIInteract(_aware.TargetID ?? Guid.Empty);
                    if ((_interact != null) && (_critter.Awarenesses[_aware.TargetID ?? Guid.Empty] > Senses.AwarenessLevel.UnAware))
                    {
                        // get its locator (hopefully adjunctable and located)
                        var _geom = (_interact as IAdjunctable)?.GetLocated()?.Locator?.GeometricRegion;
                        if (_geom != null)
                        {
                            // must be within max range
                            var _max = Range.EffectiveRange(actor, action.CoreActionClassLevel(actor, this));

                            // TODO: consider scaling max-multi-line by power/range

                            // find at least one effect line
                            var _presence = _aLoc.PlanarPresence;
                            // TODO: consider transdimensional effects...force effects also...
                            // TODO: action with Descriptor => [Force]/[TransDimensional]
                            var _line = _aLoc.LinesToTarget(_geom,
                                new SegmentSetFactory(_aLoc.Map, _aLoc.GeometricRegion, _geom,
                                ITacticalInquiryHelper.GetITacticals(_interact, actor).ToArray(),
                                SegmentSetProcess.Effect), 400, _presence)
                                .FirstOrDefault(_ls => _ls.IsLineOfEffect && _ls.Vector.Length <= _max)
                                // if line of effect doesn't succeed, try a straight vector...
                                ?? _aLoc.LinesToTarget(_geom,
                                new SegmentSetFactory(_aLoc.Map, _aLoc.GeometricRegion, _geom,
                                ITacticalInquiryHelper.GetITacticals(_interact, actor).ToArray(),
                                SegmentSetProcess.Effect), 400, _presence)
                                .FirstOrDefault(_ls => _ls.Vector.Length <= _max);

                            if (_line != null)
                            {
                                // return target
                                yield return new AwarenessTarget(Key, _interact, _line);
                            }
                        }
                    }
                }
            }
            yield break;
        }
        #endregion

        public override AimingModeInfo ToAimingModeInfo(CoreAction action, CoreActor actor)
        {
            var _info = ToRangedAimInfo<AwarenessAimInfo>(action, actor);
            _info.AllowDuplicates = AllowDuplicates;
            _info.ValidTargetTypes = ValidTargetTypes.Select(_vtt => _vtt.ToTargetTypeInfo()).ToArray();
            return _info;
        }
    }
}