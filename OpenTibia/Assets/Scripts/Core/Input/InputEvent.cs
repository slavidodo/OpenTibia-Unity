namespace OpenTibiaUnity.Core.Input
{
    class InputEvent
    {
        public const uint KeyUp = 1;
        public const uint KeyDown = 2;
        public const uint KeyRepeat = 4;
        public const uint KeyAny = KeyUp | KeyDown | KeyRepeat;

        public const uint TextInput = 8;
        public const uint TextAny = TextInput;

    }
}
