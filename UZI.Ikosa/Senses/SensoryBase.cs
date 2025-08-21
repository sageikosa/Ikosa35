using System;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Interactions;
using Uzi.Visualize;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Uzi.Ikosa.Senses
{
    [Serializable]
    public abstract class SensoryBase : ISourcedObject, ICreatureBind, ICore, IControlChange<Activation>
    {
        #region Construction
        protected SensoryBase(object source)
        {
            Source = source;
            _Range = double.MaxValue;
            _ActCtrl = new ChangeController<Activation>(this, new Activation(this, true));
            LowLight = false;
            Creature = null;
        }
        #endregion

        #region state
        private double _Range;
        private ChangeController<Activation> _ActCtrl;
        private object _Src;
        #endregion

        public object Source { get => _Src; private set => _Src = value; }

        #region public virtual string Name { get; }
        /// <summary>
        /// Name of the sense (with range)
        /// </summary>
        public virtual string Name
        {
            get
            {
                if (Range < double.MaxValue)
                {
                    return string.Format(@"{0} {1} feet", GetType().Name, Range);
                }
                else
                {
                    return string.Format(@"{0}", GetType().Name);
                }
            }
        }
        #endregion

        public double Range { get => _Range; set => _Range = value; }

        #region public virtual bool IsActive { get; set; }
        /// <summary>
        /// Gets or sets the active state of the sense.  
        /// The sense uses controlled activation, so changing this value may be blocked by a monitor.
        /// </summary>
        public virtual bool IsActive
        {
            get => _ActCtrl.LastValue.IsActive && (Creature != null);
            set
            {
                if (_ActCtrl.LastValue.IsActive != value)
                {
                    var _newAct = new Activation(this, value);
                    if (!_ActCtrl.WillAbortChange(_newAct))
                    {
                        _ActCtrl.DoPreValueChanged(_newAct);
                        _ActCtrl.DoValueChanged(_newAct);
                        PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(@"Active"));
                        if (Creature != null)
                        {
                            Creature.Senses.ApplySenses();
                        }
                    }
                }
            }
        }
        #endregion

        /// <summary>Some virtual senses may express themselves as a different type than themselves.</summary>
        public virtual Type ExpressedType() => GetType();

        /// <summary>Higher numbers are better</summary>
        public abstract int Precedence { get; }

        // TODO: may vary based on range
        /// <summary>Useable for targeting</summary>
        public virtual bool ForTargeting => true;

        /// <summary>Useable for terrain visualization</summary>
        public virtual bool ForTerrain => true;

        /// <summary>Sense can be blocked if no line of effect (most physical senses, except tremorsense)</summary>
        public virtual bool UsesLineOfEffect => false;

        /// <summary>Sense must transit through environment (some scrying senses do not transit)</summary>
        public virtual bool UsesSenseTransit => true;

        /// <summary>Darkvision, vision and blindsense</summary>
        public virtual bool UsesSight { get { return false; } }

        /// <summary>Vision</summary>
        public virtual bool UsesLight => false;

        /// <summary>Blindsight ignores, but not true seeing (examined by sense alterers)</summary>
        public virtual bool IgnoresConcealment => false;

        /// <summary>Blind-sight, blind-sense, see invisible and true seeing</summary>
        public virtual bool IgnoresInvisibility => false;

        /// <summary>Blindsight, true seeing and tremorsense ignores (examined by sense alterers)</summary>
        public virtual bool IgnoresVisualEffects => false;

        public virtual PlanarPresence PlanarPresence => PlanarPresence.Material;

        #region public bool WorksInLightLevel(LightLevel level)
        /// <summary>True if not using sight, or light; otherwise, depends on level shadowiness and low-light vision</summary>
        public bool WorksInLightLevel(LightRange level)
        {
            if (!UsesSight || !UsesLight)
            {
                return true;
            }

            if (level < LightRange.FarShadow)
            {
                return false;
            }

            if (level < LightRange.NearShadow)
            {
                return LowLight;
            }

            return true;
        }
        #endregion

        public bool LowLight { get; set; }

        /// <summary>Blindsight</summary>
        public virtual bool UsesHearing => false;

        #region ICreatureBind Members
        protected virtual void OnBind() { }
        protected virtual void OnPreUnbind() { }
        protected virtual void OnUnbind() { }

        public bool BindTo(Creature creature)
        {
            if (Creature == null)
            {
                Creature = creature;
                OnBind();
                return true;
            }
            return false;
        }

        public void UnbindFromCreature()
        {
            OnPreUnbind();
            Creature = null;
            OnUnbind();
        }
        #endregion

        public Creature Creature { get; private set; }

        public Guid ID => Creature.ID;

        #region IControlChange<Activation> Members
        public void AddChangeMonitor(IMonitorChange<Activation> monitor)
        {
            _ActCtrl.AddChangeMonitor(monitor);
        }

        public void RemoveChangeMonitor(IMonitorChange<Activation> monitor)
        {
            _ActCtrl.RemoveChangeMonitor(monitor);
        }
        #endregion

        #region INotifyPropertyChanged Members
        [field: NonSerialized, JsonIgnore]
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        #endregion

        #region public bool CarrySenseInteraction(LocalMap map, IGeometricRegion start, IGeometricRegion end)
        /// <summary>Propagates &quot;Active&quot; sense interactions through the environment</summary>
        public bool CarrySenseInteraction(LocalMap map, IGeometricRegion start, IGeometricRegion end,
            ITacticalInquiry[] exclusions)
        {
            // regen a fresh sense transit (in case of alterations)
            var _sTrans = new SenseTransit(this);
            var _senseSet = new Interaction(null, this, null, _sTrans);
            var _factory = new SegmentSetFactory(map, start, end, exclusions, SegmentSetProcess.Geometry);
            var _zones = map.MapContext.GetInteractionTransitZones(_senseSet).ToList();
            if (!_zones.Any())
            {
                return true;
            }

            // NOTE: centroid + all corners
            foreach (var _iSect in end.GetPoint3D().ToEnumerable().Concat(end.AllCorners()))
            {
                // NOTE: centroid + all corners
                foreach (var _start in start.GetPoint3D().ToEnumerable())
                {
                    var _line = map.SegmentCells(_start, _iSect, _factory, PlanarPresence);
                    if (_line.CarryInteraction(_senseSet, _zones))
                    {
                        // first line that can carry interaction is good
                        return true;
                    }
                }
            }

            // nothing worked!
            return false;
        }
        #endregion
    }
}
