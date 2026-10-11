using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

[DisallowMultipleComponent]
public partial class BookController : MonoBehaviour
{
    public class SwipeDetector : MonoBehaviour, UnityEngine.EventSystems.IBeginDragHandler, UnityEngine.EventSystems.IDragHandler, UnityEngine.EventSystems.IEndDragHandler
    {
        private Vector2 startPos;
        public BookController bookController;

        public void OnBeginDrag(UnityEngine.EventSystems.PointerEventData eventData) { startPos = eventData.position; }
        public void OnDrag(UnityEngine.EventSystems.PointerEventData eventData) { } // ?對??? IDragHandler ?????? Begin/End
                public void OnEndDrag(UnityEngine.EventSystems.PointerEventData eventData)
        {
            Vector2 delta = eventData.position - startPos;
            if (Mathf.Abs(delta.x) > 50f && Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
            {
                if (delta.x < 0) bookController.FlipToNextPage(); 
                else bookController.FlipToPrevPage();
            }
        } 
    } 

    [System.Serializable]
    public class BookItem
    {
        public string name;
        public Sprite photo;
        public string description;
        public string category;
        public string scientificName;
        public string commonName;

        public BookItem(OrganismData org)
        {
            name = org.organismName;
            photo = org.photo;
            description = org.description;
            category = org.category;
        }

        public BookItem(PlantData plant)
        {
            name = plant.plantName;
            scientificName = plant.scientificName;
            commonName = plant.commonName;
            photo = plant.plantSprite;
            description = plant.description;
            category = "\u6d77\u6d0b\u690d\u7269";
        }
    }

    [Header("Book-Page Curl ?皜? (?賹??折?謆?撖?)")]
    public Book curlBook;
    public AutoFlip autoFlip;

    [Header("???????折????對????")]
    public Button prevButton;          
    public Button nextButton;          
    public TMP_Text pageIndicatorText; 

    [Header("????潘撩??塚?? (Detail Panel)")]
    public GameObject detailPanel;     
    public Image detailBigPhoto;       
    public TMP_Text detailTitleText;   
    public TMP_Text detailDescText;    
    public Button detailCloseBtn;      

    [Header("????殉??")]
    public TMP_FontAsset chineseFont;

    [Header("????")]
    public PlantDatabase plantDatabase;
    public List<OrganismData> allOrganisms = new List<OrganismData>();

    [Header("??????")]
    public string currentCategory = "\u6d77\u6d0b\u690d\u7269";
    public List<BookItem> currentDisplayItems = new List<BookItem>();

    [Header("?蹓賤????")]
    public int currentLeftPageIndex = 0;

    private void Start()
    {
        SetupReturnButton();
        SetupBookAnimation();
        LoadCategoryData(currentCategory);
        ApplyChineseFont();
        SetupCategoryButtons();
        SetupReadingFeatures();
        SetupNavigationVisibility();

        if (prevButton != null)
        {
            prevButton.onClick.RemoveAllListeners();
            prevButton.onClick.AddListener(FlipToPrevPage);
        }
        if (nextButton != null)
        {
            nextButton.onClick.RemoveAllListeners();
            nextButton.onClick.AddListener(FlipToNextPage);
        }

        if (curlBook != null)
        {
            // Enable pointer hits on the left page as well as the right page.
            curlBook.LeftNext.raycastTarget = true;
            SetupPageClick(curlBook.LeftNext.gameObject, () => OnPageClicked(currentLeftPageIndex - 1));
            SetupSwipe(curlBook.LeftNext.gameObject); // ?秋撒????折??
            
            curlBook.RightNext.raycastTarget = true;
            SetupPageClick(curlBook.RightNext.gameObject, () => OnPageClicked(currentLeftPageIndex));
            SetupSwipe(curlBook.RightNext.gameObject); // ?秋撒????折??

            // ???????蹓?蟡????????????????頦??綜竣???蹇?方?暸???
            Transform leftHotSpot = curlBook.transform.Find("LeftHotSpot");
            if (leftHotSpot != null) SetupPageClick(leftHotSpot.gameObject, () => OnPageClicked(currentLeftPageIndex - 1));

            Transform rightHotSpot = curlBook.transform.Find("RightHotSpot");
            if (rightHotSpot != null) SetupPageClick(rightHotSpot.gameObject, () => OnPageClicked(currentLeftPageIndex));
            
            // Keep both click areas above the page graphics.
            if (leftHotSpot != null) leftHotSpot.SetAsLastSibling();
            if (rightHotSpot != null) rightHotSpot.SetAsLastSibling();
        }

        if (detailCloseBtn != null)
        {
            detailCloseBtn.onClick.RemoveAllListeners();
            detailCloseBtn.onClick.AddListener(CloseDetailPanel);
        }

        if (detailPanel != null) detailPanel.SetActive(false);

        
        UpdateStaticPages();
    }

