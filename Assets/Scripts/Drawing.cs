using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

using PDollarGestureRecognizer;

public class Drawing : MonoBehaviour {
	public Transform gestureOnScreenPrefab;
	
	private List<Gesture> trainingSet = new List<Gesture>();
	
	private List<Point> points = new List<Point>();
	private int strokeId = -1;

	private Rect drawArea;
	
	private RuntimePlatform platform;
	private int vertexCount = 0;
	
	private List<LineRenderer> gestureLinesRenderer = new List<LineRenderer>();
	private LineRenderer currentGestureLineRenderer;

	//VR
	SteamVR_TrackedObject trackedObj;
	
	//GUI
	private string message;
	private bool recognized;
	private string newGestureName = "";

	void Awake()
	{
		trackedObj = GetComponent<SteamVR_TrackedObject>();
	}
	
	void Start () {
		
		platform = Application.platform;
		drawArea = new Rect(0, 0, Screen.width, Screen.height);
		
		//Load pre-made gestures
		TextAsset[] gesturesXml = Resources.LoadAll<TextAsset>("GestureSet/10-stylus-MEDIUM/");
		foreach (TextAsset gestureXml in gesturesXml)
			trainingSet.Add(GestureIO.ReadGestureFromXML(gestureXml.text));
		
		//Load user custom gestures
		string[] filePaths = Directory.GetFiles(Application.persistentDataPath, "*.xml");
		foreach (string filePath in filePaths)
			trainingSet.Add(GestureIO.ReadGestureFromFile(filePath));
	}
	
	void FixedUpdate () {
		
//		if (platform == RuntimePlatform.Android || platform == RuntimePlatform.IPhonePlayer) {
//			if (Input.touchCount > 0) {
//				virtualKeyPosition = new Vector3(Input.GetTouch(0).position.x, Input.GetTouch(0).position.y);
//			}
//		} else {
//			if (Input.GetMouseButton(0)) {
//				virtualKeyPosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y);
//			}
//		}

		var device = SteamVR_Controller.Input((int)trackedObj.index);

		if (device.GetTouchDown(SteamVR_Controller.ButtonMask.Trigger) || Input.GetMouseButtonDown(0)) {
			Debug.Log ("PRESS!");
				if (recognized) {
					
					recognized = false;
					strokeId = -1;
					
					points.Clear();
					
					foreach (LineRenderer lineRenderer in gestureLinesRenderer) {
						
						lineRenderer.SetVertexCount(0);
						Destroy(lineRenderer.gameObject);
					}
					
					gestureLinesRenderer.Clear();
				}
				
				++strokeId;
				
				Transform tmpGesture = Instantiate(gestureOnScreenPrefab, transform.position, transform.rotation) as Transform;
				currentGestureLineRenderer = tmpGesture.GetComponent<LineRenderer>();
				
				gestureLinesRenderer.Add(currentGestureLineRenderer);
				
				vertexCount = 0;
			}
			
			if (device.GetTouch(SteamVR_Controller.ButtonMask.Trigger) || Input.GetMouseButton(0)) {
			points.Add(new Point(transform.position.x, transform.position.y, strokeId));
				
				currentGestureLineRenderer.SetVertexCount(++vertexCount);
				currentGestureLineRenderer.SetPosition(vertexCount - 1, transform.position);
			}
			
			if(device.GetTouchUp(SteamVR_Controller.ButtonMask.Trigger) || Input.GetMouseButtonUp(0)) {
				recognized = true;
				
				Gesture candidate = new Gesture(points.ToArray());
				Result gestureResult = PointCloudRecognizer.Classify(candidate, trainingSet.ToArray());
				
				message = gestureResult.GestureClass + " " + gestureResult.Score;
			}
	}
	
	void Update() {
		Debug.Log (message);
	}
}
