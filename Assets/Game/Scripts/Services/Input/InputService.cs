using System;
using System.Collections.Generic;
using Sisus.Init;
using UnityEngine;

namespace Game.Scripts.Services.Input
{
    // Перечисление возможных действий

    // Структура для хранения данных об одном состоянии действия

    [Service]
    public class InputService : IService
    {
        private bool _playerInputBlocked = false;
        public bool PlayerInputBlocked => _playerInputBlocked;

        private Dictionary<InputAction, InputActionState> _actionStates = new Dictionary<InputAction, InputActionState>();
        private Dictionary<InputAction, KeyCode> _keyBindings = new Dictionary<InputAction, KeyCode>();
        private Dictionary<InputAction, float> _actionDelays = new Dictionary<InputAction, float>();

        public void LocalAwake()
        {
            _keyBindings[InputAction.Escape] = KeyCode.Escape;
            _keyBindings[InputAction.Jump] = KeyCode.Space;
            _keyBindings[InputAction.Action] = KeyCode.E;
            _keyBindings[InputAction.MoveLeft] = KeyCode.A;
            _keyBindings[InputAction.MoveRight] = KeyCode.D;
            _keyBindings[InputAction.MoveUp] = KeyCode.W;
            _keyBindings[InputAction.MoveDown] = KeyCode.S;

            _actionDelays[InputAction.Escape] = 0.02f; // 20 мс
            _actionDelays[InputAction.Jump] = 0f;      // Без задержки
            _actionDelays[InputAction.Action] = 0f;
            // и т.д.

            foreach (InputAction action in Enum.GetValues(typeof(InputAction)))
            {
                _actionStates[action] = new InputActionState();
            }
        }

        public void LocalStart()
        {
        }

        public void LocalUpdate(float deltaTime)
        {
            foreach (var pair in _keyBindings)
            {
                InputAction action = pair.Key;
                KeyCode key = pair.Value;

                var state = _actionStates[action];

                if (state.isBlocked || PlayerInputBlocked)
                {
                    // Если заблокировано, сбрасываем только временные флаги
                    state.wasPressedThisFrame = false;
                    state.wasReleasedThisFrame = false;
                    _actionStates[action] = state;
                    continue;
                }

                bool isCurrentlyPressed = UnityEngine.Input.GetKey(key);

                // Обновляем isHeld и флаги нажатия/отпускания
                bool wasHeldLastFrame = state.isHeld;
                state.isHeld = isCurrentlyPressed;
                state.wasPressedThisFrame = !wasHeldLastFrame && isCurrentlyPressed;
                state.wasReleasedThisFrame = wasHeldLastFrame && !isCurrentlyPressed;

                // Обработка задержки
                float delay = _actionDelays.TryGetValue(action, out float d) ? d : 0f;

                if (state.delayTimer > 0f)
                {
                    // Таймер активен, уменьшаем его
                    state.delayTimer -= deltaTime;
                    if (state.delayTimer <= 0f)
                    {
                        state.delayTimer = 0f;
                        // Таймер истёк, если было нажатие, которое ждало, устанавливаем флаг
                        if (state.isDelayedPressedPending)
                        {
                            state.isDelayedPressedPending = false; // Сбрасываем ожидание
                            state.isDelayedPressed = true;         // Устанавливаем флаг для возврата
                        }
                    }
                }
                else
                {
                    // Таймер не активен
                    if (state.wasPressedThisFrame)
                    {
                        if (delay > 0f)
                        {
                            // Если есть задержка, запускаем таймер и устанавливаем флаг ожидания
                            state.delayTimer = delay;
                            state.isDelayedPressedPending = true;
                            state.isDelayedPressed = false; // Сбрасываем флаг возврата, если был
                        }
                        else
                        {
                            // Если задержки нет, сразу устанавливаем флаг возврата
                            state.isDelayedPressed = true;
                            state.isDelayedPressedPending = false; // Убедимся, что флаг ожидания сброшен
                        }
                    }
                    else
                    {
                        // Если не было нажатия и таймер не активен, сбрасываем флаг ожидания
                        state.isDelayedPressedPending = false;
                    }
                }

                // Сбрасываем флаг нажатия этого кадра, так как он используется только в этом кадре
                state.wasPressedThisFrame = false;
                state.wasReleasedThisFrame = false;

                _actionStates[action] = state;
            }
        }

        // Метод для проверки нажатия с задержкой
        public bool WasActionPressed(InputAction action)
        {
            if (_actionStates.TryGetValue(action, out InputActionState state))
            {
                if (state.isDelayedPressed)
                {
                    // Сбрасываем флаг после чтения, чтобы вернуть true только один раз
                    state.isDelayedPressed = false;
                    _actionStates[action] = state;
                    return true;
                }
            }
            return false;
        }

        // Метод для проверки удержания (без задержки)
        public bool IsActionHeld(InputAction action)
        {
            if (_actionStates.TryGetValue(action, out InputActionState state))
            {
                return state.isHeld;
            }
            return false;
        }

        // Метод для проверки отпускания (без задержки)
        public bool WasActionReleased(InputAction action)
        {
            if (_actionStates.TryGetValue(action, out InputActionState state))
            {
                if (state.wasReleasedThisFrame)
                {
                    // Этот флаг сбрасывается в следующем Update
                    return true;
                }
            }
            return false;
        }

        public void BlockPlayerInput()
        {
            _playerInputBlocked = true;
        }
        public void BlockPlayerMovementInput()
        {
           BlockAction(InputAction.Jump);
           BlockAction(InputAction.MoveLeft);
           BlockAction(InputAction.MoveRight);
           BlockAction(InputAction.MoveUp);
           BlockAction(InputAction.MoveDown);
        }
        public void UnlockPlayerMovementInput()
        {
            UnblockAction(InputAction.Jump);
            UnblockAction(InputAction.MoveLeft);
            UnblockAction(InputAction.MoveRight);
            UnblockAction(InputAction.MoveUp);
            UnblockAction(InputAction.MoveDown);
        }

        public void UnlockPlayerInput()
        {
            _playerInputBlocked = false;
        }

        public void BlockAction(InputAction action)
        {
            if (_actionStates.TryGetValue(action, out InputActionState state))
            {
                state.isBlocked = true;
                _actionStates[action] = state;
            }
        }

        public void UnblockAction(InputAction action)
        {
            if (_actionStates.TryGetValue(action, out InputActionState state))
            {
                state.isBlocked = false;
                _actionStates[action] = state;
            }
        }

        public void RebindKey(InputAction action, KeyCode newKey)
        {
            _keyBindings[action] = newKey;
        }
    
    }
}