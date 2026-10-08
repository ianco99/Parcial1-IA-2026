using UnityEngine;
using UnityEngine.InputSystem;

namespace Parcial.View.Scene
{
	internal sealed class CameraView : ViewComponent
	{
#if MOUSE_CAMERA_CONTROLS
        private const bool MOUSE_CONTROLS = true;
#else
		private const bool MOUSE_CONTROLS = false;
#endif

		private const float MOVE_SPEED = 10.0f;
		private const float SMOOTH_TIME = 0.15f;
		private const float EDGE_SIZE = 20.0f;
		private const float ZOOM_SPEED = 20.0f;
		private const float MIN_ZOOM = 3.0f;
		private const float MAX_ZOOM = 60.0f;

		private Vector3 velocity;
		private Vector3 targetPosition;
		private float zoomVelocity;
		private float targetZoom;

		private Camera gameCamera;

		public Camera GameCamera => gameCamera;

		public override void Init(params object[] parameters)
		{
			base.Init(parameters);
			gameCamera = parameters[0] as Camera;
			GameCamera.transform.position = Vector3.back;
			targetZoom = GameCamera.orthographicSize;
		}

		public override void Tick(float deltaTime)
		{
			Keyboard keyboard = Keyboard.current;
			Mouse mouse = Mouse.current;
			bool mouseControls = MOUSE_CONTROLS && mouse != null;

			Vector3 mousePosition = mouse != null ? (Vector3)mouse.position.ReadValue() : Vector3.zero;
			Vector3 diretion = Vector3.zero;

			if ((keyboard != null && keyboard.leftArrowKey.isPressed) || (mouseControls && mousePosition.x <= EDGE_SIZE))
				diretion.x = -1.0f;
			else if ((keyboard != null && keyboard.rightArrowKey.isPressed) || (mouseControls && mousePosition.x >= (Screen.width - EDGE_SIZE)))
				diretion.x = 1.0f;

			if ((keyboard != null && keyboard.downArrowKey.isPressed) || (mouseControls && mousePosition.y <= EDGE_SIZE))
				diretion.y = -1.0f;
			else if ((keyboard != null && keyboard.upArrowKey.isPressed) || (mouseControls && mousePosition.y >= (Screen.height - EDGE_SIZE)))
				diretion.y = 1.0f;

			if (diretion.sqrMagnitude > 0.0f)
			{
				diretion.Normalize();
				targetPosition += diretion * MOVE_SPEED * deltaTime;
			}

			targetPosition.z = -1.0f;
			transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, SMOOTH_TIME);

			if (keyboard != null && keyboard.zKey.isPressed)
				targetZoom -= ZOOM_SPEED * deltaTime;
			else if (keyboard != null && keyboard.xKey.isPressed)
				targetZoom += ZOOM_SPEED * deltaTime;

			targetZoom = Mathf.Clamp(targetZoom, MIN_ZOOM, MAX_ZOOM);
			GameCamera.orthographicSize = Mathf.SmoothDamp(GameCamera.orthographicSize, targetZoom, ref zoomVelocity, SMOOTH_TIME);
		}
	}
}