using System;
using System.Collections.Generic;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Proxy.ViewModel
{
    public class Take10VM : ViewModelBase
    {
        #region construction
        public Take10VM(ActorModel actor, Take10VMType type, Take10Info take10)
        {
            if (type != Take10VMType.Deltable)
            {
                _Type = type;
            }
            else
            {
                throw new ArgumentException(@"Cannot set type=Deltable");
            }
            _Actor = actor;
            _Deltable = null;
            _Take10 = take10;
        }

        public Take10VM(ActorModel actor, DeltableInfo deltable, Take10Info take10)
        {
            _Type = Take10VMType.Deltable;
            _Actor = actor;
            _Deltable = deltable;
            _Take10 = take10;
        }
        #endregion

        #region data
        private ActorModel _Actor;
        private Take10VMType _Type;
        private DeltableInfo _Deltable;
        private Take10Info _Take10;
        #endregion

        public string Remaining
            => (_Take10 != null)
            ? $@"Take 10 valid for {_Take10.RemainingRounds} more rounds"
            : @"Take 10 not active";

        #region public IEnumerable<string> Options { get; }
        public IEnumerable<string> Options
        {
            get
            {
                yield return @"Roll";
                if (_Take10 != null)
                {
                    yield return @"=10=";
                }
                yield return @"Take 10: 10 rounds";
                yield return @"Take 10: 100 rounds";
                yield return @"Take 10: 600 rounds";
                yield return @"Take 10: 4800 rounds";
                yield break;
            }
        }
        #endregion

        #region public string SelectedOption { get; set; }
        public string SelectedOption
        {
            get
            {
                return (_Take10 != null)
                    ? @"=10="
                    : @"Roll";
            }
            set
            {
                // update service if different
                if (value != SelectedOption)
                {
                    if (value != @"=10=")
                    {
                        var _duration = (value != @"Roll") ? int.Parse(value.Split(' ')[2]) : 0;
                        switch (_Type)
                        {
                            case Take10VMType.SkillBase:
                                _Actor.Proxies.IkosaProxy.Service.SetSkillsTake10(_Actor.CreatureLoginInfo.ID.ToString(), _duration);
                                break;

                            case Take10VMType.AbilityBase:
                                _Actor.Proxies.IkosaProxy.Service.SetAbilitiesTake10(_Actor.CreatureLoginInfo.ID.ToString(), _duration);
                                break;

                            case Take10VMType.Deltable:
                            default:
                                _Actor.Proxies.IkosaProxy.Service.SetTake10(_Actor.CreatureLoginInfo.ID.ToString(), _Deltable, _duration);
                                break;
                        }
                    }
                }
            }
        }
        #endregion

        /// <summary>
        /// Synchronizes remaining rounds and notifies changes if needed
        /// </summary>
        /// <param name="take10"></param>
        public void Conformulate(Take10Info take10)
        {
            // Options only change if take10 switches ON/OFF
            var _updt = ((take10 == null) && (_Take10 != null))
                || ((take10 != null) && (_Take10 == null));

            if ((take10?.RemainingRounds ?? 0) != (_Take10?.RemainingRounds ?? 0))
            {
                _Take10 = take10;
                DoPropertyChanged(nameof(Remaining));
                DoPropertyChanged(nameof(SelectedOption));
            }
            else
            {
                _Take10 = take10;
            }

            if (_updt)
            {
                DoPropertyChanged(nameof(Options));
            }
        }
    }

    public enum Take10VMType
    {
        Deltable,
        SkillBase,
        AbilityBase
    }
}
