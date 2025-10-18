namespace Game.Scripts.Services.Input
{
    public struct InputActionState
    {
        public bool wasPressedThisFrame; // Было нажатие в этом кадре
        public bool wasReleasedThisFrame; // Было отпускание в этом кадре
        public bool isHeld; // Удерживается
        public float delayTimer; // Таймер задержки
        public bool isDelayedPressed; // Нажато с задержкой (ожидает возврата)
        public bool isDelayedPressedPending; // Флаг: нажатие ожидает срабатывания по таймеру
        public bool isBlocked; // Заблокировано ли это действие
        public float axisValue; // Для осевых действий (например, движение)

        public InputActionState(bool isBlocked = false)
        {
            wasPressedThisFrame = false;
            wasReleasedThisFrame = false;
            isHeld = false;
            delayTimer = 0f;
            isDelayedPressedPending = false; 
            isDelayedPressed = false;
            this.isBlocked = isBlocked;
            axisValue = 0f;
        }
    }
}