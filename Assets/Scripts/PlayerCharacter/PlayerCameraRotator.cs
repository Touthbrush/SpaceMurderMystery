using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCameraRotator : MonoBehaviour
{
	public enum RotationAxis { None = 0, Cam_XY = 1, Cam_X = 2, Cam_Y = 3 }

	[Header("Speed Settings")]
	//[SerializeField]
	//RotationAxis rotationAxes = RotationAxis.Cam_Y;
	[SerializeField]
	float baseCameraSpeed = 4;
	[SerializeField]
	[Range(0, 2)]
	float horCameraSpeed = 1;
	[SerializeField]
	[Range(0, 2)]
	float vertCameraSpeed = 1;

	[Header("Clamping Settings")]
	[SerializeField]
	RotationAxis rotationClamp = RotationAxis.Cam_Y;
	[SerializeField]
	[Range(0, 90)]
	float upperVerticalClamp = 60;
	[SerializeField]
	[Range(-90, 0)]
	float lowerVerticalClamp = -60;
	[SerializeField]
	[Range(0, 360)]
	float upperHorizontalClamp = 360;
	[SerializeField]
	[Range(-360, 0)]
	float lowerHorizontalClamp = -360;

	[Header("Smoothing Settings")]
	[SerializeField]
	[Tooltip("Enable to smoothen out rotational camera movement")]
	bool cameraSmoothingEnabled = true;
	[SerializeField]
	[Tooltip("!Testing Value!\nNumber of prior frames to store rotation data for, which are averaged to create smooth movement.")]
	[Range(5, 20)]
	int smoothingFrames = 20;
	[SerializeField]
	[Tooltip("Sets smoothing strength, 0 is no smoothing and 1 is full smoothing.")]
	[Range(0, 1)]
	float smoothingStrength = 0.5f;

	//Functionality variables
	List<float> rotArrayX = new List<float>();
	List<float> rotArrayY = new List<float>();
	
	float averageRotX = 0;
	float averageRotY = 0;
	float rotationX = 0;
	float rotationY = 0;

	public void GetCameraRotations(Vector3 inputXY, out Quaternion xRotation, out Quaternion yRotation)
	{
		averageRotX = 0;
		averageRotY = 0;

		rotationX += inputXY.x * (baseCameraSpeed * horCameraSpeed);
		rotationY += inputXY.y * (baseCameraSpeed * vertCameraSpeed);

		if (rotationClamp == RotationAxis.Cam_X || rotationClamp == RotationAxis.Cam_XY)
		{
			rotationX = ClampAngle(rotationX, lowerHorizontalClamp, upperHorizontalClamp);
		}
		if (rotationClamp == RotationAxis.Cam_Y || rotationClamp == RotationAxis.Cam_XY)
		{
			rotationY = ClampAngle(rotationY, lowerVerticalClamp, upperVerticalClamp);
		}

		if (cameraSmoothingEnabled)
		{
			rotArrayX.Add(rotationX);
			rotArrayY.Add(rotationY);

			if (rotArrayX.Count > smoothingFrames) { rotArrayX.RemoveAt(0); }
			if (rotArrayY.Count > smoothingFrames) { rotArrayY.RemoveAt(0); }

			foreach (float rot in rotArrayX) { averageRotX += rot; }
			foreach (float rot in rotArrayY) { averageRotY += rot; }

			averageRotX /= rotArrayX.Count;
			averageRotY /= rotArrayY.Count;

			averageRotX = Mathf.Lerp(rotationX, averageRotX, smoothingStrength);
			averageRotY = Mathf.Lerp(rotationY, averageRotY, smoothingStrength);
		}
		else
		{
			averageRotX = rotationX;
			averageRotY = rotationY;
		}

		//Clamp angles between upper/lower values
		if (rotationClamp == RotationAxis.Cam_X || rotationClamp == RotationAxis.Cam_XY)
		{
			averageRotX = ClampAngle(averageRotX, lowerHorizontalClamp, upperHorizontalClamp);
		}
		if (rotationClamp == RotationAxis.Cam_Y || rotationClamp == RotationAxis.Cam_XY)
		{
			averageRotY = ClampAngle(averageRotY, lowerVerticalClamp, upperVerticalClamp);
		}

		//Set quaternion references  
		xRotation = Quaternion.AngleAxis(averageRotX, Vector3.up);
		yRotation = Quaternion.AngleAxis(averageRotY, Vector3.left);
	}

	public static float ClampAngle(float angle, float min, float max)
	{
		angle = angle % 360;
		if ((angle >= -360f) && (angle <= 360f))
		{
			if (angle < -360f) { angle += 360f; }
			if (angle > 360f) { angle -= 360f; }
		}

		return Mathf.Clamp(angle, min, max);
	}

}
