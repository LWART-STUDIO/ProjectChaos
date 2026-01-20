using CMF;
using PurrNet;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Scripts.Client.Logic.Player
{
    public class PlayerController : NetworkBehaviour
    {
       [SerializeField] private PlayerMover _playerMover;
       [SerializeField] private Camera _camera;
       [SerializeField] private bool _useToggleToCrouch;
       private bool _spawned = false;

       private PlayerInputActions _inputActions;

       protected override void OnSpawned()
       {
           base.OnSpawned();
           if (isOwner)
               _camera.transform.parent.gameObject.SetActive(true);
           _spawned = true;
           if(!isOwner)
               return;
           LocalStart();
          
       }
       
       private void LocalStart()
       {
           Cursor.lockState = CursorLockMode.Locked;
           _playerMover.Initialize();
           _inputActions = new PlayerInputActions();
           _inputActions.Enable();

           
       }

       private void OnDestroy()
       {
           _inputActions.Dispose();
       }

       private void Update()
       {
           if(!_spawned)
               return;
           if(!isOwner)
               return;
           PlayerInputActions.GameplayActions input = _inputActions.Gameplay;
           CrouchInput crouch =CrouchInput.None;
           if (input.Crouch.WasPressedThisFrame())
               crouch = _useToggleToCrouch ? CrouchInput.Toggle : CrouchInput.Hold;
           else if(input.Crouch.WasReleasedThisFrame())
           {
               if(!_useToggleToCrouch)
                crouch = CrouchInput.UnHold;
           }
           else
               crouch = CrouchInput.None;

           var characterInput = new CharacterInput
           {
               LookDirection = _camera.transform.forward,
               Move = input.Move.ReadValue<Vector2>(),
               Jump =  input.Jump.WasPressedThisFrame(),
               JumpSustain = input.Jump.IsPressed(),
               Crouch =  crouch,
                   
           };
           _playerMover.UpdateInput(characterInput);
       }

       public void Teleport(Vector3 position)
       {
           _playerMover.SetPosition(position);
       }

    
    }
}
