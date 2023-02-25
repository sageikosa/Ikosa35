using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Proxy.ViewModel
{
    public class AbilityVM : ViewModelBase
    {
        public AbilityVM(ActorModel actor, AbilityInfo ability)
        {
            _Ability = ability;
            _Take10 = new Take10VM(actor, ability, ability.Take10);
        }

        #region data
        private AbilityInfo _Ability;
        private Take10VM _Take10;
        #endregion

        public AbilityInfo Ability => _Ability;
        public Take10VM Take10 => _Take10;

        /// <summary>
        /// Synchronizes ability info and take 10
        /// </summary>
        /// <param name="ability"></param>
        public void Conformulate(AbilityInfo ability)
        {
            // ability update
            _Ability = ability;
            DoPropertyChanged(nameof(Ability));

            // take 10 conform
            _Take10.Conformulate(ability.Take10);
        }
    }
}