    private void SetupBookAnimation()
    {
        if (curlBook == null) curlBook = GetComponentInChildren<Book>(true);
        if (curlBook == null) return;
        if (autoFlip == null) autoFlip = curlBook.GetComponent<AutoFlip>();
        if (autoFlip == null) autoFlip = curlBook.gameObject.AddComponent<AutoFlip>();
        autoFlip.ControledBook = curlBook;
        autoFlip.AutoStartFlip = false;
        autoFlip.PageFlipTime = 0.7f;
        autoFlip.enabled = true;
        curlBook.enableShadowEffect = true;
        curlBook.PreparePaperPages();
        // Hide legacy static photos; the curling book renders both pages now.
        foreach (string obsoleteName in new[] { "LeftPage_Static", "RightPage_Static", "RightPage_Stati", "FlippingPage" })
        {
            Transform obsoletePage = transform.Find(obsoleteName);
            if (obsoletePage != null) obsoletePage.gameObject.SetActive(false);
        }
    }

    private void SetupNavigationVisibility()
    {
        if (prevButton == null) prevButton = transform.Find("Btn_PrevPage")?.GetComponent<Button>();
        if (nextButton == null) nextButton = transform.Find("Btn_NextPage")?.GetComponent<Button>();
        if (prevButton == null)
            prevButton = CreateButton("Btn_PrevPage", transform, new Vector2(-570f, 0f), new Vector2(130f, 65f), "上一頁");
        if (nextButton == null)
            nextButton = CreateButton("Btn_NextPage", transform, new Vector2(570f, 0f), new Vector2(130f, 65f), "下一頁");
        ConfigureNavigationButton(prevButton, "上一頁");
        ConfigureNavigationButton(nextButton, "下一頁");
        UpdateNavigationButtons();
    }

    private void ConfigureNavigationButton(Button button, string caption)
    {
        button.gameObject.SetActive(true);
        button.enabled = true;
        button.transform.SetAsLastSibling();
        RectTransform rect = button.GetComponent<RectTransform>();
        if (rect != null) rect.sizeDelta = new Vector2(130f, 65f);
        if (button.targetGraphic != null) button.targetGraphic.raycastTarget = true;
        TMP_Text label = button.GetComponentInChildren<TMP_Text>(true);
        if (label != null)
        {
            label.gameObject.SetActive(true);
            label.text = caption;
            if (chineseFont != null) label.font = chineseFont;
            label.fontSize = 22f;
            label.raycastTarget = false;
        }
        ColorBlock colors = button.colors;
        colors.disabledColor = new Color(0.6f, 0.6f, 0.6f, 0.65f);
        button.colors = colors;
    }

    private void UpdateNavigationButtons()
    {
        if (curlBook == null) return;
        bool busy = autoFlip != null && autoFlip.IsFlipping;
        if (prevButton != null) prevButton.interactable = !busy && curlBook.currentPage > 0;
        if (nextButton != null)
            nextButton.interactable = !busy && curlBook.currentPage < currentDisplayItems.Count - 1;
    }

    private void SetupReturnButton()
    {
        Canvas rootCanvas = GetComponentInParent<Canvas>();
        if (rootCanvas == null) return;

        Transform returnTransform = rootCanvas.transform.Find("Btn_Return");
        Button returnButton = returnTransform != null ? returnTransform.GetComponent<Button>() : null;
        if (returnButton == null)
        {
            Debug.LogWarning("[BookController] Btn_Return was not found.", this);
            return;
        }

        // Replace stale Inspector events when the open scene has not been reloaded.
        for (int i = 0; i < returnButton.onClick.GetPersistentEventCount(); i++)
            returnButton.onClick.SetPersistentListenerState(i, UnityEngine.Events.UnityEventCallState.Off);
        returnButton.onClick.RemoveListener(ReturnToMainMenu);
        returnButton.onClick.AddListener(ReturnToMainMenu);
        returnButton.interactable = true;
        if (returnButton.targetGraphic != null) returnButton.targetGraphic.raycastTarget = true;

        // Keep navigation above the book graphics, with the detail modal on top.
        returnTransform.SetAsLastSibling();
        if (detailPanel != null && detailPanel.transform.parent == rootCanvas.transform)
            detailPanel.transform.SetAsLastSibling();
    }

    public void ReturnToMainMenu()
    {
        SaveReadingPosition();
        PlayerPrefs.Save();
        Debug.Log("[BookController] Return clicked: loading main menu.", this);
        UnityEngine.SceneManagement.SceneManager.LoadScene("main menu");
    }

    private void Update()
    {
        UpdateNavigationButtons();
        if (curlBook != null)
        {
            if (currentLeftPageIndex != curlBook.currentPage)
            {
                currentLeftPageIndex = curlBook.currentPage;
                SaveReadingPosition();
                UpdateStaticPages();
            }
        }
    }

    public void SwitchCategory(string categoryName)
    {
        if (autoFlip != null) autoFlip.CancelFlip();
        SaveReadingPosition();
        currentCategory = categoryName;
        currentLeftPageIndex = 0;
        LoadCategoryData(categoryName);
        CloseDetailPanel();
        UpdateStaticPages();
        Debug.Log($"<color=green>[BookController] ???祕蝧餅憭? (Book-Page Curl) ?芸??函蔡摰?嚗?/color>");
    }

