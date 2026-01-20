using System;
using PurrNet;
using UnityEngine;

namespace Game.Scripts.Client.Logic.Player
{
    public class PlayerAnimator : MonoBehaviour
    {
        [SerializeField] private NetworkAnimator _animator;
        [SerializeField] private PlayerMover _playerMover;
        private bool _isGrounded=true;
        private Vector3 _velocity=Vector3.zero;
        private Vector2 speedPercent;
        

        private void Update()
        {
  
            _isGrounded = _playerMover.Grounded;
            _velocity = _playerMover.transform.InverseTransformDirection(_playerMover.Velocity);
            _animator.SetBool("Grounded",_isGrounded);
            float horizontalSpeed = Vector3.ProjectOnPlane(
                _playerMover.Velocity,
                _playerMover.GroundNormal
            ).magnitude;
            
            if (_isGrounded)
                speedPercent = new Vector2(Mathf.Clamp(_velocity.x, -1f, 1f),
                    Mathf.Clamp(_velocity.z, -1f, 1f));
            else
                speedPercent = new Vector2(10f, 10f);
            
            _animator.SetFloat("Xaxis", speedPercent.x);
            _animator.SetFloat("Yaxis", speedPercent.y);
            _animator.SetFloat("VerticalSpeed",
                Vector3.Dot(_playerMover.Velocity, _playerMover.transform.up));
            _animator.SetFloat("HorizontalSpeed", Mathf.Clamp(horizontalSpeed, 0, 1f));
            _animator.SetBool("Jump",_playerMover.Jump);
            _animator.SetFloat("MoveSpeed",_playerMover.MoveSpeed *0.2f);
            _animator.SetBool("Slide",_playerMover.Slide);
  
        }
    }
}
