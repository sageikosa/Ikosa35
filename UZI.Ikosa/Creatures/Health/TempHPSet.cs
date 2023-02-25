using System;
using System.Linq;
using System.Collections.ObjectModel;

namespace Uzi.Ikosa
{
    [Serializable]
    public class TempHPSet : ICreatureBound
    {
        #region Construction
        public TempHPSet(Creature creature)
        {
            _Critter = creature;
            _Chunks = new Collection<TempHPChunk>();
        }
        #endregion

        #region ICreatureBound Members
        private Creature _Critter;
        public Creature Creature { get { return _Critter; } }
        #endregion

        private Collection<TempHPChunk> _Chunks;

        #region public void Add(TempHPChunk newChunk)
        public void Add(TempHPChunk newChunk)
        {
            var _source = newChunk.Source;
            var _soakable = newChunk.Value;

            // look for similarly sourced existing chunks
            foreach (var _chunk in _Chunks)
            {
                if (_chunk.Source.Equals(_source))
                {
                    // soak up existing chunk values
                    if (_chunk.Value >= _soakable)
                    {
                        // this chunk is sufficient to soak up all the new stuff
                        _chunk.Value -= _soakable;
                        _soakable = 0;
                        break;
                    }
                    else //if (_chunk.Value < tempHP.Value)
                    {
                        // this chunk did soak up some, but there may still be more
                        _soakable -= _chunk.Value;
                        _chunk.Value = 0;
                    }
                }
            }

            // clean and add new
            PruneChunks();
            _Chunks.Add(newChunk);
        }
        #endregion

        #region public void Remove(int damage)
        public void Remove(int damage)
        {
            foreach (var _chunk in _Chunks.Where(_c => _c.Delta.Enabled))
            {
                if (_chunk.Value >= damage)
                {
                    _chunk.Value -= damage;
                    damage = 0;
                    break;
                }
                else
                {
                    damage -= _chunk.Value;
                    _chunk.Value = 0;
                }
            }

            // cleanup any chunks that got consumed
            PruneChunks();
        }
        #endregion

        public void Remove(TempHPChunk chunk)
        {
            if (_Chunks.Contains(chunk))
                _Chunks.Remove(chunk);
        }

        public int Total { get { return _Chunks.Where(_c => _c.Delta.Enabled).Sum(_c => _c.Value); } }

        public void PruneChunks()
        {
            for (var _cx = _Chunks.Count - 1; _cx >= 0; _cx--)
            {
                if (_Chunks[_cx].Value <= 0)
                    _Chunks.RemoveAt(_cx);
            }
        }
    }
}
