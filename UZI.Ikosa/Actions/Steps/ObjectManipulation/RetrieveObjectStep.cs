using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Interactions.Action;

namespace Uzi.Ikosa.Actions.Steps
{
    [Serializable]
    public class RetrieveObjectStep : CoreStep
    {
        #region construction
        public RetrieveObjectStep(CoreActivity activity, IObjectContainer repository, ICoreObject coreObject)
            : base(activity)
        {
            _Obj = coreObject;
            _Repository = repository;
        }

        public RetrieveObjectStep(CoreStep predecessor, IObjectContainer repository, ICoreObject coreObject)
            : base(predecessor)
        {
            _Obj = coreObject;
            _Repository = repository;
        }
        #endregion

        #region data
        private IObjectContainer _Repository;
        private ICoreObject _Obj;
        #endregion

        public ICoreObject ICoreObject => _Obj;
        public CoreActivity Activity => Process as CoreActivity;
        public IObjectContainer Repository => _Repository;

        protected override bool OnDoStep()
        {
            if ((Activity.Actor is Creature _critter) && (_Obj != null))
            {
                if (ManipulateTouch.CanManipulateTouch(_critter, _Obj))
                {
                    var _retrieve = new Retrieve(_critter, Repository);
                    var _interact = new Interaction(_critter, this, _Obj, _retrieve);
                    _Obj.HandleInteraction(_interact);
                    if (_interact.Feedback.OfType<ValueFeedback<bool>>().Any(_vfb => _vfb.Value))
                    {
                        EnqueueNotify(new RefreshNotify(true, false, true, true, false), Activity.Actor.ID);
                        //new RefreshNotify { Message = @"Retrieved", Items = true, Creature = true, Awarenesses = true });
                    }
                    else
                    {
                        var _infos = _interact.Feedback.OfType<InfoFeedback>().Select(_f => _f.Information);
                        AppendFollowing(Activity.GetActivityResultNotifyStep(_infos.ToArray()));
                    }
                }
                else
                {
                    AppendFollowing(Activity.GetActivityResultNotifyStep(@"Cannot touch"));
                }
            }
            else
            {
                AppendFollowing(Activity.GetActivityResultNotifyStep(@"Invalid creature or object"));
            }
            return true;
        }

        protected override StepPrerequisite OnNextPrerequisite() => null;
        public override bool IsDispensingPrerequisites => false;
    }
}
