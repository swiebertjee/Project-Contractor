﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Singleton;

#region values
[System.Serializable]
public class Sonar {
	public int maxPulses = 10;
	public float interval = 2;
	public float width = 2;
	[Range(0,5)]
	public float widthFade = 0;
	public float fade = 30;
	public float edgeWidth = 1;
	public float distance = 100;
	public float speed = 50;
	public bool active = false;
	public float start = 10;
	public bool enabled = true;
}
[System.Serializable]
class Fog {
	public Color startColor, endColor;
	public float _surface = 100f;
	public float _maxDepth = -200f;

	[Range(0,1)]
	public float _intensity = 0;
	[Range(1,10)]
	public float _curveShift = 0;
}
[System.Serializable]
class Caustics {
	public Color causticsColor;
	[Range(0f,50f)]
	public float size = 30f, intensity = 30f;
	public float _causticsDepth = -50f;
}
#endregion

public class underwaterFX : MonoBehaviour {

	[SerializeField]
	private bool update = false;

	[SerializeField]
	private Sonar _sonar = new Sonar();
	public Sonar SonarVals{get{ return _sonar; }}

	[SerializeField]
	private Fog _fog = new Fog();

	[SerializeField]
	private Caustics _caustics = new Caustics();

	public Material _mat; 
	public Transform _origin;
	private Camera _cam;
	private Light _light;

	public float[] aPulse;
	//	public float[] pulses{get{return aPulse;}}

	private bool[] activepulse;
	private Vector4[] aOrigin;

	private float _depth;

	void Start() {

		_light = GameObject.Find ("Directional Light").GetComponent<Light>();
		_cam = Camera.main;
		_cam.depthTextureMode = DepthTextureMode.DepthNormals;
		activepulse = new bool[_sonar.maxPulses];
		aPulse = new float[_sonar.maxPulses];
		aOrigin = new Vector4[_sonar.maxPulses];
		SetupPulses ();

		// camera values
		//		_mat.SetFloat("_cameraFar", _cam.farClipPlane);

		updateStart ();
	}

	void Update () {
		PassiveSonar ();
		PulseActivate ();
		PulseControl ();
		//		_depth = calculateWorldDepth ();
		lightUpdate();
	}

	float calculateWorldDepth() {
		float camDepth = Camera.main.transform.position.y;
		float depth = (camDepth - _fog._surface) / (_fog._maxDepth - _fog._surface);
		return depth;
	}

    void lightUpdate() {
        float intensity = 1 - calculateWorldDepth();

        Vector2 worldPos = new Vector2(_origin.position.x, _origin.position.y);

        foreach (var zone in DarkZones.Get())
        {
            Vector2 zonePos = new Vector2(zone.Position.x, zone.Position.y);
            float dist = Vector2.Distance(worldPos, zonePos);

            if (dist > zone.CloseRadius + zone.FarRadius)
                continue;

            if (dist < zone.CloseRadius)
                intensity *= zone.Color.grayscale;
            else
                intensity *= Mathf.Abs((zone.CloseRadius - dist) / zone.FarRadius);
            
            break;
        }

        _light.intensity = intensity;


    }

	void SetupPulses () {
		for (int i = 0; i < _sonar.maxPulses; i++) {
			aPulse [i] = 0;
		}
	}

	void PulseActivate() {
		if (_sonar.active) {
			for (int i = 0; i < _sonar.maxPulses; i++) {
				if (!activepulse [i]) {
					activepulse [i] = true;
					aOrigin [i] = _origin.position;
					return;
				}
			}
		}
	}

	void PulseControl() {
		for (int i = 0; i < _sonar.maxPulses; i++) {
			if (activepulse [i]) {
				aPulse [i] += Time.deltaTime * _sonar.speed;
				if (aPulse [i] > _sonar.distance) {
					activepulse [i] = false;
					aPulse [i] = 0;
				}
			}
		}
	}

	float time;
	void PassiveSonar () {
		time += Time.deltaTime;
		if (!_sonar.enabled)
			return;
		_sonar.active = false;
		if (time > _sonar.interval) {
			_sonar.active = true;
			time = 0;
		}
	}