        private void LoadCategoryData(string categoryName)
    {
        currentDisplayItems.Clear();

        if (categoryName == "\u6d77\u6d0b\u690d\u7269" && plantDatabase != null && plantDatabase.allPlants != null && plantDatabase.allPlants.Count > 0)
        {
            foreach (var plant in plantDatabase.allPlants)
            {
                if (plant != null) currentDisplayItems.Add(new BookItem(plant));
            }
        }

        if (allOrganisms != null && allOrganisms.Count > 0)
        {
            foreach (var org in allOrganisms)
            {
                if (org != null && org.category == categoryName)
                {
                    if (categoryName == "\u6d77\u6d0b\u690d\u7269" && currentDisplayItems.Exists(x => x.name.Contains(org.organismName)))
                        continue;
                    currentDisplayItems.Add(new BookItem(org));
                }
            }
        }

        if (curlBook != null)
        {
            if (curlBook.bookPages == null || curlBook.bookPages.Length != currentDisplayItems.Count)
            {
                curlBook.bookPages = new Sprite[currentDisplayItems.Count];
            }
            for (int i = 0; i < currentDisplayItems.Count; i++)
            {
                curlBook.bookPages[i] = currentDisplayItems[i].photo;
            }
            curlBook.currentPage = ReadSavedPage(categoryName, currentDisplayItems.Count);
            currentLeftPageIndex = curlBook.currentPage;
            curlBook.UpdateSprites();
        }
    }

    private void ApplyChineseFont()
    {
        if (chineseFont == null) return;
        if (detailTitleText != null) detailTitleText.font = chineseFont;
        if (detailDescText != null) detailDescText.font = chineseFont;
        if (pageIndicatorText != null) pageIndicatorText.font = chineseFont;
        if (detailCloseBtn != null)
        {
            TMP_Text closeText = detailCloseBtn.GetComponentInChildren<TMP_Text>();
            if (closeText != null) closeText.font = chineseFont;
        }
    }

    private void SetupPageClick(GameObject pageObj, UnityEngine.Events.UnityAction action)
    {
        if (pageObj == null) return;
        Button btn = pageObj.GetComponent<Button>();
        if (btn == null) btn = pageObj.AddComponent<Button>();
        btn.transition = Selectable.Transition.None;
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(action);
    }

    private void SetupSwipe(GameObject pageObj)
    {
        if (pageObj == null) return;
        SwipeDetector sd = pageObj.GetComponent<SwipeDetector>();
        if (sd == null) sd = pageObj.AddComponent<SwipeDetector>();
        sd.bookController = this;
    }

    public void OnPageClicked(int pageIndex) { Debug.Log("Page clicked: " + pageIndex);
        if (autoFlip != null && autoFlip.IsFlipping) return;
        if (pageIndex < 0 || pageIndex >= currentDisplayItems.Count) return;

        BookItem item = currentDisplayItems[pageIndex];
        if (item == null || item.photo == null) return;

        if (detailBigPhoto != null)
        {
            detailBigPhoto.sprite = item.photo;
            detailBigPhoto.preserveAspect = true;
        }

        if (detailTitleText != null) detailTitleText.text = item.name;
        if (detailDescText != null) detailDescText.text = BookDetailFormatter.Format(item.description, item.scientificName, item.commonName);


        if (detailPanel != null)
        {
            detailPanel.SetActive(true);
            ResetDetailScroll();
            detailPanel.transform.localScale = Vector3.one * 0.88f;
            StopAllCoroutines();
            StartCoroutine(PopUpAnimation(detailPanel.transform));
        }
    }

    public void CloseDetailPanel()
    {
        if (detailPanel != null) detailPanel.SetActive(false);
    }

    private IEnumerator PopUpAnimation(Transform target)
    {
        float timer = 0f;
        while (timer < 0.15f)
        {
            timer += Time.deltaTime;
            float t = timer / 0.15f;
            target.localScale = Vector3.Lerp(Vector3.one * 0.88f, Vector3.one, Mathf.SmoothStep(0, 1, t));
            yield return null;
        }
        target.localScale = Vector3.one;
    }

    [ContextMenu("下一頁")]
    public void FlipToNextPage()
    {
        if (curlBook != null && autoFlip != null)
        {
            if (curlBook.currentPage >= currentDisplayItems.Count - 1) return;
            autoFlip.FlipRightPage();
            UpdateNavigationButtons();
        }
    }

    [ContextMenu("上一頁")]
    public void FlipToPrevPage()
    {
        if (curlBook != null && autoFlip != null)
        {
            autoFlip.FlipLeftPage();
            UpdateNavigationButtons();
        }
    }

    public void UpdateStaticPages()
    {
        UpdateCategorySelection();
        int rightIndex = curlBook != null ? curlBook.currentPage : currentLeftPageIndex;
        if (leftItemLabel != null) leftItemLabel.text = GetPageItemLabel(rightIndex - 1);
        if (rightItemLabel != null) rightItemLabel.text = GetPageItemLabel(rightIndex);
        if (pageIndicatorText != null)
        {
            pageIndicatorText.text = currentDisplayItems.Count == 0
                ? currentCategory + "（目前沒有資料）"
                : currentCategory + "  ·  " + currentDisplayItems.Count + " 種";
            pageIndicatorText.rectTransform.sizeDelta = new Vector2(850f, 38f);
            pageIndicatorText.rectTransform.anchoredPosition = new Vector2(0f, -355f);
            pageIndicatorText.raycastTarget = false;
            pageIndicatorText.fontSize = 20f;
        }
    }
    private string GetPageItemLabel(int itemIndex)
    {
        if (itemIndex < 0 || itemIndex >= currentDisplayItems.Count) return string.Empty;
        BookItem item = currentDisplayItems[itemIndex];
        if (item == null) return string.Empty;
        return $"{item.name}  [ {itemIndex + 1} / {currentDisplayItems.Count} ]";
    }

