using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Contracts.Infos;
using Uzi.Visualize;

namespace Uzi.Ikosa.Contracts
{
    [Serializable]
    [DataContract(Namespace = Statics.Namespace)]
    public class AwarenessInfo : INotifyPropertyChanged
    {
        [DataMember]
        public Guid ID { get; set; }
        [DataMember]
        public Info Info { get; set; }
        [DataMember]
        public ObservableCollection<AwarenessInfo> Items { get; set; }
        [DataMember]
        public double Distance { get; set; }
        [DataMember]
        public bool AutoMeleeHit { get; set; }
        [DataMember]
        public bool IsTargetable { get; set; }

        #region interface INotifyPropertyChanged
        [field:NonSerialized, JsonIgnore]
        public event PropertyChangedEventHandler PropertyChanged;

        protected void DoPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        #endregion

        public AwarenessInfo FindAwareness(Guid id)
        {
            if (id == ID)
                return this;
            return (from _i in Items
                    let _f = _i.FindAwareness(id)
                    where _f != null
                    select _f).FirstOrDefault();
        }

        public static AwarenessInfo FindAwareness(IEnumerable<AwarenessInfo> awarenesses, Guid id)
            => (from _a in awarenesses
                let _f = _a.FindAwareness(id)
                where _f != null
                select _f).FirstOrDefault();

        /// <summary>This any sub-items match the ID</summary>
        public bool IsAnyAware(Guid id)
            => (id == ID)
            ? true
            : Items.Any(_i => _i.IsAnyAware(id));

        public bool IsInSelection { get; set; }

        #region public void Conformulate(AwarenessInfo source)
        /// <summary>
        /// Conformulate AwarenessInfos (recursively) on matching IDs
        /// </summary>
        /// <param name="source"></param>
        public void Conformulate(AwarenessInfo source)
        {
            if (source.ID == ID)
            {
                Info = source.Info;
                DoPropertyChanged(nameof(Info));

                if (Distance != source.Distance)
                {
                    Distance = source.Distance;
                    DoPropertyChanged(nameof(Distance));
                }
                else
                {
                    Distance = source.Distance;
                }

                if (AutoMeleeHit != source.AutoMeleeHit)
                {
                    AutoMeleeHit = source.AutoMeleeHit;
                    DoPropertyChanged(nameof(AutoMeleeHit));
                }
                else
                {
                    AutoMeleeHit = source.AutoMeleeHit;
                }

                if (IsTargetable != source.IsTargetable)
                {
                    IsTargetable = source.IsTargetable;
                    DoPropertyChanged(nameof(IsTargetable));
                }
                else
                {
                    IsTargetable = source.IsTargetable;
                }

                // remove items not in source
                foreach (var _rmv in (from _ai in Items
                                      where !source.Items.Any(_s => _s.ID == _ai.ID)
                                      select _ai).ToList())
                {
                    Items.Remove(_rmv);
                }

                // update items
                foreach (var _updt in (from _ai in Items
                                       join _s in source.Items
                                       on _ai.ID equals _s.ID
                                       select new { Awareness = _ai, Source = _s }).ToList())
                {
                    _updt.Awareness.Conformulate(_updt.Source);
                }

                // add source not in items
                foreach (var _add in (from _s in source.Items
                                      where !Items.Any(_ai => _ai.ID == _s.ID)
                                      select _s).ToList())
                {
                    Items.Add(_add);
                }
            }
        }
        #endregion

        #region public void SetIconResolver(IResolveIcon resolver)
        /// <summary>Applies IResolveIcon to any ObjectInfo</summary>
        public void SetIconResolver(IResolveIcon resolver)
        {
            // set icon resolver
            if (Info is IIconInfo _iconInfo)
                _iconInfo.IconResolver = resolver;

            if (Info is CreatureObjectInfo _critter)
                _critter.SetIconResolver(resolver);

            // items
            foreach (var _item in Items)
                _item.SetIconResolver(resolver);
        }
        #endregion

        #region public bool ApplySelection(IEnumerable<Guid> selection)
        /// <summary>Applies selection by clearing or setting IsInSelection in self and Items</summary>
        public bool ApplySelection(IEnumerable<Guid> selection)
        {
            var _redraw = false;

            // self
            if (selection.Contains(ID))
            {
                if (!IsInSelection)
                    _redraw = true;
                IsInSelection = true;
            }
            else
            {
                if (IsInSelection)
                    _redraw = true;
                IsInSelection = false;
            }

            // items
            foreach (var _item in Items)
                _redraw |= _item.ApplySelection(selection);

            return _redraw;
        }
        #endregion

        #region public IEnumerable<Guid> SelectedIDs { get; }
        /// <summary>
        /// Yields own ID (if selected), and any nested selected ID
        /// </summary>
        public IEnumerable<Guid> SelectedIDs
        {
            get
            {
                // self
                if (IsInSelection)
                    yield return ID;

                // items
                foreach (var _id in from _i in Items
                                    from _si in _i.SelectedIDs
                                    select _si)
                    yield return _id;

                yield break;
            }
        }
        #endregion

        #region public IEnumerable<AwarenessInfo> SelectedItems { get; }
        /// <summary>
        /// Yields self (if selected), and any nested selected awareness
        /// </summary>
        public IEnumerable<AwarenessInfo> SelectedItems
        {
            get
            {
                // self
                if (IsInSelection)
                    yield return this;

                // items
                foreach (var _ai in from _i in Items
                                    from _si in _i.SelectedItems
                                    select _si)
                    yield return _ai;

                yield break;
            }
        }
        #endregion

        public bool HasAnySelected
        {
            get
            {
                if (IsInSelection)
                    return true;
                return Items.Any(_i => _i.HasAnySelected);
            }
        }
    }
}