	void updateStart () {
		// sonar
		_mat.SetInt ("_pulselength", _sonar.maxPulses);
		_mat.SetFloatArray ("_pulses", aPulse);
		_mat.SetVectorArray ("originarray", aOrigin);
		_mat.SetFloat ("width", _sonar.width);
		_mat.SetFloat ("widthFade", _sonar.widthFade);
		_mat.SetFloat ("fade", _sonar.fade);
		_mat.SetFloat ("edgeWidth", _sonar.edgeWidth);
		_mat.SetFloat ("_start", _sonar.start);
		_mat.SetFloat ("_distance", _sonar.distance);

		// fog
		_mat.SetColor("_startColor", _fog.startColor);
		_mat.SetColor("_endColor", _fog.endColor);
		_mat.SetFloat("surface", _fog._surface);
		_mat.SetFloat("_fogDepth", _fog._maxDepth);


		_mat.SetFloat ("_intensity", _fog._intensity);
		_mat.SetFloat ("_curve", _fog._curveShift);


        //_mat.SetFloat("_darkZones", DarkZones.Get().Count);
        //_mat.SetVectorArray("_darkPositions", DarkZones.Positions());
        //_mat.SetFloatArray("_darkCloseRadius", DarkZones.CloseRadius());
        //_mat.SetFloatArray("_darkFarRadius", DarkZones.FarRadius());

        //_mat.SetVectorArray("_darkColors", DarkZones.Colors());

		// caustics
		_mat.SetFloat("causticsSize", _caustics.size);
		_mat.SetFloat("causticsIntensity", _caustics.intensity);
		_mat.SetFloat ("causticsDepth", _caustics._causticsDepth);
		_mat.SetColor ("causticsColor", _caustics.causticsColor);
	}

	void updateShader()
	{
		// sonar
		_mat.SetFloatArray ("_pulses", aPulse);
		_mat.SetVectorArray ("originarray", aOrigin);

	}

	[ImageEffectOpaque]
	void OnRenderImage (RenderTexture src, RenderTexture dst){
		updateShader ();

		if (update)
			updateStart ();

		RaycastCornerBlit (src, dst, _mat);

		foreach (var volume in Volumes.Get())
			volume.Render(ref dst);
	}

	void RaycastCornerBlit(RenderTexture source, RenderTexture dest, Material mat)
	{
		// Compute Frustum Corners
		float camFar = _cam.farClipPlane;
		float camFov = _cam.fieldOfView;
		float camAspect = _cam.aspect;

		float fovWHalf = camFov * 0.5f;

		Vector3 toRight = _cam.transform.right * Mathf.Tan(fovWHalf * Mathf.Deg2Rad) * camAspect;
		Vector3 toTop = _cam.transform.up * Mathf.Tan(fovWHalf * Mathf.Deg2Rad);

		Vector3 topLeft = (_cam.transform.forward - toRight + toTop);
		float camScale = topLeft.magnitude * camFar;

		topLeft.Normalize();
		topLeft *= camScale;

		Vector3 topRight = (_cam.transform.forward + toRight + toTop);
		topRight.Normalize();
		topRight *= camScale;

		Vector3 bottomRight = (_cam.transform.forward + toRight - toTop);
		bottomRight.Normalize();
		bottomRight *= camScale;

		Vector3 bottomLeft = (_cam.transform.forward - toRight - toTop);
		bottomLeft.Normalize();
		bottomLeft *= camScale;

		// Custom Blit, encoding Frustum Corners as additional Texture Coordinates
		RenderTexture.active = dest;

		mat.SetTexture("_Scene", source);

		GL.PushMatrix();
		GL.LoadOrtho();

		mat.SetPass(0);

		GL.Begin(GL.QUADS);

		GL.MultiTexCoord2(0, 0.0f, 0.0f);
		GL.MultiTexCoord(1, bottomLeft);
		GL.Vertex3(0.0f, 0.0f, 0.0f);

		GL.MultiTexCoord2(0, 1.0f, 0.0f);
		GL.MultiTexCoord(1, bottomRight);
		GL.Vertex3(1.0f, 0.0f, 0.0f);

		GL.MultiTexCoord2(0, 1.0f, 1.0f);
		GL.MultiTexCoord(1, topRight);
		GL.Vertex3(1.0f, 1.0f, 0.0f);

		GL.MultiTexCoord2(0, 0.0f, 1.0f);
		GL.MultiTexCoord(1, topLeft);
		GL.Vertex3(0.0f, 1.0f, 0.0f);

		GL.End();
		GL.PopMatrix();
	}

}