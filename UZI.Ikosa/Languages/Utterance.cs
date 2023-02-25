using System;

namespace Uzi.Ikosa.Languages
{
    [Serializable]
    public class Utterance
    {
        public Utterance(string words, Language language, double time)
        {
            _Words = words;
            _Language = language;
            _Time = time;
        }

        #region private data
        private string _Words;
        private Language _Language;
        private double _Time;
        #endregion

        public string Words { get { return _Words; } }
        public Language Language { get { return _Language; } }
        public double Time { get { return _Time; } }
    }
}
