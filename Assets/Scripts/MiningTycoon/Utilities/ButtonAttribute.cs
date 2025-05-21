using System;

namespace MiningTycoon.Utilities
{
    public class ButtonAttribute : Attribute
    {
        public string ButtonMame { get; private set; }

        public ButtonAttribute(string buttonName = "")
        {
            ButtonMame = buttonName;
        }
    }
}