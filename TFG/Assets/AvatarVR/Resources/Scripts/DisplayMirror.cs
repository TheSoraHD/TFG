// original source from: http://wiki.unity3d.com/index.php/MirrorReflection4
// This is in fact just the Water script from Pro Standard Assets, just with refraction stuff removed.

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[ExecuteInEditMode]
public class DisplayMirror : MonoBehaviour
{
    public bool sideMirror = false;

    private const string MAIN_TEXTURE_ID = "_MainTex";
    private const string REFLECTION_TEXTURE_LEFT_ID = "_ReflectionTexLeft";
    private const string REFLECTION_TEXTURE_RIGHT_ID = "_ReflectionTexRight";
    private const string DISPLAY_TEXTURE_ID = "_DisplayTex";
    private const int DISPLAY_TEXTURE_SIZE = 2048;

    /***************************************************************************************/

    // Display properties
    private int fontCountX = 10;
    private int fontCountY = 10;

    private float lineSpacing = 0.75f;
    private float characterSize = 0.9f; // 1 = the exact size in pixels that the font appears in the texture
    private int textPlacementX = 25;
    private int textPlacementY = 925;

    private string oldMessage = "";
    private Texture oldTexture = null;

    private TMPro.TextMeshProUGUI textMesh;
    private Material material;

    /***************************************************************************************/

    // Mirror properties

    private GameObject camerasHolder = null;

    private bool m_DisablePixelLights = true;
    private int m_TextureSize = 2048;
    private float m_ClipPlaneOffset = 0.07f;

    private LayerMask m_ReflectLayers = -1;

    private Dictionary<Camera, Camera> m_ReflectionCameras = new Dictionary<Camera, Camera>();
    private static HashSet<Camera> currentReflectionCameras = new HashSet<Camera>();

    private RenderTexture m_ReflectionTextureLeft = null;
    private RenderTexture m_ReflectionTextureRight = null;
    private int m_OldReflectionTextureSize = 0;

    /***************************************************************************************/

    [HideInInspector] public AvatarVR avatarVR;

    void Awake()
    {
        avatarVR = GetComponentInParent<AvatarVR>();

        // Find references among children
        GameObject textGO = transform.parent.Find("Display").GetChild(0).GetChild(0).gameObject; // Text GameObject
        textMesh = textGO.AddComponent<TMPro.TextMeshProUGUI>();
        textMesh.fontSize = 10.0f;
        textMesh.text = "";
        textMesh.rectTransform.offsetMin = Vector2.zero;
        textMesh.rectTransform.offsetMax = Vector2.zero;
        textMesh.color = Color.black;

#if UNITY_EDITOR
        // Create a layer for everything that should not be seen on the mirror
        LayerUtils.CreateLayer("NotMirror");
        LayerUtils.MoveToLayer(transform.parent.gameObject, "NotMirror");
#endif

        if (sideMirror) CleanText();
    }

    // This is called when it's known that the object will be rendered by some
    // camera. We render reflections and do other updates here. Because the script
    // executes in edit mode, reflections for the scene view camera will just work!
    public void OnWillRenderObject()
    {
        return;
        //if (m_frameCounter > 0)
        //{
        //    m_frameCounter--;
        //    return;
        //}

        //var rend = GetComponent<Renderer>();
        //if (!enabled || !rend || !rend.sharedMaterial || !rend.enabled)
        //{
        //    return;
        //}

        //Camera cam = Camera.current;
        //if (!cam)
        //{
        //    return;
        //}

        //// Safeguard from recursive reflections.    
        //if (s_InsideRendering || currentReflectionCameras.Contains(cam))
        //{
        //    return;
        //}
        //s_InsideRendering = true;

        //m_frameCounter = m_framesNeededToUpdate;

        //RenderCamera(cam, rend, Camera.StereoscopicEye.Left, ref m_ReflectionTextureLeft);
        //if (cam.stereoEnabled)
        //{
        //    RenderCamera(cam, rend, Camera.StereoscopicEye.Right, ref m_ReflectionTextureRight);
        //}
    }

