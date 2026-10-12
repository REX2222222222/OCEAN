# OCEAN 海洋生物圖鑑

Unity 海洋生物圖鑑 Android 與 iPad 測試版。

## 下載

請前往本儲存庫的 [Releases](https://github.com/REX2222222222/OCEAN/releases) 下載 `MarineEncyclopedia.apk`。

## 2026-10-11 更新

- 修正連續翻頁後右下角拖曳觸控區被頁面遮住，導致紙張捲曲效果失效的問題。
- 支援 ARM64 與 ARMv7。
- 最低 Android 5.1（API 22），目標 API 34。

此 APK 為使用除錯簽章的手機測試版本。

SHA-256：`A4890C652173006D88DB0E6A1A6DE98D2981EA6304CF8FB62AE8E4BD3F28FFD6`

## iPad 雲端測試

Unity 原始碼與素材已加入此儲存庫。手動雲端建置流程會輸出未簽章 IPA，需在 Windows 上以自己的 Apple 帳號簽章後安裝。

請依 [iPad 測試說明](IPAD-TESTING.md) 匯出 Xcode 專案，再到 Actions 執行 `Compile exported Xcode project for iPad`。
另保留 `Build iPad IPA for local signing` 流程，供能在雲端啟用 Unity 授權的帳號使用。
2026-10-11 已完成 [iPad 雲端建置](https://github.com/REX2222222222/OCEAN/actions/runs/38124833422) 與 IPA 檔案檢查。

下載 [iPad 測試版 IPA](https://github.com/REX2222222222/OCEAN/releases/download/ipad-test-2026-10-11/MarineEncyclopedia-unsigned.ipa)，再依測試說明簽章安裝。原版已由使用者安裝，並提供 iPad 上的圖鑑畫面；翻頁動畫與相機辨識仍需實機驗證。

## 2026-10-12 平板版面調整

- 圖鑑改用 1160×850 的參考版面，依螢幕比例等比例縮放。
- 放大分類按鈕，將上一頁／下一頁移到書本下方，增加書本可用寬度。
- 保留頁面原始尺寸及捲曲陰影幾何，避免縮放影響翻頁效果。
- Unity 模擬檢查通過 2048×1430、2048×1536、1920×1080 三種比例，分類與導覽按鈕均在畫面內，下一頁能正常前進。

新版使用 `ios-xcode-export-2026-10-12-tablet` 的 Xcode 匯出檔作為雲端輸入，IPA 仍需簽章安裝後進行 iPad 實機測試。

2026-10-12 的 [平板版面雲端建置](https://github.com/REX2222222222/OCEAN/actions/runs/38211506822) 已成功。
新版 IPA：[`ipad-tablet-layout-2026-10-12`](https://github.com/REX2222222222/OCEAN/releases/tag/ipad-tablet-layout-2026-10-12)。
