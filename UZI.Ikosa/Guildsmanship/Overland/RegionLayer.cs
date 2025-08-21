using System;
using System.Collections.Generic;
using System.Linq;

namespace Uzi.Ikosa.Guildsmanship.Overland
{
    [Serializable]
    public class RegionLayer<LayerData>
    {
        private byte[][] _Cells;
        private Region _Region;
        private readonly Func<int, int, byte, LayerData> _Resolver;
        private readonly Func<int, int, byte, bool> _Validator;

        public RegionLayer(Region region, Func<int, int, byte, LayerData> resolver, Func<int, int, byte, bool> validator)
        {
            _Region = region;
            _Cells = GetRegionCellArray();
            _Resolver = resolver;
            _Validator = validator;
        }

        public LayerData Resolve(int row, int col, byte index) => _Resolver(row, col, index);

        private byte[][] GetRegionCellArray()
        {
            var _cells = new byte[_Region.Rows][];
            for (var _rx = 0; _rx < _Region.Rows; _rx++)
            {
                _cells[_rx] = new byte[_Region.Columns];
            }
            return _cells;
        }

        public void RefreshArray()
        {
            var _newCells = GetRegionCellArray();

            // copy
            for (var _rx = 0; _rx < Math.Min(_Region.Rows, _Cells.GetLength(0)); _rx++)
            {
                for (var _cx = 0; _cx < Math.Min(_Region.Columns, _Cells[0].GetLength(0)); _cx++)
                {
                    _newCells[_rx][_cx] = _Cells[_rx][_cx];
                }

            }

            // set new cells
            _Cells = _newCells;
        }

        public LayerData this[int row, int col]
            => _Resolver(row, col, _Cells[row][col]);

        public void SetIndex(int row, int col, byte index)
        {
            if (_Validator(row, col, index))
            {
                _Cells[row][col] = index;
            }
        }

        public byte GetIndex(int row, int column)
            => _Cells[row][column];

        public IEnumerable<LayerData> LayerDataSteam
        {
            get
            {
                for (var _rx = 0; _rx < _Region.Rows; _rx++)
                {
                    for (var _cx = 0; _cx < _Region.Columns; _cx++)
                    {
                        yield return _Resolver(_rx, _cx, _Cells[_rx][_cx]);
                    }
                }
            }
        }

        public IEnumerable<byte> IndexDataStream
            => from _row in _Cells
               from _cell in _row
               select _cell;

        /// <summary>Fill a rectangle with an index value.</summary>
        public void PaintRectangle(int top, int left, int height, int width, byte idx)
        {
            for (var _rx = top; _rx < top + height; _rx++)
            {
                for (var _cx = left; _cx < left + width; _cx++)
                {
                    if (_Validator(_rx, _cx, idx))
                    {
                        _Cells[_rx][_cx] = idx;
                    }
                }
            }
        }

        /// <summary>Replace all instances of one index value with another within a rectangle.</summary>
        public void ReplaceInRectangle(int top, int left, int height, int width, byte oldIdx, byte newIdx)
        {
            for (var _rx = top; _rx < top + height; _rx++)
            {
                for (var _cx = left; _cx < left + width; _cx++)
                {
                    if (_Validator(_rx, _cx, newIdx))
                    {
                        if (_Cells[_rx][_cx] == oldIdx)
                        {
                            _Cells[_rx][_cx] = newIdx;
                        }
                    }
                }
            }
        }
    }
}
