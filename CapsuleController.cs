using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CapsuleControllerSystem
{
    [RequireComponent(typeof(CapsuleCollider), typeof(Rigidbody))]
    public class CapsuleController : MonoBehaviour
    {

        #region Fields
        [SerializeField] private float _standingHeight = 2f;
        [SerializeField] private float _crouchHeight = 1f;
        [SerializeField] private float _kneesHeight = 0.4f;
        [SerializeField] private int _raycastAmount = 8;
        [SerializeField] private float _raycastLenght = 0.5f;

        [SerializeField] private float _horizontalSpeed = 200f;
        [SerializeField] private float _rotationSpeed = 200f;
        [SerializeField] private float _jumpForce = 2.5f;
        [SerializeField] private int _maxJumpCount = 1;
        private int _currentJumpCount;

        private Vector3 _moveDirection;
        private float _rotateDirection;
        private bool _triggerJump;

        public bool allowControlInMidAir;
        public bool allowJumpInMidAir;

        private Transform _transform;
        private CapsuleCollider _capsule;
        private Rigidbody _rigidbody;
        #endregion

        #region Properties
        public bool IsGrounded { get; private set; }
        #endregion

        #region Methods
        public void Crouch()
        {
            SetCrouchHeight();
        }
        public void Stand()
        {
            SetStandingHeight();
        }
        public void MoveWorld(Vector3 direction)
        {
            _moveDirection = direction;
        }
        public void MoveLocal(Vector3 direction)
        {
            _moveDirection = _transform.TransformVector(direction);
        }
        public void Rotate(float direction)
        {
            _rotateDirection = direction;
        }
        public void Jump()
        {
            if (!_triggerJump && _currentJumpCount < _maxJumpCount && (IsGrounded || allowJumpInMidAir) )
            {
                _currentJumpCount++;
                _triggerJump = true;
            }
        }

        #region Private
        private bool TouchFloor(out float averageHeight)
        {
            float height = _kneesHeight + _capsule.radius;
            float lenght = height + _raycastLenght;
            int rayTouchCount = 0;
            float rayHeightSum = 0;

            // Center ray
            if (Physics.Raycast(_transform.TransformPoint(Vector3.up * height), Vector3.down * lenght, out RaycastHit hitCenter, lenght))
            {
                rayTouchCount++;
                rayHeightSum += hitCenter.point.y;
            }
            Debug.DrawRay(_transform.TransformPoint(Vector3.up * height), Vector3.down * lenght, Color.magenta);

            float angle = (2f * 3.14159265f)/ _raycastAmount;
            for (int i = 0; i < _raycastAmount; i++)
            {
                Vector3 point = new Vector3(Mathf.Sin(angle * i) * _capsule.radius, height, Mathf.Cos(angle * i) * _capsule.radius);
                if (Physics.Raycast(_transform.TransformPoint(point), Vector3.down * lenght, out RaycastHit hit, lenght))
                {
                    rayTouchCount++;
                    rayHeightSum += hit.point.y;
                }
                Debug.DrawRay(_transform.TransformPoint(point), Vector3.down * lenght, Color.magenta);
            }

            if (rayTouchCount == 0)
            {
                averageHeight = 0f;
                return false;
            }
            else
            {
                averageHeight = rayHeightSum / rayTouchCount;
                return true;
            }
        }
        private void SetStandingHeight()
        {
            float height = _standingHeight - _kneesHeight;
            float center = height/2f + _kneesHeight;
            _capsule.height = height;
            _capsule.center = new Vector3(0f, center, 0f);
        }
        private void SetCrouchHeight()
        {
            float height = _crouchHeight - _kneesHeight;
            float center = height / 2f + _kneesHeight;
            _capsule.height = height;
            _capsule.center = new Vector3(0f, center, 0f);
        }
        #endregion
        #endregion

        #region Unity API
    #if UNITY_EDITOR
        private void OnValidate()
        {
            if (_crouchHeight < _kneesHeight) Debug.LogWarning($"CapsuleController of {gameObject.name} have a knees value >= to crouch value. Are you crazy ???");
            if (_standingHeight < _crouchHeight) Debug.LogWarning($"CapsuleController of {gameObject.name} have a crouch value >= to standing value. Are you crazy ???");

            _capsule = GetComponent<CapsuleCollider>();
            SetStandingHeight();
        }
    #endif
        private void Awake()
        {
            _transform = GetComponent<Transform>();
            _capsule = GetComponent<CapsuleCollider>();
            _rigidbody = GetComponent<Rigidbody>();
        }
        private void FixedUpdate()
        {
            Vector3 velocity = _rigidbody.velocity;

            // Vertical mouvement
            // Jump mouvement
            if (_triggerJump)
            {
                _triggerJump = false;
                velocity.y = _jumpForce;
                _rigidbody.useGravity = true;
                IsGrounded = false;
            }
            // We seek the floor
            else if (velocity.y <= 0f && TouchFloor(out float averageHeight))
            {
                // If not falling just snap the average ground
                if (IsGrounded)
                {
                    _rigidbody.MovePosition(new Vector3(
                        _rigidbody.position.x,
                        averageHeight,
                        _rigidbody.position.z
                        ));
                    velocity.y = 0f;
                }
                // If falling and feet can touch the ground -> begging snapping
                else if (_rigidbody.position.y <= averageHeight)
                {
                    IsGrounded = true;
                    _rigidbody.MovePosition(new Vector3(
                        _rigidbody.position.x,
                        averageHeight,
                        _rigidbody.position.z
                        ));
                    velocity.y = 0f;
                    _rigidbody.useGravity = false;
                    // Reset the jump count
                    _currentJumpCount = 0;
                }
                // Else keep falling in darkness
            }
            // If no ground detected but not falling -> activate falling
            else if (IsGrounded)
            {
                IsGrounded = false;
                _rigidbody.useGravity = true;
            }

            // Horizontal mouvement
            if (IsGrounded || allowControlInMidAir)
            {
                velocity.x = _moveDirection.x * _horizontalSpeed * Time.fixedDeltaTime; // Straf
                velocity.z = _moveDirection.z * _horizontalSpeed * Time.fixedDeltaTime; // Forward
            }        

            _rigidbody.velocity = velocity;
        }
        private void Update()
        {
            _transform.Rotate(Vector3.up, _rotateDirection * _rotationSpeed * Time.deltaTime);
        }
        #endregion

    }
}