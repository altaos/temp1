using System;

namespace AliveChessLibrary.GameObjects.Characters
{
    public class UnitFacrory
    {
        private GameData _data;

        public UnitFacrory(GameData data)
        {
            this._data = data;
        }

        public Unit Create(Guid guid, int id, int count, UnitType type)
        {
            Unit unit = new Unit();
            unit.UnitCount = count;
            unit.UnitType = type;
            return unit;
        }
    }
}
