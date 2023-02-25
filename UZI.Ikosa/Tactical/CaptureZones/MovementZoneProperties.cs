using System;

namespace Uzi.Ikosa.Tactical
{
    [Serializable]
    public abstract class MovementZoneProperties
    {
        protected MovementZoneProperties(string name, Type moveType)
        {
            _MoveType = moveType;
            _Name = name;
        }

        private Type _MoveType;
        private string _Name;

        public string Name { get { return _Name; } set { _Name = value; } }
        public Type MoveType { get { return _MoveType; } }
    }
}
