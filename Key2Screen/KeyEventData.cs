using System.Collections.Generic;
using System.Windows.Forms;

namespace Key2Screen
{
    public class KeyEventData
    {
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = (int)Modifier;
                hashCode = (hashCode * 397) ^ (int)Key;
                hashCode = (hashCode * 397) ^ Windows.GetHashCode();
                return hashCode;
            }
        }

        public Keys Modifier { get; private set; }

        public KeyEventData()
        {
        }

        protected bool Equals(KeyEventData other)
        {
            if (other == null)
            {
                return false;
            }
            return Modifier == other.Modifier && Key == other.Key && Windows.Equals(other.Windows);
        }

        private static readonly Dictionary<Keys, string> KeyToStringMapping = new Dictionary<Keys, string>
            {
                { Keys.D0, "0" },
                { Keys.D1, "1" },
                { Keys.D2, "2" },
                { Keys.D3, "3" },
                {
                    Keys.D4, "4"
                },
                {
                    Keys.D5, "5"
                },
                {
                    Keys.D6, "6"
                },
                {
                    Keys.D7, "7"
                },
                {
                    Keys.D8, "8"
                },
                {
                    Keys.D9, "9"
                },
                {
                    Keys.OemMinus, "-"
                },
                {
                    Keys.OemBackslash, ">"
                },
                {
                    Keys.OemPeriod, "."
                },
                {
                    Keys.Oemcomma, ","
                },
                {
                    Keys.OemOpenBrackets, "ß"
                },
                {
                    Keys.Oem5, "^"
                },
                {
                    Keys.Oem6, "´"
                },
                {
                    Keys.Oemplus, "+"
                },
                {
                    Keys.OemQuestion, "#"
                },
                {
                    Keys.Oem1, "Ü"
                },
                {
                    Keys.Oem7, "Ä"
                },
                {
                    Keys.Oemtilde, "Ö"
                },
                {
                    Keys.Next, "PageDown"
                }
            };

        public Keys Key { get; private set; }

        public int Repetitions { get; set; }
        public bool Ignore { get; set; }
        public bool Windows { get; private set; }

        public KeyEventData(Keys key,
            Keys modifier,
            bool windows)
        {
            Modifier = modifier;
            Key = key;
            Windows = windows;
            Repetitions = 1;
            Ignore = true;
        }

        public override string ToString()
        {
            var pressedKeyString = string.Empty;
            if ((Modifier & Keys.Control) == Keys.Control)
            {
                pressedKeyString += "Ctrl + ";
            }
            if ((Modifier & Keys.Alt) == Keys.Alt)
            {
                pressedKeyString += "Alt + ";
            }
            if ((Modifier & Keys.Shift) == Keys.Shift)
            {
                pressedKeyString += "Shift + ";
            }
            if (Windows && ((Keys.LWin & Key) != Keys.LWin) && ((Keys.RWin & Key) != Keys.RWin))
            {
                pressedKeyString += "Windows + ";
            }
            pressedKeyString += KeyToStringMapping.ContainsKey(Key) ? KeyToStringMapping[Key] : Key.ToString();
            return pressedKeyString;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            if (obj.GetType() != GetType())
            {
                return false;
            }
            return Equals((KeyEventData)obj);
        }
    }
}