# OCEAN 海洋生物圖鑑

Unity 海洋生物圖鑑 Android 測試版。

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

請先依 [iPad 測試說明](IPAD-TESTING.md) 設定 Unity 授權，再到 Actions 執行 `Build iPad IPA for local signing`。
目前流程尚未完成雲端建置或 iPad 實機驗證；現有 Android APK Release 不適用於 iPad。
