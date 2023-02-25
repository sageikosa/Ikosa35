using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Proxy.ViewModel
{
    public class CoreSelectPrerequisiteModel : PrerequisiteModel
    {
        public CoreSelectPrerequisiteModel()
        {
            _SelectCmd = new RelayCommand<AwarenessSelection>((aware) =>
            {
                var _pre = Prerequisite as CoreSelectPrerequisiteInfo;
                _pre.Selected = aware?.ID ?? Guid.Empty;
                _pre.IsReady = (_pre.Selected != null);
                PrerequisiteProxy.Proxies.IkosaProxy.Service.SetPreRequisites(new[] { _pre });
                IsSent = true;
            });
        }

        #region data
        private List<AwarenessSelection> _Infos;
        private RelayCommand<AwarenessSelection> _SelectCmd;
        #endregion

        public List<AwarenessSelection> Infos => _Infos;
        public RelayCommand<AwarenessSelection> SelectCmd => _SelectCmd;

        public void SetInfos(Guid[] ids, List<AwarenessInfo> awarenesses)
        {
            AwarenessSelection _lookup(Guid id)
            {
                if (id == Guid.Empty)
                    return new NoneAwarenessSelection
                    {
                        SelectCmd = SelectCmd
                    };
                return (from _a in awarenesses
                        where _a.ID == id
                        select new AwarenessSelection
                        {
                            Awareness = _a,
                            SelectCmd = SelectCmd
                        }).FirstOrDefault();
            }

            _Infos = (from _id in ids
                      select _lookup(_id)).ToList();
        }
    }
}
