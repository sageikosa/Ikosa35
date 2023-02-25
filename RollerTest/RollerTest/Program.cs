using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Uzi.Core.Dice;

namespace RollerTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var _line = Console.ReadLine();
            while (!string.IsNullOrWhiteSpace(_line))
            {
                var _complex = new ComplexDiceRoller(_line);
                var _log = _complex.GetRollerLog();
                foreach (var _item in _log.Parts)
                    Console.WriteLine(string.Format(@"Log:{0}={1}", _item.Expression, _item.Total));
                Console.WriteLine(string.Format(@"Result={0}", _log.Total));
                _line = Console.ReadLine();
            }
        }
    }
}
