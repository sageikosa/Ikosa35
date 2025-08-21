using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using Uzi.Core.Contracts;

namespace Uzi.Core.Dice
{
    /// <summary>Abstract Roller Class</summary>
    [Serializable]
    public abstract class Roller
    {
        public abstract int RollValue(Guid? principalID, string title, string description, params Guid[] targets);
        public abstract RollerLog GetRollerLog();
        public string RollString { get { return ToString(); } }
        public abstract int MaxRoll { get; }

        public static int LogRollValue(string title, string description, Guid principalID, RollerLog rollerLog, Guid[] targets)
        {
            if (Roller.RollLog != null)
            {
                var _targets = targets.ToList();
                if (!_targets.Contains(Guid.Empty))
                {
                    _targets.Add(Guid.Empty);
                }

                Roller.RollLog?.Invoke(title, description, principalID, rollerLog, _targets);
            }
            return rollerLog.Total;
        }

        [field:NonSerialized, JsonIgnore]
        public static Action<string, string, Guid, RollerLog, List<Guid>> RollLog { get; set; }
    };
}
