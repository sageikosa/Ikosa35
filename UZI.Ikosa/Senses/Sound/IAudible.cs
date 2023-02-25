using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Ikosa.Tactical;
using Uzi.Core;
using Uzi.Ikosa.Interactions;
using Uzi.Visualize;
using System.Diagnostics;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Uzi.Visualize.Contracts.Tactical;

namespace Uzi.Ikosa.Senses
{
    /// <summary>
    /// Defines an audible sound
    /// </summary>
    public interface IAudible
    {
        /// <summary>ID for channel isolation</summary>
        Guid SoundGroupID { get; }

        /// <summary>ID for identifying source</summary>
        Guid SourceID { get; }

        /// <summary>Reporting name for game-master log messages</summary>
        string Name { get; }

        /// <summary>Let sound figure out how to describe itself based on awareness</summary>
        SoundInfo GetSoundInfo(ISensorHost sensors, SoundAwareness awareness);

        void LostSoundInfo(ISensorHost sensors);
    }

    public static class AudibleStatics
    {
        public static bool IsSoundAudible(this IAudible sound, IGeometricContext context)
        {
            if (context != null)
            {
                var _map = context.MapContext.Map;
                var _effectLines = context.LinesFromPoint(context.GeometricRegion.GetPoint3D(),
                    new SegmentSetFactory(_map,
                        context.GeometricRegion, context.GeometricRegion,
                        ITacticalInquiryHelper.EmptyArray, SegmentSetProcess.Effect), PlanarPresence.Both);
                var _audible = _map.GetICore<ICore>(sound.SourceID);

                foreach (var _lSet in _effectLines)
                {
                    // regen a fresh sound transit for each line
                    var _sTrans = new SoundTransit(sound);
                    var _noiseInteract = new Interaction(null, _audible, null, _sTrans);

                    // carry the interaction through the environment
                    if (_lSet.CarryInteraction(_noiseInteract))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static void UpdateListenersInGroups(this ConcurrentDictionary<Guid, LocalCellGroup> updateGroups,
            Func<Guid, bool> soundFilter)
        {
            if (updateGroups?.Any() ?? false)
            {
                // all potential listeners
                // TODO: debug list groups
                var _locList = updateGroups.SelectMany(_g => _g.Value.Locators.Where(_l => _l.ICore is ISensorHost)).Distinct().ToList();
                // TODO: debug list locators
                Parallel.ForEach(_locList, _loc => _loc.RefreshSoundAwareness(soundFilter));
            }
        }
    }
}