    private void SetupCategoryButtons()
    {
        Canvas rootCanvas = GetComponentInParent<Canvas>();
        if (rootCanvas == null) return;
        foreach (Button button in rootCanvas.GetComponentsInChildren<Button>(true))
        {
            TMP_Text label = button.GetComponentInChildren<TMP_Text>();
            string text = label != null ? label.text.Trim() : button.name;
            foreach (string category in new[] { "海洋植物", "沿海動物", "腔腸動物", "節肢動物", "棘皮動物", "軟體動物" })
            {
                if (!text.Contains(category) && !button.name.Contains(category)
                    && !(category == "海洋植物" && button.name == "Btn_OceanPlant")) continue;
                string selectedCategory = category;
                if (category == "沿海動物" && !allOrganisms.Exists(org => org != null && org.category == category)
                    && allOrganisms.Exists(org => org != null && org.category == "腔腸動物"))
                    selectedCategory = "腔腸動物";
                for (int i = 0; i < button.onClick.GetPersistentEventCount(); i++)
                    button.onClick.SetPersistentListenerState(i, UnityEngine.Events.UnityEventCallState.Off);
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => SwitchCategory(selectedCategory));
                categoryButtons[button] = selectedCategory;
                originalCategoryColors[button] = button.colors;
                if (label != null) originalCategoryTextColors[button] = label.color;
                break;
            }
        }
    }



#if UNITY_EDITOR
    [InitializeOnLoadMethod]
    private static void AutoCleanOnRecompile() {
        EditorApplication.delayCall += () => {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            BookController book = FindObjectOfType<BookController>();
            if (book != null) book.AutoSetupBook();
        };
    }