    // Cleanup all the objects we possibly have created
    void OnDisable()
    {
        if (m_ReflectionTextureLeft)
        {
            DestroyImmediate(m_ReflectionTextureLeft);
            m_ReflectionTextureLeft = null;
        }

        if (m_ReflectionTextureRight)
        {
            DestroyImmediate(m_ReflectionTextureRight);
            m_ReflectionTextureRight = null;
        }

        foreach (var kvp in m_ReflectionCameras)
        {
            DestroyImmediate(((Camera)kvp.Value).gameObject);
        }
        m_ReflectionCameras.Clear();
    }

    /*************************************** Display ***************************************/

    public void CleanText()
    {
        textMesh.text = "";
        material.SetTexture(DISPLAY_TEXTURE_ID, null);
    }

    public void ShowText(string message, Color background, int secs, bool block = false, bool overwrite = true)
    {
        Texture2D bTexture = TextToTexture.CreateFillTexture2D(background, 1, 1);
        bTexture.Apply();
        try
        {
            StartCoroutine(ShowTextForSeconds(message, secs, bTexture, block, overwrite));
        }
        catch (System.Exception)
        {

        }
    }

    public void ShowTextAgain(string message, Color background, int secs, string message2, Color background2, int secs2, bool block = false, bool overwrite = true)
    {
        Texture2D bTexture = TextToTexture.CreateFillTexture2D(background, 1, 1);
        bTexture.Apply();
        Texture2D bTexture2 = TextToTexture.CreateFillTexture2D(background2, 1, 1);
        bTexture2.Apply();
        StartCoroutine(ShowTextForSecondsAgain(message, secs, bTexture, message2, secs2, bTexture2, block, overwrite));
    }

    public void ShowTextAsTexture(string message, Color background, int secs, bool block = false, bool overwrite = true)
    {
        TextToTexture textToTexture = new TextToTexture(null, fontCountX, fontCountY, PerCharacterKerning.ArialCharacterKerning(), true, background, Color.black);
        Texture2D newText = textToTexture.CreateTextToTexture(message, textPlacementX, textPlacementY, DISPLAY_TEXTURE_SIZE, characterSize, lineSpacing);
        StartCoroutine(ShowTextureForSeconds(secs, newText, block, overwrite));
    }

    /*********************************** Mirror helpers ************************************/

    private void RenderCamera(Camera cam, Renderer rend, Camera.StereoscopicEye eye, ref RenderTexture reflectionTexture)
    {
        Camera reflectionCamera;
        CreateMirrorObjects(cam, eye, out reflectionCamera, ref reflectionTexture);

        // find out the reflection plane: position and normal in world space
        Vector3 pos = transform.position;
        Vector3 normal = transform.up;

        // Optionally disable pixel lights for reflection
        int oldPixelLightCount = QualitySettings.pixelLightCount;
        if (m_DisablePixelLights)
        {
            QualitySettings.pixelLightCount = 0;
        }

        CopyCameraProperties(cam, reflectionCamera);

        // Render reflection
        // Reflect camera around reflection plane
        float d = -Vector3.Dot(normal, pos) - m_ClipPlaneOffset;
        Vector4 reflectionPlane = new Vector4(normal.x, normal.y, normal.z, d);

        Matrix4x4 reflection = Matrix4x4.zero;
        CalculateReflectionMatrix(ref reflection, reflectionPlane);

        Vector3 oldEyePos;
        Matrix4x4 worldToCameraMatrix;
        if (cam.stereoEnabled)
        {
            worldToCameraMatrix = cam.GetStereoViewMatrix(eye) * reflection;

            var eyeOffset = SteamVR.instance.eyes[(int)eye].pos;
            eyeOffset.z = 0.0f;
            oldEyePos = cam.transform.position + cam.transform.TransformVector(eyeOffset);
        }
        else
        {
            worldToCameraMatrix = cam.worldToCameraMatrix * reflection;
            oldEyePos = cam.transform.position;
        }

        Vector3 newEyePos = reflection.MultiplyPoint(oldEyePos);
        reflectionCamera.transform.position = newEyePos;

        reflectionCamera.worldToCameraMatrix = worldToCameraMatrix;

        // Setup oblique projection matrix so that near plane is our reflection
        // plane. This way we clip everything below/above it for free.
        Vector4 clipPlane = CameraSpacePlane(worldToCameraMatrix, pos, normal, 1.0f);

        Matrix4x4 projectionMatrix;
        if (cam.stereoEnabled)
        {
            projectionMatrix = HMDMatrix4x4ToMatrix4x4(SteamVR.instance.hmd.GetProjectionMatrix((Valve.VR.EVREye)eye, cam.nearClipPlane, cam.farClipPlane));
        }
        else
        {
            projectionMatrix = cam.projectionMatrix;
        }

        //projectionMatrix = cam.CalculateObliqueMatrix(clipPlane);
        MakeProjectionMatrixOblique(ref projectionMatrix, clipPlane);

        reflectionCamera.projectionMatrix = projectionMatrix;
        reflectionCamera.cullingMask = m_ReflectLayers.value;
        reflectionCamera.targetTexture = reflectionTexture;
        GL.invertCulling = true;
        //Vector3 euler = cam.transform.eulerAngles;
        //reflectionCamera.transform.eulerAngles = new Vector3(0, euler.y, euler.z);
        reflectionCamera.transform.rotation = cam.transform.rotation;
        try
        {
            reflectionCamera.Render();
        }
        catch (System.Exception e)
        {
            Debug.LogWarning(e.Message);
        }
        //reflectionCamera.transform.position = oldEyePos;
        GL.invertCulling = false;
        Material[] materials = rend.sharedMaterials;
        string property = "_ReflectionTex" + eye.ToString();
        foreach (Material mat in materials)
        {
            if (mat.HasProperty(property))
                mat.SetTexture(property, reflectionTexture);
        }

        // Restore pixel light count
        if (m_DisablePixelLights)
            QualitySettings.pixelLightCount = oldPixelLightCount;

    }

