using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

//This editor script displays the inspector GUI of the mover components;
//It also updates the collider dimensions whenever a value is changed in the inspector;
[CustomEditor(typeof(Mover))]
public class MoverInspector : Editor {

	private Mover mover;

	private string[] physicsLayers;

	private Vector3[] raycastArrayPositions;

	void Start()
	{
		Setup();
	}

	void Reset()
	{
		Setup();
	}

	void OnEnable()
	{
		Setup();
	}

	void Setup()
	{
		mover = (Mover)target;

		List<string> _layers = new List<string>();

		for(int i = 0; i < 32; i ++)
		{
			_layers.Add(LayerMask.LayerToName(i));
		}

		physicsLayers = _layers.ToArray();

		raycastArrayPositions = Sensor.GetRaycastStartPositions(mover.sensorArrayRows, mover.sensorArrayRayCount, mover.sensorArrayRowsAreOffset, 1f);
	}

	public override void OnInspectorGUI()
	{
		if(Application.isPlaying)
		{
			GUILayout.Label("Mover settings can't be changed  in the inspector while the game is running.", EditorStyles.centeredGreyMiniLabel);
			return;
		}
			
		if(mover == null)
		{
			Setup();
			return;
		}

		GUILayout.Label("Mover Options", EditorStyles.boldLabel);

		Rect _space;

		EditorGUI.BeginChangeCheck();

		mover.stepHeightRatio = EditorGUILayout.Slider("Step Height Ratio", mover.stepHeightRatio, 0f, 1f);

		GUILayout.Label("Collider Options", EditorStyles.boldLabel);

		mover.colliderHeight = EditorGUILayout.FloatField("Collider Height", mover.colliderHeight);
		mover.colliderThickness = EditorGUILayout.FloatField("Collider Thickness",mover.colliderThickness);
		mover.colliderOffset = EditorGUILayout.Vector3Field("Collider Offset", mover.colliderOffset);

		if(EditorGUI.EndChangeCheck())
		{
			mover.RecalculateColliderDimensions();
			OnEditorVariableChanged();
		}

		GUILayout.Label("Sensor Options", EditorStyles.boldLabel);

		EditorGUI.BeginChangeCheck();

		mover.sensorType = (Sensor.CastType)EditorGUILayout.EnumPopup("Sensor Type", mover.sensorType);
		mover.sensorLayermask = EditorGUILayout.MaskField("Layermask", mover.sensorLayermask, physicsLayers);

		mover.isInDebugMode = EditorGUILayout.Toggle("Debug Mode",mover.isInDebugMode);

		if(EditorGUI.EndChangeCheck())
		{
			OnEditorVariableChanged();
		}

		if(mover.sensorType == Sensor.CastType.RaycastArray)
			GUILayout.Label("Advanced Options", EditorStyles.centeredGreyMiniLabel);
		GUILayout.Space(5);

		if(mover.sensorType == Sensor.CastType.Raycast)
		{
		}
		else if(mover.sensorType == Sensor.CastType.Spherecast)
		{

		}
		else if(mover.sensorType == Sensor.CastType.RaycastArray)
		{
			if(raycastArrayPositions == null)
				raycastArrayPositions = Sensor.GetRaycastStartPositions(mover.sensorArrayRows, mover.sensorArrayRayCount, mover.sensorArrayRowsAreOffset, 1f);

			EditorGUI.BeginChangeCheck();

			mover.sensorArrayRayCount = EditorGUILayout.IntSlider("Number", mover.sensorArrayRayCount, 3, 9);
			mover.sensorArrayRows = EditorGUILayout.IntSlider("Rows", mover.sensorArrayRows, 1, 5);
			mover.sensorArrayRowsAreOffset = EditorGUILayout.Toggle("Offset Rows", mover.sensorArrayRowsAreOffset);

			if(EditorGUI.EndChangeCheck())
			{
				raycastArrayPositions = Sensor.GetRaycastStartPositions(mover.sensorArrayRows, mover.sensorArrayRayCount, mover.sensorArrayRowsAreOffset, 1f);
				OnEditorVariableChanged();
			}

			GUILayout.Space(5);

			_space = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(100));

			Rect background = new Rect(_space.x + (_space.width - _space.height)/2f, _space.y, _space.height, _space.height);
			EditorGUI.DrawRect(background, Color.grey);

			float point_size = 3f;

			Vector2 center = new Vector2(background.x + background.width/2f, background.y + background.height/2f);

			if(raycastArrayPositions != null && raycastArrayPositions.Length != 0)
			{
				for(int i = 0; i < raycastArrayPositions.Length; i++)
				{
					Vector2 position = center + new Vector2(raycastArrayPositions[i].x, raycastArrayPositions[i].z) * background.width/2f * 0.9f;

					EditorGUI.DrawRect(new Rect(position.x - point_size/2f, position.y - point_size/2f, point_size, point_size), Color.white);
				}
			}

			if(raycastArrayPositions != null && raycastArrayPositions.Length != 0)
				GUILayout.Label("Number of rays = " + raycastArrayPositions.Length, EditorStyles.centeredGreyMiniLabel );
		}
	}

	void OnEditorVariableChanged()
	{
		EditorUtility.SetDirty(mover);
		EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
	}
}
