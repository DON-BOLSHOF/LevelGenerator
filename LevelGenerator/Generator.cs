using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace LevelGenerator
{
    internal class Generator
    {
        private static List<List<Field>> _field;
        private static Vector2 _heroPosition;
        private static Vector2 _currentPosition;
        private static int _currentCardAmount;
        private static readonly int _maxCardAmount = 10;

        private static float CurrentSpawnPerCent => (1 - (float)_currentCardAmount / _maxCardAmount) * 100;
        private static float _branchPerCent; //Чтобы по одной ветке не шел

        private static Vector2 _tableSize;

        private static void Main(string[] args)
        {
            _field = Enumerable.Range(0, 3)
                .Select(f => Enumerable.Range(0, 5).Select(fa => new Field(new Vector2(f, fa))).ToList()).ToList();
            _tableSize =
                new Vector2(_field.Count,
                    _field[0].Count); //В матрице координаты YX, а не XY!!! не обращай внимание на встроенный XY в Vector2 

            OutPutList();
            Console.WriteLine();

            PositionBuilder();

            OutPutList();
            Console.WriteLine();

            Console.WriteLine(_heroPosition.ToString());
            _currentPosition = _heroPosition;

            SpawnLevel();

            OutPutList();
            Console.WriteLine();

            while (true)
            {
                string s = Console.ReadLine();
                if (s.Contains("Reload"))
                {
                    ReloadLevel();
                    OutPutList();
                    Console.WriteLine();
                }
            }
        }

        private static void ReloadLevel()
        {
            _currentCardAmount = 0;
            _field.ForEach(delegate(List<Field> list) { list.ForEach(delegate(Field field)
            {
                field.SetDefaultState(); }); });
            _currentPosition = _heroPosition = new Vector2((int)_tableSize.X / 2, (int) _tableSize.Y/2);
            _field[(int)_currentPosition.X][(int)_currentPosition.Y].Visit(State.HeroPosition);
            SpawnLevel();
        }

        private static void SpawnLevel()
        {
            while (CurrentSpawnPerCent > 0)
            {
                var around = AroundPlace(_currentPosition);

                if (_currentPosition == _heroPosition && around == AroundPosition.None)
                    break;

                if ((around & AroundPosition.Left) != 0)
                {
                    if (FieldStep(_currentPosition + new Vector2(0, -1)))
                    {
                        continue;
                    }
                }

                if ((around & AroundPosition.Top) != 0)
                {
                    if (FieldStep(_currentPosition + new Vector2(-1, 0)))
                    {
                        continue;
                    }
                }

                if ((around & AroundPosition.Right) != 0)
                {
                    if (FieldStep(_currentPosition + new Vector2(0, 1)))
                    {
                        continue;
                    }
                }

                if ((around & AroundPosition.Bottom) != 0)
                {
                    if (FieldStep(_currentPosition + new Vector2(1, 0)))
                    {
                        continue;
                    }
                }

                if (_field[(int)_currentPosition.X][(int)_currentPosition.Y].CurrentState != State.HeroPosition)
                    _field[(int)_currentPosition.X][(int)_currentPosition.Y].Visit(State.EndJourneyPosition);

                _currentPosition = _heroPosition;
                _branchPerCent = 0f;
            }
        }

        private static bool FieldStep(Vector2 pos)
        {
            if (Probability(CurrentSpawnPerCent - _branchPerCent))
            {
                _field[(int)pos.X][(int)pos.Y].Visit(State.CardPosition);
                _currentCardAmount++;
                _branchPerCent += 5;
                _currentPosition = pos;
                return true;
            }

            _field[(int)pos.X][(int)pos.Y].Visit(State.Checked);
            return false;
        }

        private static AroundPosition AroundPlace(Vector2 pos)
        {
            AroundPosition result = AroundPosition.None;

            var leftPos = IsInTable(new Vector2(pos.X, pos.Y - 1)) &&
                          _field[(int)(pos.X)][(int)pos.Y - 1].CurrentState == State.UnChecked;
            if (leftPos)
                result |= AroundPosition.Left;

            var topPos = IsInTable(new Vector2(pos.X - 1, pos.Y)) && //Y сверху вниз идет
                         _field[(int)(pos.X - 1)][(int)pos.Y].CurrentState == State.UnChecked;
            if (topPos)
                result |= AroundPosition.Top;

            var rightPos = IsInTable(new Vector2(pos.X, pos.Y + 1)) &&
                           _field[(int)(pos.X)][(int)pos.Y + 1].CurrentState == State.UnChecked;
            if (rightPos)
                result |= AroundPosition.Right;

            var bottomPos = IsInTable(new Vector2(pos.X + 1, pos.Y)) &&
                            _field[(int)(pos.X + 1)][(int)pos.Y].CurrentState == State.UnChecked;
            if (bottomPos)
                result |= AroundPosition.Bottom;

            if (result > 0)
                result -= AroundPosition.None;

            return result;
        }

        [Flags]
        private enum AroundPosition
        {
            None = 0,
            Left = 1,
            Top = 2,
            Right = 4,
            Bottom = 8
        }

        private static bool IsInTable(Vector2 pos)
        {
            if (pos.X >= _tableSize.X || pos.X < 0)
                return false;

            if (pos.Y >= _tableSize.Y || pos.Y < 0)
                return false;

            return true;
        }

        private static void OutPutList()
        {
            _field.ForEach(delegate(List<Field> list)
            {
                list.ForEach(delegate(Field field) { Console.Write($"{field.Value}\t"); });
                Console.WriteLine();
            });
        }

        private static void PositionBuilder()
        {
            int iterator = 0;
            int threshold = (_field.Count + _field[0].Count - 2) * 2;
            while (iterator < threshold)
            {
                for (int x = 0; x < _field[0].Count; x++)
                {
                    if (Probability(iterator * 100 / threshold))
                    {
                        _field[0][x].Visit(State.HeroPosition);
                        _heroPosition = _field[0][x].Position;
                        return;
                    }

                    iterator++;
                }

                for (int y = 1; y < _field.Count; y++)
                {
                    if (Probability(iterator * 100 / threshold))
                    {
                        _field[y][_field[0].Count - 1].Visit(State.HeroPosition);
                        _heroPosition = _field[y][_field[0].Count - 1].Position;
                        return;
                    }

                    iterator++;
                }

                for (int x = _field[0].Count - 2; x >= 0; x--)
                {
                    if (Probability(iterator * 100 / threshold))
                    {
                        _field[^1][x].Visit(State.HeroPosition);
                        _heroPosition = _field[_field.Count - 1][x].Position;
                        return;
                    }

                    iterator++;
                }

                for (int y = _field.Count - 2; y > 0; y--)
                {
                    if (Probability(iterator * 100 / threshold))
                    {
                        _field[y][0].Visit(State.HeroPosition);
                        _heroPosition = _field[y][0].Position;
                        return;
                    }

                    iterator++;
                }
            }
        }

        private static bool Probability(float percent)
        {
            Random rand = new Random((int)DateTime.Now.Ticks);
            int res = rand.Next(101);
            if (res <= percent) return true;

            return false;
        }
    }
}