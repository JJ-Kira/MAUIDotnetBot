// This script controls the camera's behavior, allowing for mouse or touch input
// to orbit around a target and reset its position.

using Evergine.Common.Input;
using Evergine.Common.Input.Mouse;
using Evergine.Common.Input.Pointer;
using Evergine.Framework;
using Evergine.Framework.Graphics;
using Evergine.Framework.Services;
using Evergine.Mathematics;
using System;
using System.Linq;

namespace TestDotnetBot.Behaviors
{
    public class CameraBehavior : Behavior
    {
        /// <summary>
        /// The camera to move, bound to the Transform3D component of the entity.
        /// </summary>
        [BindComponent(false)]
        public Transform3D Transform = null;

        /// <summary>
        /// The camera transform used for movement and rotation.
        /// </summary>
        private Transform3D cameraTransform;

        /// <summary>
        /// Indicates whether the camera is being dragged or rotated.
        /// </summary>
        private bool isRotating;

        /// <summary>
        /// The scale factor for orbiting (affects the sensitivity of rotation).
        /// </summary>
        private const float OrbitScale = 0.005f;

        /// <summary>
        /// Sensitivity of touch input (affects rotation speed for touch controls).
        /// </summary>
        public float TouchSensibility { get; set; } = 0.5f;

        /// <summary>
        /// Current angle of rotation around the Y-axis (horizontal orbit).
        /// </summary>
        private float theta;

        /// <summary>
        /// Flag to indicate when the camera's transform needs updating.
        /// </summary>
        private bool isDirty;

        /// <summary>
        /// The Camera3D component associated with the camera.
        /// </summary>
        [BindComponent(source: BindComponentSource.ChildrenSkipOwner)]
        private Camera3D camera3D;

        /// <summary>
        /// GraphicsPresenter is used to handle display and rendering.
        /// </summary>
        [BindService]
        private GraphicsPresenter graphicsPresenter;

        /// <summary>
        /// Current display the camera is rendering to.
        /// </summary>
        private Display display;

        /// <summary>
        /// Initial position of the camera for resetting purposes.
        /// </summary>
        private Vector3 cameraInitialPosition;

        /// <summary>
        /// Dispatchers to handle mouse and touch input.
        /// </summary>
        private MouseDispatcher mouseDispatcher;
        private PointerDispatcher touchDispatcher;

        /// <summary>
        /// Stores the current and last mouse/touch positions for calculating deltas.
        /// </summary>
        private Evergine.Mathematics.Point currentMouseState;
        private Vector2 lastMousePosition;
        private Evergine.Mathematics.Point currentTouchState;
        private Vector2 lastTouchPosition;

        public CameraBehavior() { }

        /// <summary>
        /// Called when the behavior is loaded into the scene. Resets variables.
        /// </summary>
        protected override void OnLoaded()
        {
            base.OnLoaded();

            this.theta = 0; // Initialize rotation angle
            this.isRotating = false; // Camera is not being dragged initially
            this.isDirty = true; // Mark the transform as needing an update
        }

        /// <summary>
        /// Called when the behavior is attached to an entity. Sets up the camera transform.
        /// </summary>
        protected override bool OnAttached()
        {
            // Find the Transform3D component of the first child entity (assumes the camera is a child).
            this.cameraTransform = this.Owner.ChildEntities.First().FindComponent<Transform3D>();
            this.cameraInitialPosition = this.cameraTransform.LocalPosition; // Save the initial position for resets
            return base.OnAttached();
        }

        /// <summary>
        /// Called when the behavior is activated. Prepares the display and input handlers.
        /// </summary>
        protected override void OnActivated()
        {
            base.OnActivated();
            this.RefreshDisplay(); // Initialize the display and input dispatchers
        }

        /// <summary>
        /// Refreshes the current display and retrieves input dispatchers for mouse and touch input.
        /// </summary>
        private void RefreshDisplay()
        {
            this.display = this.camera3D.Display;
            if (this.display != null)
            {
                this.mouseDispatcher = this.display.MouseDispatcher;
                this.touchDispatcher = this.display.TouchDispatcher;
            }
        }

