using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Uzi.Core;
using Uzi.Core.Contracts;

namespace Uzi.Ikosa.Languages
{
    [Serializable]
    public class LanguageSet : IEnumerable<Language>, IControlChange<Utterance>
    {
        #region construction
        public LanguageSet()
        {
            _LanguageSet = [];
            _UCtrl = new ChangeController<Utterance>(this, null);
            _LastUtterance = new Utterance(string.Empty, new Common(this), double.MinValue);
        }
        #endregion

        #region private data
        private Collection<Language> _LanguageSet;
        private Utterance _LastUtterance;
        private ChangeController<Utterance> _UCtrl;
        #endregion

        #region public void Add(Language lang)
        /// <summary>
        /// Positions a lanuage in the collection after languages of similar type, 
        /// or at the end of the list if there are no type-matching languages.
        /// This allows the unique list to be built with a linear pass.
        /// </summary>
        public void Add(Language lang)
        {
            // pseudo-sort next to existing languages of the same type so the unique list is easier to generate
            Type _lType = lang.GetType();
            for (var _lx = 0; _lx < _LanguageSet.Count; _lx++)
            {
                if (_LanguageSet[_lx].GetType().Equals(_lType))
                {
                    // found an existing language, add immediately after
                    _LanguageSet.Insert(_lx + 1, lang);
                    return;
                }
            }

            // add to end
            _LanguageSet.Add(lang);
        }
        #endregion

        public void Remove(Language lang)
        {
            _LanguageSet.Remove(lang);
        }

        #region public IEnumerable<Language> UniqueLanguages()
        /// <summary>Do not use for Sources, since only the first of each will be returned</summary>
        public IEnumerable<Language> UniqueLanguages()
        {
            Type _lastType = null;
            foreach (Language _lang in _LanguageSet)
            {
                Type _currType = _lang.GetType();
                if (!_currType.Equals(_lastType))
                {
                    // new type
                    yield return _lang;
                    _lastType = _currType;
                }
            }
            yield break;
        }
        #endregion

        public bool CanUnderstandLanguage(Type languageType)
        {
            foreach (Language _lang in _LanguageSet)
            {
                if (_lang.IsCompatible(languageType))
                {
                    return true;
                }
            }
            return false;
        }

        #region public bool CanUnderstandAlphabet(string alphabet)
        /// <summary>
        /// Has at least one language that uses the particular alphabet
        /// </summary>
        /// <param name="alphabet"></param>
        /// <returns></returns>
        public bool CanUnderstandAlphabet(string alphabet)
        {
            foreach (Language _lang in _LanguageSet)
            {
                if (_lang.IsAlphabetCompatible(alphabet))
                {
                    return true;
                }
            }
            return false;
        }
        #endregion

        /// <summary>
        /// Used when lanugage needs to be picked during aiming mode (e.g., language-dependent spells)
        /// </summary>
        public IEnumerable<OptionAimOption> ProjectableLanguageOptions
        {
            get
            {
                foreach (var _lang in UniqueLanguages().Where(_l => _l.CanProject))
                {
                    yield return new OptionAimValue<Language>()
                    {
                        Key = _lang.Name,
                        Description = _lang.Name,
                        Name = _lang.Name,
                        Value = _lang
                    };
                }
                yield break;
            }
        }

        #region IEnumerable<Language> Members
        public IEnumerator<Language> GetEnumerator()
        {
            return _LanguageSet.GetEnumerator();
        }
        #endregion

        #region IEnumerable Members
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _LanguageSet.GetEnumerator();
        }
        #endregion

        public Utterance LastUtterance
        {
            get { return _LastUtterance; }
            set
            {
                _LastUtterance = value;
                _UCtrl.DoValueChanged(value);
            }
        }

        #region IControlChange<Utterance> Members

        public void AddChangeMonitor(IMonitorChange<Utterance> monitor)
        {
            _UCtrl.AddChangeMonitor(monitor);
        }

        public void RemoveChangeMonitor(IMonitorChange<Utterance> monitor)
        {
            _UCtrl.RemoveChangeMonitor(monitor);
        }

        #endregion

        #region INotifyPropertyChanged Members
        [field:NonSerialized, JsonIgnore]
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        #endregion
    }
}
