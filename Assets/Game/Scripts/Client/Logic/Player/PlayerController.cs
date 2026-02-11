using Game.Scripts.Client.Logic.Colectables;
using Game.Scripts.Client.Logic.Collectables;
using Game.Scripts.Services.Audio;
using Game.Scripts.Services.Input;
using PurrNet;
using Sisus.Init;
using UnityEngine;

namespace Game.Scripts.Client.Logic.Player
{
    public class PlayerController : NetworkBehaviour
    {
       [SerializeField] private PlayerMover _playerMover;
       [SerializeField] private Camera _camera;
       [SerializeField] private bool _useToggleToCrouch;
       [SerializeField] private SyncVar<Vector3> _startPosition = new SyncVar<Vector3>();
       private bool _spawned = false;

       private PlayerInputActions _inputActions;
       private InputService _input => Service<InputService>.Instance;

       public void SetStartPosition(Vector3 position)
       {
           _startPosition.value = position;
       }
       protected override void OnSpawned()
       {
           base.OnSpawned();
           if (isOwner)
           {
               _camera.transform.parent.gameObject.SetActive(true);
               AudioService.instance.AttachAudioListenerToObject(_camera.transform);
           }
              
           _spawned = true;
           if(!isOwner)
               return;
           LocalStart();
          
       }

       public void MoveToStartPosition()
       {
           Teleport(_startPosition.value);
       }
       
       private void LocalStart()
       {
           Cursor.lockState = CursorLockMode.Locked;
           _playerMover.enabled = true;
           _playerMover.Initialize();
           _inputActions = new PlayerInputActions();
           _inputActions.Enable();
           Teleport(_startPosition.value);
       }

       private void OnDisable()
       {
           if(isOwner)
               AudioService.instance.DetachAudioListener();
           if (_inputActions != null)
           {
               _inputActions.Disable();
               _inputActions.Dispose();
           }
           StopAllCoroutines();
       }
       
       
       private void Update()
       {
           if(!_spawned)
               return;
           if(!isOwner)
               return;
           if(_input.InputBlocked)
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
           //Debug
           
           if (_inputActions.Debug.Magnet.WasPressedThisFrame())
           {
               ExpOrbManager.MagnetAllOrbsToPlayer(transform);
           }

       }

       public void Teleport(Vector3 position)
       {
           _playerMover.SetPosition(position);
       }

    
    }
}
