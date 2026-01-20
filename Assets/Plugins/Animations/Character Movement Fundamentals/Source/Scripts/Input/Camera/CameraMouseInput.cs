using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CMF

{
    public struct CameraInputData
    {
        public Vector2 Look;
    }
    //This camera input class is an example of how to get input from a connected mouse using Unity's default input system;
    //It also includes an optional mouse sensitivity setting;
    public class CameraMouseInput : CameraInput
    {
        [SerializeField] private Camera _camera;

        //Invert input options;
		public bool invertHorizontalInput = false;
		public bool invertVerticalInput = false;
        public Camera Camera=>_camera;

        //Use this value to fine-tune mouse movement;
        //All mouse input will be multiplied by this value;
        public float mouseInputMultiplier = 0.01f;
        private float _horizontalInput;
        private float _verticalInput;
        

        public override float GetHorizontalCameraInput()
        {
            float val = _horizontalInput;
            _horizontalInput = 0f; 
            return val;
        }

        public override float GetVerticalCameraInput()
        {
            float val = _verticalInput;
            _verticalInput = 0f; 
            return val;
        }
        

        public void UpdateRotation(CameraInputData inputData)
        {
            _horizontalInput = CalculateHorizontal(inputData.Look.x);
            _verticalInput = CalculateVertical(inputData.Look.y);
        }

        private float CalculateHorizontal(float input)
        {
            float _input = input;
            
            //Since raw mouse input is already time-based, we need to correct for this before passing the input to the camera controller;
            if(Time.timeScale > 0f && Time.deltaTime > 0f)
            {
                _input /= Time.deltaTime;
                _input *= Time.timeScale;
            }
            else
                _input = 0f;

            //Apply mouse sensitivity;
            _input *= mouseInputMultiplier;

            //Invert input;
            if(invertHorizontalInput)
                _input *= -1f;

            return _input;
        }
        private float CalculateVertical(float input)
        {
            //Get raw mouse input;
            float _input = -input;
            
            //Since raw mouse input is already time-based, we need to correct for this before passing the input to the camera controller;
            if(Time.timeScale > 0f && Time.deltaTime > 0f)
            {
                _input /= Time.deltaTime;
                _input *= Time.timeScale;
            }
            else
                _input = 0f;

            //Apply mouse sensitivity;
            _input *= mouseInputMultiplier;

            //Invert input;
            if(invertVerticalInput)
                _input *= -1f;

            return _input;
        }
    }
}
