using Uzi.Core.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Tactical;
using System.Collections.ObjectModel;
using Uzi.Ikosa.Descriptions;
using Uzi.Ikosa.Senses;

namespace Uzi.Ikosa.Adjuncts
{
    /// <summary>
    /// Adjunct for all sorts of
    /// </summary>
    [Serializable]
    public class MagicMark : Adjunct, IVisible, IInformable, IDecipherable
    {
        #region Construction
        public MagicMark(object source, Guid seedCaster, bool isVisible, string mark)
            : base(source)
        {
            _Casters =
            [
                seedCaster
            ];
            _IsVisible = isVisible;
            _Mark = mark;
        }
        private MagicMark(object source, IEnumerable<Guid> seedCaster, bool isVisible, string mark)
            : base(source)
        {
            _Casters = new Collection<Guid>(seedCaster.ToList());
            _IsVisible = isVisible;
            _Mark = mark;
        }
        #endregion

        private Collection<Guid> _Casters;

        #region ICore Members
        private bool _IsVisible;
        public bool IsVisible => _IsVisible && ((Anchor as IVisible)?.IsVisible ?? false);
        #endregion

        #region IInformable Members
        public virtual IEnumerable<Info> Inform(CoreActor observer)
        {
            var _critter = observer as Creature;
            if (_critter.Awarenesses[ID] == AwarenessLevel.Aware)
            {
                // aware of the mark
                if (_Casters.Contains(_critter.ID))
                {
                    yield return new Description(@"Mark", Mark);
                }
                else
                {
                    yield return new Description(@"????", @"?*?*?*");
                }
            }
            yield break;
        }
        #endregion

        private string _Mark;
        public virtual string Mark => _Mark;

        /// <summary>Override to trigger when the mark is deciphered</summary>
        protected virtual CoreStep OnDecipher(CoreActivity activity)
            => null;

        #region public IStep Decipher(CoreActivity activity)
        public CoreStep Decipher(CoreActivity activity)
        {
            if (!HasDeciphered(activity.Actor.ID))
            {
                _Casters.Add(activity.Actor.ID);
                return activity.GetActivityResultNotifyStep(Inform(activity.Actor).ToArray());
            }
            return null;
        }
        #endregion

        #region public bool HasDeciphered(Guid guid)
        public bool HasDeciphered(Guid guid)
        {
            return _Casters.Contains(guid);
        }
        #endregion

        public override object Clone()
            => new MagicMark(Source, _Casters.AsEnumerable(), IsVisible, Mark);
    }
}
