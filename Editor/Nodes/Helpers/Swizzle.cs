using System.Linq;
using UnityEngine.UIElements;

namespace z3y.ShaderGraph
{
    public static class Swizzle
    {
        private static char[] _validCharacters = new char[] { 'x', 'y', 'z', 'w'};
        private static char[] _validCharacters2 = new char[] { 'r', 'g', 'b', 'a' };


        public static bool IsValid(string value)
        {
            if (value.Length > 4)
            {
                return false;
            }

            if (value.Length < 1)
            {
                return false;
            }

            if (_validCharacters.Contains(value[0]) && value.Any(x => !_validCharacters.Contains(x)))
            {
                return false;
            }

            if (_validCharacters2.Contains(value[0]) && value.Any(x => !_validCharacters2.Contains(x)))
            {
                return false;
            }


            return true;
        }

        public static string ValidateSwizzle(ChangeEvent<string> evt, TextField textField)
        {
            var newValue = evt.newValue;
            if (IsValid(newValue))
            {
                return newValue;
            }

            var previousValue = evt.previousValue;
            textField.SetValueWithoutNotify(previousValue);
            return previousValue;
        }

    }
}