    [ContextMenu("Auto Setup Book")]
    public void AutoSetupBook() {
        Undo.RecordObject(gameObject, "Auto Setup Complete Book");
        if (chineseFont == null) {
            string fontPath = AssetDatabase.GUIDToAssetPath("ca921a4fc3a6ad74b91a8ea5229ac0b7");
            if (!string.IsNullOrEmpty(fontPath)) chineseFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(fontPath);
        }
        if (plantDatabase == null) plantDatabase = AssetDatabase.LoadAssetAtPath<PlantDatabase>("Assets/MyPlantDatabase.asset");
        allOrganisms.Clear();
        string[] orgGuids = AssetDatabase.FindAssets("t:OrganismData", new string[] { "Assets/Data_Organisms" });
        foreach (string guid in orgGuids) {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            OrganismData org = AssetDatabase.LoadAssetAtPath<OrganismData>(path);
            if (org != null && !allOrganisms.Contains(org)) allOrganisms.Add(org);
        }
        Canvas rootCanvas = GetComponentInParent<Canvas>();
        if (rootCanvas != null) {
            Transform bgTrans = rootCanvas.transform.Find("SceneBackground");
            if (bgTrans == null) {
                GameObject bgGO = new GameObject("SceneBackground", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                bgGO.transform.SetParent(rootCanvas.transform, false);
                bgGO.transform.SetAsFirstSibling(); 
                bgTrans = bgGO.transform;
                RectTransform rt = bgGO.GetComponent<RectTransform>();
                rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
                rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
            }
            Image bgImg = bgTrans.GetComponent<Image>();
            Sprite bgSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/img/background.png");
            if (bgSprite != null) bgImg.sprite = bgSprite; else bgImg.color = new Color(0.85f, 0.95f, 1f); bgImg.raycastTarget = false;
            Transform bookBgTrans = rootCanvas.transform.Find("BookBackground");
            if (bookBgTrans == null) {
                GameObject bookBgGO = new GameObject("BookBackground", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                bookBgGO.transform.SetParent(rootCanvas.transform, false);
                bookBgTrans = bookBgGO.transform;
                bookBgTrans.SetSiblingIndex(1); 
                RectTransform rt = bookBgGO.GetComponent<RectTransform>();
                rt.anchoredPosition = new Vector2(0, 0); 
                rt.sizeDelta = new Vector2(950, 600); 
            }
            Image bookBgImg = bookBgTrans.GetComponent<Image>();
            Sprite bookSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/img/book.png");
            if (bookSprite != null) bookBgImg.sprite = bookSprite; else bookBgImg.color = new Color(0.89f, 0.83f, 0.77f, 1f); bookBgImg.raycastTarget = false;
            Transform t1 = transform.Find("LeftPage_Static"); if(t1) Undo.DestroyObjectImmediate(t1.gameObject);
            Transform t2 = transform.Find("RightPage_Static"); if(t2) Undo.DestroyObjectImmediate(t2.gameObject);
            Transform t3 = transform.Find("FlippingPage"); if(t3) Undo.DestroyObjectImmediate(t3.gameObject);
        }
        if (curlBook == null) {
            Book existingBook = GetComponentInChildren<Book>(true);
            if (existingBook != null) {
                curlBook = existingBook;
                autoFlip = curlBook.GetComponent<AutoFlip>();
                RectTransform rt = curlBook.GetComponent<RectTransform>();
                rt.anchoredPosition = new Vector2(0, 0); 
                rt.sizeDelta = new Vector2(950, 600); 
            } else {
                GameObject bookPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Book-Page Curl/Example/prefabs/Book.prefab");
                if (bookPrefab != null) {
                    GameObject bookGO = PrefabUtility.InstantiatePrefab(bookPrefab, transform) as GameObject;
                    curlBook = bookGO.GetComponent<Book>();
                    autoFlip = bookGO.GetComponent<AutoFlip>();
                    if (autoFlip == null) autoFlip = bookGO.AddComponent<AutoFlip>();
                    autoFlip.AutoStartFlip = false; 
                    RectTransform rt = bookGO.GetComponent<RectTransform>();
                    rt.anchoredPosition = new Vector2(0, 0);
                    rt.sizeDelta = new Vector2(950, 600);
                    bookGO.transform.SetSiblingIndex(2);
                }
            }
            Sprite bookSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/img/book.png");
            if (bookSprite != null) curlBook.background = bookSprite;
        }
        Transform prevBtnTrans = transform.Find("Btn_PrevPage");
        if (prevBtnTrans == null) {
            GameObject prevGO = new GameObject("Btn_PrevPage", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            prevGO.transform.SetParent(transform, false);
            prevBtnTrans = prevGO.transform;
            RectTransform rt = prevGO.GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(-570, 0); 
            rt.sizeDelta = new Vector2(100, 65);
            prevGO.GetComponent<Image>().color = new Color(0.18f, 0.32f, 0.48f, 0.9f);
            GameObject textGO = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            textGO.transform.SetParent(prevGO.transform, false);
            TextMeshProUGUI t = textGO.AddComponent<TextMeshProUGUI>();
            t.text = "Previous"; t.fontSize = 24; t.alignment = TextAlignmentOptions.Center; t.color = Color.white;
            if (chineseFont != null) t.font = chineseFont;
            RectTransform trt = textGO.GetComponent<RectTransform>();
            trt.anchorMin = Vector2.zero; trt.anchorMax = Vector2.one;
            trt.offsetMin = Vector2.zero; trt.offsetMax = Vector2.zero;
            prevBtnTrans.SetAsLastSibling();
        }
        prevButton = prevBtnTrans.GetComponent<Button>();
        Transform nextBtnTrans = transform.Find("Btn_NextPage");
        if (nextBtnTrans == null) {
            GameObject nextGO = new GameObject("Btn_NextPage", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            nextGO.transform.SetParent(transform, false);
            nextBtnTrans = nextGO.transform;
            RectTransform rt = nextGO.GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(570, 0); 
            rt.sizeDelta = new Vector2(100, 65);
            nextGO.GetComponent<Image>().color = new Color(0.18f, 0.32f, 0.48f, 0.9f);
            GameObject textGO = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            textGO.transform.SetParent(nextGO.transform, false);
            TextMeshProUGUI t = textGO.AddComponent<TextMeshProUGUI>();
            t.text = "Next"; t.fontSize = 24; t.alignment = TextAlignmentOptions.Center; t.color = Color.white;
            if (chineseFont != null) t.font = chineseFont;
            RectTransform trt = textGO.GetComponent<RectTransform>();
            trt.anchorMin = Vector2.zero; trt.anchorMax = Vector2.one;
            trt.offsetMin = Vector2.zero; trt.offsetMax = Vector2.zero;
            nextBtnTrans.SetAsLastSibling();
        }
        nextButton = nextBtnTrans.GetComponent<Button>();
        Transform pageIndTrans = transform.Find("PageIndicatorText");
        if (pageIndTrans == null) {
            GameObject indGO = new GameObject("PageIndicatorText", typeof(RectTransform), typeof(TextMeshProUGUI));
            indGO.transform.SetParent(transform, false);
            pageIndTrans = indGO.transform;
            RectTransform rt = indGO.GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(0, -350); 
            rt.sizeDelta = new Vector2(400, 45);
            TextMeshProUGUI tmp = indGO.AddComponent<TextMeshProUGUI>();
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontSize = 22; tmp.fontStyle = FontStyles.Bold; tmp.color = new Color(0.15f, 0.25f, 0.35f);
        }
        pageIndicatorText = pageIndTrans.GetComponent<TextMeshProUGUI>();
        if (chineseFont != null) pageIndicatorText.font = chineseFont;
        if (rootCanvas != null) {
            Transform existingModal = rootCanvas.transform.Find("PlantDetailPanel");
            if (existingModal != null) {
                detailPanel = existingModal.gameObject;
                detailBigPhoto = existingModal.Find("CardBody/BigPhoto")?.GetComponent<Image>();
                detailTitleText = existingModal.Find("CardBody/TitleText")?.GetComponent<TextMeshProUGUI>();
                detailDescText = existingModal.Find("CardBody/DescText")?.GetComponent<TextMeshProUGUI>();
                detailCloseBtn = existingModal.Find("CardBody/Btn_Close")?.GetComponent<Button>();
            }
        }
        if (detailPanel != null) detailPanel.SetActive(false);
        LoadCategoryData(currentCategory);
        UpdateStaticPages();
        EditorUtility.SetDirty(this);
        EditorUtility.SetDirty(gameObject);
        Debug.Log("[BookController] Auto Setup Complete!");
    }
#endif
}
 


 
 // Force recompile

 









public partial class BookController
{
    private TMP_Text leftItemLabel;
    private TMP_Text rightItemLabel;
    private TMP_InputField searchInput;
    private GameObject searchResultsPanel;
    private RectTransform searchResultsContent;
    private ScrollRect detailScroll;
    private readonly Dictionary<Button, string> categoryButtons = new Dictionary<Button, string>();
    private readonly Dictionary<Button, ColorBlock> originalCategoryColors = new Dictionary<Button, ColorBlock>();
    private readonly Dictionary<Button, Color> originalCategoryTextColors = new Dictionary<Button, Color>();
    private readonly List<BookItem> searchableItems = new List<BookItem>();
    private const string ReadingKey = "Encyclopedia.ReadingPage.";

    private void SetupReadingFeatures()
    {
        leftItemLabel = CreateLabel("LeftItemLabel", transform, new Vector2(-212.5f, -302f), new Vector2(390f, 64f), "", 22f);
        rightItemLabel = CreateLabel("RightItemLabel", transform, new Vector2(212.5f, -302f), new Vector2(390f, 64f), "", 22f);
        leftItemLabel.enableAutoSizing = rightItemLabel.enableAutoSizing = true;
        leftItemLabel.fontSizeMin = rightItemLabel.fontSizeMin = 16f;
        leftItemLabel.fontSizeMax = rightItemLabel.fontSizeMax = 22f;
        SetupDetailLayout();
        SetupSearch();
    }

    private static RectTransform CreateRect(string name, Transform parent, Vector2 position, Vector2 size)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.layer = parent.gameObject.layer;
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        SetRect(rect, position, size);
        return rect;
    }

    private static void SetRect(RectTransform rect, Vector2 position, Vector2 size)
    {
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
    }

    private static void Stretch(RectTransform rect, float padding = 0f)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = new Vector2(padding, padding);
        rect.offsetMax = new Vector2(-padding, -padding);
    }

    private TMP_Text CreateLabel(string name, Transform parent, Vector2 position, Vector2 size, string text, float fontSize)
    {
        RectTransform rect = CreateRect(name, parent, position, size);
        TextMeshProUGUI label = rect.gameObject.AddComponent<TextMeshProUGUI>();
        if (chineseFont != null) label.font = chineseFont;
        label.text = text;
        label.fontSize = fontSize;
        label.color = new Color(0.12f, 0.22f, 0.29f);
        label.alignment = TextAlignmentOptions.Center;
        label.raycastTarget = false;
        return label;
    }

    private Button CreateButton(string name, Transform parent, Vector2 position, Vector2 size, string text)
    {
        RectTransform rect = CreateRect(name, parent, position, size);
        Image image = rect.gameObject.AddComponent<Image>();
        image.color = new Color(0.90f, 0.95f, 0.96f);
        Button button = rect.gameObject.AddComponent<Button>();
        button.targetGraphic = image;
        TMP_Text label = CreateLabel("Label", rect, Vector2.zero, size, text, 20f);
        Stretch(label.rectTransform, 8f);
        return button;
    }

    private void UpdateCategorySelection()
    {
        foreach (KeyValuePair<Button, string> pair in categoryButtons)
        {
            if (pair.Key == null) continue;
            bool selected = pair.Value == currentCategory;
            ColorBlock colors = originalCategoryColors[pair.Key];
            if (selected)
            {
                colors.normalColor = new Color(0.18f, 0.40f, 0.47f);
                colors.highlightedColor = new Color(0.24f, 0.48f, 0.54f);
                colors.selectedColor = colors.normalColor;
                colors.pressedColor = new Color(0.12f, 0.30f, 0.37f);
            }
            pair.Key.colors = colors;
            TMP_Text label = pair.Key.GetComponentInChildren<TMP_Text>();
            if (label != null) label.color = selected ? Color.white : originalCategoryTextColors[pair.Key];
        }
    }

    private void SaveReadingPosition()
    {
        if (curlBook == null || string.IsNullOrEmpty(currentCategory)) return;
        PlayerPrefs.SetInt(ReadingKey + currentCategory, curlBook.currentPage);
    }

    private static int ReadSavedPage(string category, int itemCount)
    {
        int lastSpread = itemCount > 0 ? (itemCount / 2) * 2 : 0;
        int savedPage = Mathf.Clamp(PlayerPrefs.GetInt(ReadingKey + category, 0), 0, lastSpread);
        return savedPage - savedPage % 2;
    }

    private void OnApplicationPause(bool paused)
    {
        if (!paused) return;
        SaveReadingPosition();
        PlayerPrefs.Save();
    }

    private void OnApplicationQuit()
    {
        SaveReadingPosition();
        PlayerPrefs.Save();
    }

    private void SetupDetailLayout()
    {
        if (detailPanel == null || detailDescText == null) return;
        RectTransform body = detailPanel.transform.Find("CardBody") as RectTransform;
        if (body == null) return;
        SetRect(body, Vector2.zero, new Vector2(980f, 620f));
        if (detailTitleText != null)
        {
            SetRect(detailTitleText.rectTransform, new Vector2(-15f, 255f), new Vector2(810f, 65f));
            detailTitleText.alignment = TextAlignmentOptions.MidlineLeft;
            detailTitleText.enableAutoSizing = true;
            detailTitleText.fontSizeMin = 22f;
            detailTitleText.fontSizeMax = 34f;
            detailTitleText.raycastTarget = false;
            detailTitleText.color = new Color(0.12f, 0.22f, 0.29f);
        }
        if (detailBigPhoto != null)
        {
            SetRect(detailBigPhoto.rectTransform, new Vector2(-290f, -25f), new Vector2(340f, 440f));
            detailBigPhoto.raycastTarget = false;
        }
        if (detailCloseBtn != null)
        {
            SetRect(detailCloseBtn.GetComponent<RectTransform>(), new Vector2(445f, 260f), new Vector2(55f, 48f));
            TMP_Text closeLabel = detailCloseBtn.GetComponentInChildren<TMP_Text>();
            if (closeLabel != null) closeLabel.text = "X";
        }
        RectTransform scrollRoot = CreateRect("DetailScroll", body, new Vector2(175f, -30f), new Vector2(520f, 460f));
        Image background = scrollRoot.gameObject.AddComponent<Image>();
        background.color = new Color(0.96f, 0.98f, 0.97f);
        detailScroll = scrollRoot.gameObject.AddComponent<ScrollRect>();
        detailScroll.horizontal = false;
        detailScroll.movementType = ScrollRect.MovementType.Clamped;
        detailScroll.scrollSensitivity = 30f;
        RectTransform viewport = CreateRect("Viewport", scrollRoot, Vector2.zero, Vector2.zero);
        Stretch(viewport, 14f);
        viewport.gameObject.AddComponent<RectMask2D>();
        RectTransform content = detailDescText.rectTransform;
        content.SetParent(viewport, false);
        content.anchorMin = new Vector2(0f, 1f);
        content.anchorMax = Vector2.one;
        content.pivot = new Vector2(0.5f, 1f);
        content.anchoredPosition = Vector2.zero;
        content.sizeDelta = new Vector2(0f, 460f);
        detailDescText.color = new Color(0.12f, 0.22f, 0.29f);
        detailDescText.fontSize = 23f;
        detailDescText.enableAutoSizing = false;
        detailDescText.alignment = TextAlignmentOptions.TopLeft;
        detailDescText.enableWordWrapping = true;
        detailDescText.overflowMode = TextOverflowModes.Overflow;
        detailDescText.raycastTarget = false;
        detailDescText.lineSpacing = 6f;
        detailDescText.richText = true;
        ContentSizeFitter fitter = content.gameObject.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        detailScroll.viewport = viewport;
        detailScroll.content = content;
        CreateLabel("ScrollHint", body, new Vector2(175f, -280f), new Vector2(520f, 28f), "上下滑動或使用滾輪閱讀介紹", 16f);
        if (detailCloseBtn != null) detailCloseBtn.transform.SetAsLastSibling();
    }

    private void ResetDetailScroll()
    {
        if (detailScroll == null) return;
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(detailScroll.content);
        detailScroll.StopMovement();
        detailScroll.verticalNormalizedPosition = 1f;
    }

    private void SetupSearch()
    {
        RectTransform inputRoot = CreateRect("NameSearch", transform, new Vector2(-40f, 312f), new Vector2(450f, 44f));
        Image background = inputRoot.gameObject.AddComponent<Image>();
        background.color = Color.white;
        searchInput = inputRoot.gameObject.AddComponent<TMP_InputField>();
        searchInput.targetGraphic = background;
        RectTransform viewport = CreateRect("TextArea", inputRoot, Vector2.zero, Vector2.zero);
        Stretch(viewport, 9f);
        viewport.gameObject.AddComponent<RectMask2D>();
        TMP_Text inputText = CreateLabel("InputText", viewport, Vector2.zero, Vector2.zero, "", 22f);
        Stretch(inputText.rectTransform);
        inputText.alignment = TextAlignmentOptions.MidlineLeft;
        TMP_Text placeholder = CreateLabel("Placeholder", viewport, Vector2.zero, Vector2.zero, "搜尋動物或植物名稱", 22f);
        Stretch(placeholder.rectTransform);
        placeholder.alignment = TextAlignmentOptions.MidlineLeft;
        placeholder.color = new Color(0.40f, 0.47f, 0.48f);
        searchInput.textViewport = viewport;
        searchInput.textComponent = (TextMeshProUGUI)inputText;
        searchInput.placeholder = placeholder;
        searchInput.richText = false;
        searchInput.lineType = TMP_InputField.LineType.SingleLine;
        searchInput.onValueChanged.AddListener(UpdateSearchResults);
        Button clear = CreateButton("ClearSearch", transform, new Vector2(235f, 312f), new Vector2(80f, 44f), "清除");
        clear.onClick.AddListener(() => searchInput.text = string.Empty);

        RectTransform panel = CreateRect("SearchResults", transform, new Vector2(0f, 283f), new Vector2(650f, 252f));
        panel.pivot = new Vector2(0.5f, 1f);
        searchResultsPanel = panel.gameObject;
        Image panelBackground = panel.gameObject.AddComponent<Image>();
        panelBackground.color = new Color(0.98f, 0.99f, 0.97f);
        ScrollRect scroll = panel.gameObject.AddComponent<ScrollRect>();
        scroll.horizontal = false;
        scroll.scrollSensitivity = 28f;
        scroll.movementType = ScrollRect.MovementType.Clamped;
        RectTransform resultViewport = CreateRect("Viewport", panel, Vector2.zero, Vector2.zero);
        Stretch(resultViewport, 8f);
        resultViewport.gameObject.AddComponent<RectMask2D>();
        searchResultsContent = CreateRect("Content", resultViewport, Vector2.zero, Vector2.zero);
        searchResultsContent.anchorMin = new Vector2(0f, 1f);
        searchResultsContent.anchorMax = Vector2.one;
        searchResultsContent.pivot = new Vector2(0.5f, 1f);
        VerticalLayoutGroup layout = searchResultsContent.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 5f;
        layout.childControlWidth = layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;
        ContentSizeFitter fitter = searchResultsContent.gameObject.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        scroll.viewport = resultViewport;
        scroll.content = searchResultsContent;
        searchResultsPanel.SetActive(false);
        BuildSearchIndex();
    }

    private void BuildSearchIndex()
    {
        searchableItems.Clear();
        HashSet<string> seen = new HashSet<string>();
        if (plantDatabase != null && plantDatabase.allPlants != null)
            foreach (PlantData plant in plantDatabase.allPlants)
                if (plant != null) AddSearchItem(new BookItem(plant), seen);
        if (allOrganisms != null)
            foreach (OrganismData organism in allOrganisms)
                if (organism != null) AddSearchItem(new BookItem(organism), seen);
    }

    private void AddSearchItem(BookItem item, HashSet<string> seen)
    {
        if (string.IsNullOrWhiteSpace(item.name)) return;
        if (seen.Add(item.category + "\n" + item.name)) searchableItems.Add(item);
    }

    private void UpdateSearchResults(string query)
    {
        foreach (Transform child in searchResultsContent)
        {
            child.gameObject.SetActive(false);
            Destroy(child.gameObject);
        }
        query = query.Trim();
        searchResultsPanel.SetActive(query.Length > 0);
        if (query.Length == 0) return;
        int matches = 0;
        foreach (BookItem candidate in searchableItems)
        {
            if (candidate.name.IndexOf(query, System.StringComparison.OrdinalIgnoreCase) < 0
                && (candidate.scientificName ?? "").IndexOf(query, System.StringComparison.OrdinalIgnoreCase) < 0) continue;
            BookItem result = candidate;
            Button row = CreateButton("Result", searchResultsContent, Vector2.zero, new Vector2(630f, 48f), candidate.name + "  ·  " + candidate.category);
            LayoutElement rowLayout = row.gameObject.AddComponent<LayoutElement>();
            rowLayout.preferredHeight = 48f;
            row.onClick.AddListener(() => OpenSearchResult(result));
            matches++;
        }
        if (matches == 0)
        {
            TMP_Text empty = CreateLabel("NoResults", searchResultsContent, Vector2.zero, new Vector2(630f, 50f), "找不到符合的名稱，請試試其他關鍵字", 20f);
            empty.gameObject.AddComponent<LayoutElement>().preferredHeight = 50f;
        }
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(searchResultsContent);
        searchResultsPanel.GetComponent<ScrollRect>().verticalNormalizedPosition = 1f;
    }

    private void OpenSearchResult(BookItem result)
    {
        SwitchCategory(result.category);
        int itemIndex = currentDisplayItems.FindIndex(item => item.name == result.name);
        if (itemIndex < 0) return;
        int spread = itemIndex % 2 == 0 ? itemIndex : itemIndex + 1;
        if (curlBook != null)
        {
            curlBook.currentPage = spread;
            curlBook.UpdateSprites();
        }
        currentLeftPageIndex = spread;
        SaveReadingPosition();
        searchInput.SetTextWithoutNotify(string.Empty);
        searchInput.DeactivateInputField();
        searchResultsPanel.SetActive(false);
        UpdateStaticPages();
        OnPageClicked(itemIndex);
    }
}

public static class BookDetailFormatter
{
    public static string Format(string description, string scientificName = null, string commonName = null)
    {
        description = (description ?? "").Replace("\r\n", "\n").Trim();
        var markers = System.Text.RegularExpressions.Regex.Matches(description, @"(科\s*別|學\s*名|俗\s*名|簡\s*介)\s*[：:]\s*");
        var output = new System.Text.StringBuilder();
        if (markers.Count > 0)
        {
            if (markers[0].Index > 0) output.Append(description.Substring(0, markers[0].Index).Trim()).Append("\n\n");
            for (int i = 0; i < markers.Count; i++)
            {
                var marker = markers[i];
                string heading = System.Text.RegularExpressions.Regex.Replace(marker.Groups[1].Value, @"\s+", "");
                int start = marker.Index + marker.Length;
                int end = i + 1 < markers.Count ? markers[i + 1].Index : description.Length;
                string value = description.Substring(start, end - start).Trim();
                output.Append("<b><color=#244D59>").Append(heading).Append("</color></b>\n")
                    .Append(value.Length > 0 ? value : "未提供").Append("\n\n");
            }
        }
        else
        {
            if (!string.IsNullOrWhiteSpace(scientificName)) output.Append("<b>學名</b>\n").Append(scientificName).Append("\n\n");
            if (!string.IsNullOrWhiteSpace(commonName)) output.Append("<b>俗名</b>\n").Append(commonName).Append("\n\n");
            output.Append("<b>介紹</b>\n").Append(description.Length > 0 ? description : "目前尚未提供介紹。");
        }
        return output.ToString().Trim();
    }
}