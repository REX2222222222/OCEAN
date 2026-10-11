using UnityEngine;
using Unity.Sentis;
using TMPro; 
using UnityEngine.UI; 
using System.Linq;

public class YoloClassifier : MonoBehaviour
{
    [Header("模型與資料")]
    public ModelAsset modelAsset;
    public TextAsset labelsAsset;
    
    [Header("UI 連結 (請拖拉對應物件)")]
    public TextMeshProUGUI resultText; 
    public RawImage displayImage;     
    public Button cameraButton;      
    
    [Header("系統串接")]
    public YoloUIManager uiManager;           
    public float detectionThreshold = 0.7f;   

    private TextMeshProUGUI m_ButtonText; 
    private Worker m_Worker;
    private WebCamTexture m_WebCam;
    private string[] m_Labels;
    private bool isScanning = false;
    private bool isRequestingCamera; 

    // 👉 新增：用來記住目前畫面上的植物，避免重複瘋狂觸發 UI
    private string lastDetectedPlant = ""; 

    void Start()
    {
        if (labelsAsset != null)
        {
            m_Labels = labelsAsset.text.Split(new[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
        }

        Model runtimeModel = ModelLoader.Load(modelAsset);
        m_Worker = new Worker(runtimeModel, BackendType.GPUCompute);

        displayImage.enabled = false; 
        resultText.text = "";        

        m_ButtonText = cameraButton.GetComponentInChildren<TextMeshProUGUI>();
        if (m_ButtonText != null) m_ButtonText.text = "開啟相機辨識"; 
        cameraButton.onClick.AddListener(OnCameraButtonPressed); 
    }

    void OnCameraButtonPressed()
    {
        if (isRequestingCamera) return;
        if (isScanning)
        {
            isScanning = false;
            m_WebCam?.Stop();
            displayImage.enabled = false;
            if (m_ButtonText != null) m_ButtonText.text = "開啟相機辨識";
            resultText.text = "";
            return;
        }
        StartCoroutine(StartCamera());
    }

    private System.Collections.IEnumerator StartCamera()
    {
        isRequestingCamera = true;
#if UNITY_ANDROID
        if (!UnityEngine.Android.Permission.HasUserAuthorizedPermission(UnityEngine.Android.Permission.Camera))
        {
            bool answered = false;
            var callbacks = new UnityEngine.Android.PermissionCallbacks();
            callbacks.PermissionGranted += permission => answered = true;
            callbacks.PermissionDenied += permission => answered = true;
            callbacks.PermissionDeniedAndDontAskAgain += permission => answered = true;
            UnityEngine.Android.Permission.RequestUserPermission(UnityEngine.Android.Permission.Camera, callbacks);
            while (!answered) yield return null;
            if (!UnityEngine.Android.Permission.HasUserAuthorizedPermission(UnityEngine.Android.Permission.Camera))
            {
                resultText.text = "相機權限未開啟，請到手機設定允許此 App 使用相機。";
                isRequestingCamera = false;
                yield break;
            }
        }
#endif
#if UNITY_IOS
        if (!Application.HasUserAuthorization(UserAuthorization.WebCam))
            yield return Application.RequestUserAuthorization(UserAuthorization.WebCam);
        if (!Application.HasUserAuthorization(UserAuthorization.WebCam))
        {
            resultText.text = "相機權限未開啟，請到 iPhone／iPad 設定允許此 App 使用相機。";
            isRequestingCamera = false;
            yield break;
        }
#endif
        if (m_WebCam == null)
        {
            WebCamDevice[] devices = WebCamTexture.devices;
            if (devices.Length == 0)
            {
                resultText.text = "找不到可用的相機。";
                isRequestingCamera = false;
                yield break;
            }
            string deviceName = devices[0].name;
            foreach (WebCamDevice device in devices)
                if (!device.isFrontFacing) { deviceName = device.name; break; }
            m_WebCam = new WebCamTexture(deviceName, 1280, 720, 30);
        }
        m_WebCam.Play();
        isScanning = true;
        isRequestingCamera = false;
        displayImage.texture = m_WebCam;
        displayImage.enabled = true;
        if (m_ButtonText != null) m_ButtonText.text = "關閉相機";
        resultText.text = "正在初始化鏡頭...";
        yield break;
    }
    void Update()
    {
        if (!isScanning) return;
        if (!m_WebCam.isPlaying || !m_WebCam.didUpdateThisFrame) return;

        using Tensor<float> inputTensor = TextureConverter.ToTensor(m_WebCam, width: 640, height: 640, channels: 3);
        m_Worker.Schedule(inputTensor);

        using Tensor<float> outputTensor = m_Worker.PeekOutput() as Tensor<float>;
        float[] outputData = outputTensor.DownloadToArray();

        int bestIndex = 0;
        float highestProb = 0f;

        for (int i = 0; i < outputData.Length; i++)
        {
            if (outputData[i] > highestProb)
            {
                highestProb = outputData[i];
                bestIndex = i;
            }
        }

        string rawLabelString = (m_Labels != null && bestIndex < m_Labels.Length) ? m_Labels[bestIndex] : null;
        string finalDisplayName;

        if (!string.IsNullOrEmpty(rawLabelString))
        {
            string[] labelParts = rawLabelString.Split(',');
            finalDisplayName = labelParts[labelParts.Length - 1].Trim();
        }
        else
        {
            finalDisplayName = $"未知 (索引 {bestIndex})";
        }

        string resultStr = $"{finalDisplayName}\n信心: {(highestProb * 100):0.0}%";
        if (resultText != null) resultText.text = resultStr;

        // ==========================================
        // 🌟 修改後的邏輯：連續掃描與 UI 觸發
        if (highestProb >= detectionThreshold)
        {
            // 只有當「第一次看到」或「看到不一樣的植物」時，才呼叫管家彈出視窗
            if (uiManager != null && finalDisplayName != lastDetectedPlant)
            {
                lastDetectedPlant = finalDisplayName; // 把這個植物記在腦海裡
                uiManager.OnPlantDetected(finalDisplayName);
            }
            
            // ❌ 這裡原本的 OnCameraButtonPressed(); 已經被刪除
            // 所以相機畫面會繼續順暢地播放！
        }
        else if (highestProb < 0.3f) 
        {
            // 當鏡頭移開，或是信心很低 (小於 30%) 時，忘記剛剛的植物
            // 這樣下次重新把鏡頭對準同一個植物時，提示視窗才會重新彈出來
            lastDetectedPlant = ""; 
        }
        // ==========================================
    }

    void OnDisable()
    {
        StopAllCoroutines();
        isRequestingCamera = false;
        isScanning = false;
        m_Worker?.Dispose();
        m_WebCam?.Stop();
    }
}