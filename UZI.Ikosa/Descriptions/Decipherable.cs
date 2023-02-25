using System.Collections.Generic;
using System.Linq;
using Uzi.Core;

namespace Uzi.Ikosa.Descriptions
{
    public static class Decipherable
    {
        #region public static IEnumerable<IDecipherable> GetDecipherables(this ICoreObject coreObject)
        /// <summary>
        /// List all IDecipherables anchored to the object
        /// </summary>
        /// <param name="coreObject"></param>
        /// <returns></returns>
        public static IEnumerable<IDecipherable> GetDecipherables(this ICoreObject coreObject)
        {
            CoreObject _core = coreObject as CoreObject;
            if (_core != null)
                foreach (Adjunct _adj in _core.Adjuncts.Where(_a => _a is IDecipherable))
                {
                    yield return _adj as IDecipherable;
                }
            yield break;
        }
        #endregion
    }
}
