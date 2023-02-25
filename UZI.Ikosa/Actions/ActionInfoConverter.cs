using Uzi.Core;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Actions
{
    public static class ActionInfoConverter
    {
        public static ActionInfo ToActionInfo(this ActionResult self, CoreActor actor)
        {
            var _action = self.Action as ActionBase;
            if (_action != null)
            {
                return _action.ToActionInfo(self.Provider, self.IsExternal, actor);
            }
            return new ActionInfo();
        }

        public static ActionInfo ToActionInfo(this ChoiceBinder self)
        {
            return self.Action.ToActionInfo(null, self.IsExternal, self.Actor);
        }
    }
}
