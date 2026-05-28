using UnityEngine;

namespace BearRP.Core;

public class BearCamera : MonoBehaviour {
	[SerializeField] private int nativeWidth = 480;
	[SerializeField] private int nativeHeight = 270;

	private Camera _camera = null!;

	public void SetCamera(Camera camera) {
		_camera = camera;
	}

	public Vector2 GetPixelResolution() {
		return new Vector2(GetPixelWidth(), GetPixelHeight());
	}

	public int GetPixelWidth() {
		return nativeWidth;
	}

	public int GetPixelHeight() {
		return nativeHeight;
	}

	public int GetIntegerScale() {
		int widthScale = Mathf.FloorToInt(_camera.pixelRect.width / nativeWidth);
		int heightScale = Mathf.FloorToInt(_camera.pixelRect.height / nativeHeight);
		return Mathf.Min(widthScale, heightScale);
	}

	public bool TryGetScaledResolution(out int scaledWidth, out int scaledHeight) {
		int scale = GetIntegerScale();
		scaledWidth = GetPixelWidth() * scale;
		scaledHeight = GetPixelHeight() * scale;
		return scale > 0;
	}

	public Vector2 TransformMousePosition(Vector2 screenPixelPosition) {
		float normalizedXPosition = (screenPixelPosition.x - ((float)Screen.width / 2)) / Screen.width;
		float normalizedYPosition = (screenPixelPosition.y - ((float)Screen.height / 2)) / Screen.height;
		Vector2 position = new Vector2(normalizedXPosition, normalizedYPosition);

		int internalWidth = GetPixelWidth();
		int internalHeight = GetPixelHeight();
		float internalAspect = internalWidth / (float)internalHeight;
		float screenAspect = Screen.width / (float)Screen.height;
		if (TryGetScaledResolution(out int scaledWidth, out int scaledHeight)) {
			position.x *= Screen.width / (float)scaledWidth;
			position.y *= Screen.height / (float)scaledHeight;
		} else if (screenAspect > internalAspect) {
			position.x *= screenAspect / internalAspect;
		} else if (internalAspect > screenAspect) {
			position.y *= internalAspect / screenAspect;
		}

		screenPixelPosition = new Vector2(position.x * (float)internalWidth, position.y * (float)internalHeight);
		return screenPixelPosition;
	}

	public void ApplyCameraAspect() {
		if (nativeWidth <= 0 || nativeHeight <= 0) {
			return;
		}
		_camera.aspect = (float) nativeWidth / nativeHeight;
	}

	public void ResetCameraAspect() {
		if (_camera != null) {
			_camera.ResetAspect();
		}
	}

}