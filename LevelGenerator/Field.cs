using System.Numerics;

namespace LevelGenerator
{
    public class Field
    {
        public Field(Vector2 pos)
        {
            _pos = pos;
        }
        
        private int _id;
        public int Id => _id;

        public int Value => (int)_state;

        private Vector2 _pos;
        public Vector2 Position => _pos;

        private State _state = State.UnChecked;

        public State CurrentState => _state;

        public void SetDefaultState()
        {
            _state = State.UnChecked;
        }

        public void Visit(State state)
        {
            _state = state;
        }
    }

    public enum State
    {
        UnChecked,
        Checked,
        CardPosition,
        HeroPosition,
        EndJourneyPosition
    }
}