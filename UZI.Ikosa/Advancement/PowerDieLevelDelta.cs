using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;

namespace Uzi.Ikosa.Advancement
{
    /// <summary>Provides count of "boosts" by power die number</summary>
    [Serializable]
    public class PowerDieTrackDelta : IQualifyDelta, ISourcedObject
    {
        #region construction
        /// <summary>Provides count of "boosts" by power die number</summary>
        public PowerDieTrackDelta(AdvancementClass advClass, string description)
        {
            _Term = new TerminateController(this);
            _Class = advClass;
            _PowerDice = new Stack<int>();
            _Descr = description;
        }
        #endregion

        #region data
        private readonly TerminateController _Term;
        private AdvancementClass _Class;
        private Stack<int> _PowerDice;
        private string _Descr;
        #endregion

        public void Push(int pdLevel) { _PowerDice.Push(pdLevel); }
        public int Pop() { return _PowerDice.Pop(); }

        #region IQualifyDelta Members

        public IEnumerable<IDelta> QualifiedDeltas(Qualifier qualify)
        {
            if (qualify is Interaction _iAct)
            {
                // if doing a power level check, use requested value
                if (_iAct.InteractData is PowerLevelCheck _check)
                {
                    var _val = _PowerDice.Count(_d => _d <= _check.PowerLevel);
                    if (_val > 0)
                    {
                        yield return new QualifyingDelta(_val, Source, _Descr);
                        yield break;
                    }
                }
            }

            // otherwise use the natural count
            yield return new QualifyingDelta(_PowerDice.Count, Source, _Descr);
            yield break;
        }

        #endregion

        #region IControlTerminate Members
        public void DoTerminate() { _Term.DoTerminate(); }
        public void AddTerminateDependent(IDependOnTerminate subscriber)
        {
            _Term.AddTerminateDependent(subscriber);
        }

        public void RemoveTerminateDependent(IDependOnTerminate subscriber)
        {
            _Term.RemoveTerminateDependent(subscriber);
        }

        public int TerminateSubscriberCount => _Term.TerminateSubscriberCount;
        #endregion

        public object Source => _Class;
        public AdvancementClass AdvancementClass => _Class;
    }

    public class PowerLevelCheck : InteractData
    {
        public PowerLevelCheck(int powerLevel)
            : base(null)
        {
            PowerLevel = powerLevel;
        }

        public int PowerLevel { get; set; }

        /// <summary>Creates an interaction to test for a power die level</summary>
        public static Interaction LevelCheck(int powerDie)
        {
            return new Interaction(null, null, null, new PowerLevelCheck(powerDie));
        }

    }
}