    private void CopyCameraProperties(Camera src, Camera dest)
    {
        if (dest == null)
        {
            return;
        }

        // set camera to clear the same way as current camera
        dest.clearFlags = src.clearFlags;
        dest.backgroundColor = src.backgroundColor;
        if (src.clearFlags == CameraClearFlags.Skybox)
        {
            Skybox sky = src.GetComponent(typeof(Skybox)) as Skybox;
            Skybox mysky = dest.GetComponent(typeof(Skybox)) as Skybox;
            if (!sky || !sky.material)
            {
                mysky.enabled = false;
            }
            else
            {
                mysky.enabled = true;
                mysky.material = sky.material;
            }
        }

        // update other values to match current camera.
        // even if we are supplying custom camera&projection matrices,
        // some of values are used elsewhere (e.g. skybox uses far plane)
        dest.farClipPlane = Mathf.Max(src.farClipPlane, 1);
        dest.nearClipPlane = Mathf.Max(src.nearClipPlane, 0.01f);
        dest.orthographic = src.orthographic;
        dest.fieldOfView = src.fieldOfView;
        dest.aspect = src.aspect;
        dest.orthographicSize = src.orthographicSize;
    }

    // On-demand create any objects we need
    private void CreateMirrorObjects(Camera currentCamera, Camera.StereoscopicEye eye, out Camera reflectionCamera, ref RenderTexture reflectionTexture)
    {
        reflectionCamera = null;

        // Reflection render texture
        if (!reflectionTexture || m_OldReflectionTextureSize != m_TextureSize)
        {
            if (reflectionTexture)
            {
                DestroyImmediate(reflectionTexture);
            }
            reflectionTexture = new RenderTexture(m_TextureSize, m_TextureSize, 16);
            reflectionTexture.name = "__MirrorReflection" + eye.ToString() + GetInstanceID();
            reflectionTexture.isPowerOfTwo = true;
            reflectionTexture.hideFlags = HideFlags.DontSave;
            m_OldReflectionTextureSize = m_TextureSize;
        }

        // Camera for reflection
        if (!m_ReflectionCameras.TryGetValue(currentCamera, out reflectionCamera)) // catch both not-in-dictionary and in-dictionary-but-deleted-GO
        {
            GameObject go = new GameObject("Mirror Reflection Camera id" + GetInstanceID() + " for " + currentCamera.GetInstanceID(), typeof(Camera), typeof(Skybox));
            if (!camerasHolder)
            {
                Transform camerasHolderTransform = transform.Find("Cameras");
                if (camerasHolderTransform)
                {
                    camerasHolder = camerasHolderTransform.gameObject;
                }
                if (!camerasHolder)
                {
                    camerasHolder = new GameObject("Cameras");
                    camerasHolder.transform.parent = transform;
                    camerasHolder.transform.localPosition = Vector3.zero;
                    camerasHolder.transform.localRotation = Quaternion.identity;
                    camerasHolder.transform.localScale = Vector3.one;
                }
            }
            go.transform.parent = camerasHolder.transform;
            camerasHolder.transform.localPosition = Vector3.zero;
            camerasHolder.transform.localRotation = Quaternion.identity;
            camerasHolder.transform.localScale = Vector3.one;

            reflectionCamera = go.GetComponent<Camera>();
            reflectionCamera.enabled = false;
            reflectionCamera.transform.position = transform.position;
            reflectionCamera.transform.rotation = transform.rotation;
            reflectionCamera.gameObject.AddComponent<FlareLayer>();
            go.hideFlags = HideFlags.DontSave;

            // Mirror cameras should not render stuff in NotMirror layer
#if UNITY_EDITOR
            LayerUtils.HideLayerInCamera("NotMirror", reflectionCamera);
#endif

            currentReflectionCameras.Add(reflectionCamera);
            m_ReflectionCameras.Add(currentCamera, reflectionCamera);
        }
    }

