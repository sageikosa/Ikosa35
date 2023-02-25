using System.Linq;

namespace Uzi.Core
{
    public interface ICoreSettingNotify
    {
        void OnCoreSetting();
    }

    public static class CoreSettingNotifyHelper
    {
        public static void CoreSettingNotify(this ICoreObject self)
        {
            foreach (var _adj in from _a in self.Adjuncts
                                 where _a is ICoreSettingNotify
                                 select _a as ICoreSettingNotify)
            {
                _adj.OnCoreSetting();
            }
        }
    }
}