        /// <summary>
        /// Updates the behavior each frame. Handles input and updates the camera transform.
        /// </summary>
        protected override void Update(TimeSpan gameTime)
        {
            // Check if the display has changed (e.g., multi-display setups)
            this.graphicsPresenter.TryGetDisplay("DefaultDisplay", out var presenterDisplay);
            if (presenterDisplay != this.display)
            {
                this.camera3D.DisplayTagDirty = true;
                this.RefreshDisplay();
            }

            // Process input (mouse or touch)
            this.HandleInput();

            // Update the camera transform if changes have been made
            if (this.isDirty)
            {
                this.CommitChanges();
                this.isDirty = false; // Reset the dirty flag
            }
        }

        /// <summary>
        /// Determines whether to handle mouse or touch input based on the platform.
        /// </summary>
        private void HandleInput()
        {
            if (Evergine.Platform.DeviceInfo.PlatformType == Evergine.Common.PlatformType.Windows)
            {
                this.HandleMouse();
            }
            else
            {
                this.HandleTouch();
            }
        }

        /// <summary>
        /// Handles mouse input for orbiting the camera.
        /// </summary>
        private void HandleMouse()
        {
            if (this.mouseDispatcher == null)
            {
                return; // No mouse dispatcher available
            }

            if (this.mouseDispatcher.IsButtonDown(MouseButtons.Left))
            {
                this.currentMouseState = this.mouseDispatcher.Position;

                if (!this.isRotating)
                {
                    this.isRotating = true; // Start dragging
                }
                else
                {
                    // Calculate delta (change in mouse position)
                    Vector2 delta = Vector2.Zero;
                    delta.X = -this.currentMouseState.X + this.lastMousePosition.X;
                    delta.Y = this.currentMouseState.Y - this.lastMousePosition.Y;

                    this.Orbit(delta * OrbitScale); // Apply rotation based on delta
                }

                // Update the last mouse position
                this.lastMousePosition.X = this.currentMouseState.X;
                this.lastMousePosition.Y = this.currentMouseState.Y;
            }
            else
            {
                this.isRotating = false; // Stop dragging
            }
        }

        /// <summary>
        /// Handles touch input for orbiting the camera.
        /// </summary>
        private void HandleTouch()
        {
            if (this.touchDispatcher == null)
            {
                return; // No touch dispatcher available
            }

            var point = this.touchDispatcher.Points.FirstOrDefault(); // Get the first touch point
            if (point == null)
            {
                return;
            }

            if (point.State == ButtonState.Pressed)
            {
                this.currentTouchState = point.Position;

                if (!this.isRotating)
                {
                    this.isRotating = true; // Start dragging
                }
                else
                {
                    // Calculate delta (change in touch position)
                    Vector2 delta = Vector2.Zero;
                    delta.X = -this.currentTouchState.X + this.lastTouchPosition.X;
                    delta.Y = this.currentTouchState.Y - this.lastTouchPosition.Y;

                    this.Orbit(delta * OrbitScale); // Apply rotation based on delta
                }

                // Update the last touch position
                this.lastTouchPosition.X = this.currentTouchState.X;
                this.lastTouchPosition.Y = this.currentTouchState.Y;
            }
            else
            {
                this.isRotating = false; // Stop dragging
            }
        }

        /// <summary>
        /// Rotates the camera based on the delta value.
        /// </summary>
        public void Orbit(Vector2 delta)
        {
            this.theta += delta.X; // Adjust the rotation angle based on horizontal movement
            this.isDirty = true;  // Mark the transform as needing an update
        }

        /// <summary>
        /// Applies the calculated rotation to the camera transform.
        /// </summary>
        public void CommitChanges()
        {
            var rotation = this.Transform.LocalRotation;
            rotation.Y = -this.theta; // Apply horizontal rotation
            this.Transform.LocalRotation = rotation;
        }

        /// <summary>
        /// Resets the camera to its initial position and orientation.
        /// </summary>
        public void Reset()
        {
            this.cameraTransform.LocalPosition = this.cameraInitialPosition; // Reset position
            this.cameraTransform.LocalLookAt(Vector3.Zero, Vector3.Up); // Reset look direction
            this.Transform.LocalPosition = Vector3.Zero;
            this.Transform.LocalRotation = Vector3.Zero;

            this.theta = 0; // Reset rotation
            this.isRotating = false; // Stop dragging
        }

        public void RotateLeft()
        {
            this.theta -= 0.1f; // Adjust sensitivity if needed
            this.isDirty = true;
        }

        public void RotateRight()
        {
            this.theta += 0.1f; // Adjust sensitivity if needed
            this.isDirty = true;
        }
    }
}
