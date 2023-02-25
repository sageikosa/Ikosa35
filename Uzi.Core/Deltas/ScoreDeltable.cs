using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core.Contracts;

namespace Uzi.Core
{
    [Serializable]
    public class ScoreDeltable : ISupplyQualifyDelta
    {
        public ScoreDeltable(int score, IDeltable primary, string baseName)
        {
            // TODO: take 10?
            _Score = new ConstDeltable(score);
            Score.Deltas.Add(new SoftQualifiedDelta(this));
            _Primary = primary;
            _Name = baseName;
        }

        #region state
        private readonly ConstDeltable _Score;
        private readonly IDeltable _Primary;
        private readonly string _Name;
        #endregion

        public IDeltable Score => _Score;

        public IEnumerable<IDelta> QualifiedDeltas(Qualifier qualify)
            => _Primary.QualifiedDeltas(qualify, this, _Name);
    }
}
