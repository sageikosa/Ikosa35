using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Proxy.ViewModel
{
    public static class IdentitiesMenuViewModel
    {
        public static MenuViewModel GetContextMenu(ActorModel actor, ObjectInfo objectInfo, ICommand command)
        {
            // setup to gather identities
            var _sub = new MenuViewModel
            {
                Header = @"Recognize"
            };

            var _idents = actor.Proxies.IkosaProxy.Service.GetIdentityInfos(actor.FulfillerID.ToString(), objectInfo.ID.ToString());
            if (_idents.Count > 0)
            {
                var _any = false;
                foreach (var _id in _idents)
                {
                    _any = _any || _id.IsActive;
                    _sub.SubItems.Add(new MenuViewModel
                    {
                        Header = _id.ObjectInfo,
                        IsChecked = _id.IsActive,
                        Command = command,
                        ToolTip = @"Recognize as this identity",
                        Parameter = new Tuple<ActorModel, ObjectInfo, Guid>(actor, _id.ObjectInfo, _id.InfoID)
                    });
                }
                _sub.SubItems.Add(new MenuViewModel
                {
                    Header = new Info { Message = @"Base" },
                    IsChecked = !_any,
                    Command = command,
                    ToolTip = @"Recognize as base identity",
                    Parameter = new Tuple<ActorModel, ObjectInfo, Guid>(actor, objectInfo, Guid.Empty)
                });
            }
            return _sub;
        }
    }
}
