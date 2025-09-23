using System;


namespace MazeKeeper.Class
{
    [Serializable]
    public class Option<T>
    {
        public Option() { }


        public Option(T value)
        {
            Value = value;
        }


        public bool Enabled;
        public T    Value;
    }
}