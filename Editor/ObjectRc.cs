using UnityEngine;

namespace Graphlit
{
    public class ObjectRc<T> where T : Object
    {
        public ObjectRc(T obj)
        {
            _object = obj;
        }

        readonly T _object;
        int _rc = 0;
        public T Clone()
        {
            _rc++;
            return _object;
        }
        public void Drop()
        {
            _rc--;
            if (_rc <= 0 && _object)
            {
                Object.DestroyImmediate(_object);
            }
        }
    }
}