using Godot;

namespace Dungeon
{
    public struct PlayerMovementState
    {
        public MovementKeyState Forwards;
        public MovementKeyState Backwards;
        public MovementKeyState Left;
        public MovementKeyState Right;

        public MovementKeyState RotateRight;
        public MovementKeyState RotateLeft;
    }

    public struct MovementKeyState
    {
        public bool Pressed;
        public bool JustPressed;
    }

    public static class PlayerMovement
    {
        public static PlayerMovementState ReadState()
        {
            return new PlayerMovementState
            {
                Forwards = ReadKeyState(DungeonActions.Forwards),
                Backwards = ReadKeyState(DungeonActions.Backwards),
                Left = ReadKeyState(DungeonActions.Left),
                Right = ReadKeyState(DungeonActions.Right),

                RotateLeft = ReadKeyState(DungeonActions.RotateLeft),
                RotateRight = ReadKeyState(DungeonActions.RotateRight)
            };
        }

        private static MovementKeyState ReadKeyState(string action)
        {
            return new MovementKeyState
            {
                Pressed = Input.IsActionPressed(action),
                JustPressed = Input.IsActionJustPressed(action)
            };
        }
    }
}
