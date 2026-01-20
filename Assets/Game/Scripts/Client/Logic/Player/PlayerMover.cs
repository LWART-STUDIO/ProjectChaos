using KinematicCharacterController;
using UnityEngine;

namespace Game.Scripts.Client.Logic.Player
{
    public enum CrouchInput
    {
        None = 0,
        Toggle = 1,
        Hold = 2,
        UnHold = 3,
    }

    public struct CharacterState
    {
        public bool Grounded;
        public Stance Stance;
        public Vector3 Velocity;
    }

    public enum Stance
    {
        Stand = 0,
        Crouch = 1,
        Slide = 2,
    }

    public struct CharacterInput
    {
        public Vector3 LookDirection;
        public Vector2 Move;
        public bool Jump;
        public bool JumpSustain;
        public CrouchInput Crouch;
    }

    public class PlayerMover : MonoBehaviour, ICharacterController
    {
        [SerializeField] private KinematicCharacterMotor _motor;

        [Space] [SerializeField] private float _walkSpeed = 20f;
        [SerializeField] private float _walkAcceleration = 25f;
        [SerializeField] private float _rotationSmoothTime = 0.15f;
        [SerializeField] private float _crouchSpeed = 7f;
        [SerializeField] private float _crouchAcceleration = 20f;
        [SerializeField] private float _slideGravity = -21f;

        [Space] [SerializeField] private float _airSpeed = 3f;
        [SerializeField] private float _airAcceleration = 15f;

        [Space] [SerializeField] private float _jumpSpeed = 70f;
        [SerializeField] private float _coyoteTime = 0.2f;
        [Range(0, 1f)] [SerializeField] private float _jumpSustainGravity = 0.4f;
        [SerializeField] private float _gravity = -90f;

        [Space] [SerializeField] private float _slideStartSpeed = 25f;
        [SerializeField] private float _slideEndSpeed = 15f;
        [SerializeField] private float _slideFriction = 0.8f;
        [SerializeField] private float _slideSteerAcceleration = 5f;

        [Space] [SerializeField] private float _standHeight = 2f;
        [SerializeField] private float _crouchHeight = 2f;

        private CharacterState _state;
        private CharacterState _lastState;
        private CharacterState _tempState;


        private Vector3 _requestedMovement;
        private bool _requestedJump;
        private bool _requestedSustainJump;
        private bool _requestedCrouch;
        private bool _requestedCrouchInAir;

        private float _timeSinceUngrounded;
        private float _timeSinceJumpRequest;
        private bool _ungroundDueToJump;


        private Collider[] _uncrouchOverlapResults;

        //Animator need data
        public bool Grounded => _state.Grounded;
        public Vector3 Velocity => _motor.Velocity;
        public Vector3 GroundNormal => _motor.GroundingStatus.GroundNormal;
        public bool Jump;
        public bool Slide => _state.Stance is Stance.Slide;
        public bool Crouch => _state.Stance is Stance.Crouch;
        public float MoveSpeed => _walkSpeed;

        public void Initialize()
        {
            _motor.enabled = true;
            _state.Stance = Stance.Stand;
            _lastState = _state;
            _uncrouchOverlapResults = new Collider[8];
            _motor.CharacterController = this;
        }

        public void UpdateInput(CharacterInput input)
        {
            _requestedMovement = new Vector3(input.Move.x, 0, input.Move.y);
            _requestedMovement = Vector3.ClampMagnitude(_requestedMovement, 1f);

            _requestedMovement =
                Quaternion.LookRotation(
                    Vector3.ProjectOnPlane(input.LookDirection, _motor.CharacterUp),
                    _motor.CharacterUp
                ) * _requestedMovement;
            
            var wasRequestingJump = _requestedJump;
            _requestedJump = _requestedJump || input.Jump;
            if (_requestedJump && !wasRequestingJump)
                _timeSinceJumpRequest = 0f;
            _requestedSustainJump = input.JumpSustain;
            var wasRequestedCrouch = _requestedCrouch;
            _requestedCrouch = input.Crouch switch
            {
                CrouchInput.Toggle => !_requestedCrouch,
                CrouchInput.None => _requestedCrouch,
                CrouchInput.Hold => true,
                CrouchInput.UnHold => false,
                _ => _requestedCrouch
            };
            if (_requestedCrouch && !wasRequestedCrouch)
                _requestedCrouchInAir = !_state.Grounded;
            if (!_requestedCrouch && wasRequestedCrouch)
                _requestedCrouchInAir = false;
        }