    // Given position/normal of the plane, calculates plane in camera space.
    private Vector4 CameraSpacePlane(Matrix4x4 worldToCameraMatrix, Vector3 pos, Vector3 normal, float sideSign)
    {
        Vector3 offsetPos = pos + normal * m_ClipPlaneOffset;
        Vector3 cpos = worldToCameraMatrix.MultiplyPoint(offsetPos);
        Vector3 cnormal = worldToCameraMatrix.MultiplyVector(normal).normalized * sideSign;
        return new Vector4(cnormal.x, cnormal.y, cnormal.z, -Vector3.Dot(cpos, cnormal));
    }

    // Calculates reflection matrix around the given plane
    private static void CalculateReflectionMatrix(ref Matrix4x4 reflectionMat, Vector4 plane)
    {
        reflectionMat.m00 = (1F - 2F * plane[0] * plane[0]);
        reflectionMat.m01 = (-2F * plane[0] * plane[1]);
        reflectionMat.m02 = (-2F * plane[0] * plane[2]);
        reflectionMat.m03 = (-2F * plane[3] * plane[0]);

        reflectionMat.m10 = (-2F * plane[1] * plane[0]);
        reflectionMat.m11 = (1F - 2F * plane[1] * plane[1]);
        reflectionMat.m12 = (-2F * plane[1] * plane[2]);
        reflectionMat.m13 = (-2F * plane[3] * plane[1]);

        reflectionMat.m20 = (-2F * plane[2] * plane[0]);
        reflectionMat.m21 = (-2F * plane[2] * plane[1]);
        reflectionMat.m22 = (1F - 2F * plane[2] * plane[2]);
        reflectionMat.m23 = (-2F * plane[3] * plane[2]);

        reflectionMat.m30 = 0F;
        reflectionMat.m31 = 0F;
        reflectionMat.m32 = 0F;
        reflectionMat.m33 = 1F;
    }

    private Matrix4x4 HMDMatrix4x4ToMatrix4x4(Valve.VR.HmdMatrix44_t input)
    {
        var m = Matrix4x4.identity;

        m[0, 0] = input.m0;
        m[0, 1] = input.m1;
        m[0, 2] = input.m2;
        m[0, 3] = input.m3;

        m[1, 0] = input.m4;
        m[1, 1] = input.m5;
        m[1, 2] = input.m6;
        m[1, 3] = input.m7;

        m[2, 0] = input.m8;
        m[2, 1] = input.m9;
        m[2, 2] = input.m10;
        m[2, 3] = input.m11;

        m[3, 0] = input.m12;
        m[3, 1] = input.m13;
        m[3, 2] = input.m14;
        m[3, 3] = input.m15;

        return m;
    }