        public void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
        {
            // Используем направление движения для поворота персонажа, а не взгляд
            if (_requestedMovement.sqrMagnitude < 0.0001f)
                return;

            Vector3 flatMoveDirection = Vector3.ProjectOnPlane(_requestedMovement, _motor.CharacterUp);
            if (flatMoveDirection.sqrMagnitude < 0.0001f)
                return;

            Quaternion targetRotation = Quaternion.LookRotation(flatMoveDirection, _motor.CharacterUp);

            float t = 1f - Mathf.Exp(-deltaTime / _rotationSmoothTime);

            currentRotation = Quaternion.Slerp(currentRotation, targetRotation, t);
        }

        public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
        {
            //OnGround
            if (_motor.GroundingStatus.IsStableOnGround)
            {
                _timeSinceUngrounded = 0f;
                _ungroundDueToJump = false;
                
                var groundedMovement = _motor.GetDirectionTangentToSurface
                (
                    direction: _requestedMovement,
                    surfaceNormal: _motor.GroundingStatus.GroundNormal
                ) * _requestedMovement.magnitude;
                //StartSliding
                {
                    var moving = groundedMovement.sqrMagnitude > 0f;
                    var crouching = _state.Stance is Stance.Crouch;
                    var wasStanding = _lastState.Stance is Stance.Stand;
                    var wasInAir = !_lastState.Grounded;
                    if (moving && crouching && (wasStanding || wasInAir))
                    {
                        _state.Stance = Stance.Slide;

                        if (wasInAir)
                        {
                            currentVelocity = Vector3.ProjectOnPlane
                            (
                                vector: _lastState.Velocity,
                                planeNormal: _motor.GroundingStatus.GroundNormal
                            );
                        }


                        var effectiveSlideStartSpeed = _slideStartSpeed;
                        if (!_lastState.Grounded && !_requestedCrouchInAir)
                        {
                            effectiveSlideStartSpeed = 0f;
                            _requestedCrouchInAir = false;
                        }
                        var slideSpeed = Mathf.Max(effectiveSlideStartSpeed, currentVelocity.magnitude);
                        currentVelocity = _motor.GetDirectionTangentToSurface
                        (
                            direction: currentVelocity,
                            surfaceNormal: _motor.GroundingStatus.GroundNormal
                        ) * slideSpeed;
                    }
                }

                if (_state.Stance is Stance.Stand or Stance.Crouch)
                {
                    var speed = _state.Stance is Stance.Stand ? _walkSpeed : _crouchSpeed;
                    var acceleration = _state.Stance is Stance.Crouch ? _walkAcceleration : _crouchAcceleration;
                    var targetVelocity = groundedMovement * speed;
                    currentVelocity = Vector3.Lerp(currentVelocity, targetVelocity,
                        1f - Mathf.Exp(-acceleration * deltaTime));
                }
                //Continue sliding
                else
                {
                    //Friction
                    currentVelocity -= currentVelocity * (_slideFriction * deltaTime);

                    //Slope
                    {
                        var force = Vector3.ProjectOnPlane
                        (
                            -_motor.CharacterUp,
                            _motor.GroundingStatus.GroundNormal
                        ) * _slideGravity;
                        currentVelocity -= force * deltaTime;
                    }

                    //Steer
                    {
                        var currentSpeed = currentVelocity.magnitude;
                        var targetVelocity = groundedMovement * currentSpeed;
                        var steerForce = (targetVelocity - currentVelocity) * _slideSteerAcceleration * deltaTime;

                        currentVelocity += steerForce;
                        currentVelocity = Vector3.ClampMagnitude(currentVelocity, currentSpeed);
                    }
                    //Stop
                    if (currentVelocity.sqrMagnitude < _slideEndSpeed)
                        _state.Stance = Stance.Crouch;
                }
            }
            //InAir
            else
            {
                _timeSinceUngrounded += deltaTime;
                //Move
                if (_requestedMovement.sqrMagnitude > 0f)
                {
                    var planarMovement = Vector3.ProjectOnPlane
                    (
                        vector: _requestedMovement,
                        planeNormal: _motor.CharacterUp
                    ) * _requestedMovement.magnitude;

                    var currentPlanarVelocity = Vector3.ProjectOnPlane
                    (
                        vector: currentVelocity,
                        planeNormal: _motor.CharacterUp
                    );

                    var movementForce = planarMovement * _airAcceleration * deltaTime;
                    if (currentPlanarVelocity.magnitude < _airSpeed)
                    {
                        var targetVelocity = currentPlanarVelocity + movementForce;
                        targetVelocity = Vector3.ClampMagnitude(targetVelocity, _airSpeed);
                        movementForce = targetVelocity - currentPlanarVelocity;
                    }
                    else if (Vector3.Dot(currentPlanarVelocity, movementForce) > 0f)
                    {
                        var constrainedMovementForce = Vector3.ProjectOnPlane
                        (
                            vector: currentPlanarVelocity,
                            planeNormal: currentPlanarVelocity.normalized
                        );
                        movementForce = constrainedMovementForce;
                    }

                    //Prevent air-climbing on slopes
                    if (_motor.GroundingStatus.FoundAnyGround)
                    {
                        if (Vector3.Dot(movementForce, currentVelocity + movementForce) > 0f)
                        {
                            var obstructionNormal = Vector3.Cross
                            (
                                _motor.CharacterUp,
                                Vector3.Cross
                                (
                                    _motor.CharacterUp,
                                    _motor.GroundingStatus.GroundNormal
                                )
                            ).normalized;

                            movementForce = Vector3.ProjectOnPlane(movementForce, obstructionNormal);
                        }
                    }

                    currentVelocity += movementForce;
                }

                //Gravity
                var effectiveGravity = _gravity;
                var verticalSpeed = Vector3.Dot(currentVelocity, _motor.CharacterUp);
                if (_requestedSustainJump && verticalSpeed > 0f)
                    effectiveGravity *= _jumpSustainGravity;
                currentVelocity += _motor.CharacterUp * effectiveGravity * deltaTime;
            }

            if (_requestedJump)
            {
                var grounded = _motor.GroundingStatus.IsStableOnGround;
                var canCoyoteJump = _timeSinceJumpRequest<_coyoteTime && !_ungroundDueToJump;
                if (grounded||canCoyoteJump)
                {
                    Jump = true;
                    _requestedJump = false;
                    _requestedCrouch = false;
                    _requestedCrouchInAir =false;
                
                    _motor.ForceUnground(time: 0f);
                    _ungroundDueToJump = true;

                    var currentVelocitySpeed = Vector3.Dot(currentVelocity, _motor.CharacterUp);
                    var targetVelocity = Mathf.Max(currentVelocitySpeed, _jumpSpeed);
                    currentVelocity.y += _motor.CharacterUp.y * (targetVelocity - currentVelocitySpeed);

                }
                else
                {
                    _timeSinceJumpRequest += deltaTime;
                    var canJumpLater =_timeSinceJumpRequest<_coyoteTime;
                    _requestedJump = canJumpLater;
                    Jump = false;
                }
                
                
            }
        }

        public void BeforeCharacterUpdate(float deltaTime)
        {
            _tempState = _state;
            //Crouch
            if (_requestedCrouch && _state.Stance == Stance.Stand)
            {
                _state.Stance = Stance.Crouch;
                _motor.SetCapsuleDimensions
                (
                    radius: _motor.Capsule.radius,
                    height: _crouchHeight,
                    yOffset: _crouchHeight * 0.5f
                );
            }
        }

        public void PostGroundingUpdate(float deltaTime)
        {
            Jump = false;
            if (!_motor.GroundingStatus.IsStableOnGround && _state.Stance is Stance.Slide)
                _state.Stance = Stance.Crouch;
        }

        public void AfterCharacterUpdate(float deltaTime)
        {
            //Uncrouch
            if (!_requestedCrouch && _state.Stance is not Stance.Stand)
            {
                _motor.SetCapsuleDimensions
                (
                    radius: _motor.Capsule.radius,
                    height: _standHeight,
                    yOffset: _standHeight * 0.5f
                );

                //ChekWorldCollisionUp
                Vector3 pos = _motor.TransientPosition;
                Quaternion rot = _motor.TransientRotation;
                LayerMask mask = _motor.CollidableLayers;
                if (_motor.CharacterOverlap(pos, rot, _uncrouchOverlapResults,
                        mask, QueryTriggerInteraction.Ignore) > 0)
                {
                    //Re-crouch
                    _requestedCrouch = true;
                    _motor.SetCapsuleDimensions
                    (
                        radius: _motor.Capsule.radius,
                        height: _crouchHeight,
                        yOffset: _crouchHeight * 0.5f
                    );
                }
                else
                {
                    _state.Stance = Stance.Stand;
                }
            }

            _state.Grounded = _motor.GroundingStatus.IsStableOnGround;
            _state.Velocity = _motor.Velocity;
            _lastState = _tempState;
        }

        public bool IsColliderValidForCollisions(Collider coll)
        {
            return true;
        }

        public void OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint,
            ref HitStabilityReport hitStabilityReport)
        {
        }

        public void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint,
            ref HitStabilityReport hitStabilityReport)
        {
        }

        public void ProcessHitStabilityReport(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint,
            Vector3 atCharacterPosition,
            Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport)
        {
        }

        public void OnDiscreteCollisionDetected(Collider hitCollider)
        {
        }

        public void SetPosition(Vector3 position, bool killVelocity = true)
        {
            _motor.SetPosition(position);
            if (killVelocity)
                _motor.BaseVelocity = Vector3.zero;
        }
    }
}