    // taken from http://www.terathon.com/code/oblique.html
    private static void MakeProjectionMatrixOblique(ref Matrix4x4 matrix, Vector4 clipPlane)
    {
        Vector4 q;

        // Calculate the clip-space corner point opposite the clipping plane
        // as (sgn(clipPlane.x), sgn(clipPlane.y), 1, 1) and
        // transform it into camera space by multiplying it
        // by the inverse of the projection matrix

        q.x = (Utils.sgn(clipPlane.x) + matrix[8]) / matrix[0];
        q.y = (Utils.sgn(clipPlane.y) + matrix[9]) / matrix[5];
        q.z = -1.0F;
        q.w = (1.0F + matrix[10]) / matrix[14];

        // Calculate the scaled plane vector
        Vector4 c = clipPlane * (2.0F / Vector3.Dot(clipPlane, q));

        // Replace the third row of the projection matrix
        matrix[2] = c.x;
        matrix[6] = c.y;
        matrix[10] = c.z + 1.0F;
        matrix[14] = c.w;
    }

    /*********************************** Display helpers ***********************************/

    private IEnumerator ShowTextForSeconds(string message, int secs, Texture2D text, bool block = false, bool overwrite = true)
    {
        if (material == null) material = GetComponent<Renderer>().sharedMaterial;

        if (block) avatarVR.singleInput.blockControllers(true);

        if (overwrite)
        {
            oldMessage = textMesh.text;
            oldTexture = material.GetTexture(DISPLAY_TEXTURE_ID);
        }
        textMesh.text = "\n\n\n\n\n" + message;
        material.SetTexture(DISPLAY_TEXTURE_ID, text);
        if (secs > 0)
        {
            yield return new WaitForSeconds(secs);
            textMesh.text = "\n\n\n\n\n" + oldMessage;
            material.SetTexture(DISPLAY_TEXTURE_ID, oldTexture);
        }

        if (block) avatarVR.singleInput.blockControllers(false);
    }

    private IEnumerator ShowTextForSecondsAgain(string message, int secs, Texture2D text, string message2, int secs2, Texture2D text2, bool block = false, bool overwrite = true)
    {
        if (material == null) material = GetComponent<Renderer>().sharedMaterial;

        if (block) avatarVR.singleInput.blockControllers(true);

        if (overwrite)
        {
            oldMessage = textMesh.text;
            oldTexture = material.GetTexture(DISPLAY_TEXTURE_ID);
        }
        textMesh.text = "\n\n\n\n\n" + message;
        material.SetTexture(DISPLAY_TEXTURE_ID, text);
        if (secs > 0)
        {
            yield return new WaitForSeconds(secs);
            textMesh.text = "\n\n\n\n\n" + oldMessage;
            material.SetTexture(DISPLAY_TEXTURE_ID, oldTexture);
        }

        if (overwrite)
        {
            oldMessage = textMesh.text;
            oldTexture = material.GetTexture(DISPLAY_TEXTURE_ID);
        }
        textMesh.text = "\n\n\n\n\n" + message2;
        material.SetTexture(DISPLAY_TEXTURE_ID, text2);
        if (secs2 > 0)
        {
            yield return new WaitForSeconds(secs2);
            textMesh.text = "\n\n\n\n\n" + oldMessage;
            material.SetTexture(DISPLAY_TEXTURE_ID, oldTexture);
        }

        if (block) avatarVR.singleInput.blockControllers(false);
    }

    private IEnumerator ShowTextureForSeconds(int secs, Texture2D text, bool block = false, bool overwrite = true)
    {
        if (material == null) material = GetComponent<Renderer>().sharedMaterial;

        if (block) avatarVR.singleInput.blockControllers(true);

        if (overwrite)
        {
            oldTexture = material.GetTexture(DISPLAY_TEXTURE_ID);
        }
        material.SetTexture(DISPLAY_TEXTURE_ID, text);
        if (secs > 0)
        {
            yield return new WaitForSeconds(secs);
            material.SetTexture(DISPLAY_TEXTURE_ID, oldTexture);
        }

        if (block) avatarVR.singleInput.blockControllers(false);
    }
}