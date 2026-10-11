# iPad 雲端建置與免費帳號測試

這個流程使用 Unity 2022.3.62f3，並在 GitHub 的 macOS 主機編譯 Xcode 專案，輸出未簽章 IPA。
IPA 必須先在你的 Windows 電腦上簽章，才能裝到 iPad；不是 TestFlight 發布版。
2026-10-11 已成功完成雲端編譯與 IPA 完整性、ARM64、iPad 支援及相機權限描述檢查；尚未完成 iPad 實機驗證。

下載：[MarineEncyclopedia-unsigned.ipa](https://github.com/REX2222222222/OCEAN/releases/download/ipad-test-2026-10-11/MarineEncyclopedia-unsigned.ipa)。
建置：[成功紀錄](https://github.com/REX2222222222/OCEAN/actions/runs/38124833422)。
SHA-256：`fb5ea67483b8040b626a68579694741f049e8e5f0333f328abca4d20b9ee52b2`。

## 使用本機匯出的 Xcode 專案

當雲端 Unity 登入失敗時，可以由已啟用的本機 Unity 匯出，再交給雲端 Xcode 編譯。

1. 安裝 Unity 2022.3.62f3 的 iOS Build Support。
2. 用 `CloudIOSExport.Build` 匯出啟用的場景與 iOS Xcode 專案。
3. 將匯出資料夾內的檔案壓縮為 `OCEAN-Xcode.zip`，並產生 `SHA256SUMS.txt`。
4. 將兩個檔案上傳至本儲存庫 `ios-xcode-export-2026-10-11` Release。
5. Actions → Compile exported Xcode project for iPad → Run workflow。

這個工作流程不讀取 Unity 帳號 Secrets。產出的 IPA 仍需要以下述方式簽章後才能安裝。
請重新匯出並更新 Release 資產，才能在此流程中包含後續的 Unity 專案變更。

## 直接在雲端執行 Unity（另一個流程）

在本儲存庫 Settings → Secrets and variables → Actions → New repository secret 新增：

- `UNITY_LICENSE`：有效的 Unity Personal `.ulf` 授權內容。
- `UNITY_EMAIL`：該 Unity 帳號的電子郵件。
- `UNITY_PASSWORD`：該 Unity 帳號的密碼。

請直接填入 GitHub Secrets，不要提交到原始碼或貼在聊天中。
本機常見授權位置為 `C:\ProgramData\Unity\Unity_lic.ulf`。
如果不存在，需依 GameCI 啟用文件處理，不能僅將空值或 Windows 序號當成授權。
不要為此關閉帳號的安全保護；授權／登入不相容時應檢查錯誤並改用合適的建置服務。

官方流程：https://game.ci/docs/github/activation/

## 執行建置

1. Actions → Build iPad IPA for local signing → Run workflow。
2. 成功後下載 `iPad-IPA-requires-local-signing` artifact，解壓取得 IPA。
3. 失敗時查看失敗步驟和 `iPad-build-logs`，不要將失敗當成已產生 IPA。

此工作流程僅手動觸發；不會因每次提交自動啟動。
私人儲存庫的 macOS 建置會使用帳號額度，超額可能收費；本專案目前放在公開儲存庫。

## 安裝到自己的 iPad

使用 Sideloadly 官方 Windows 版本：https://sideloadly.io/

1. 依官方說明準備 Apple 驅動／相依軟體，使用 USB 連接並信任電腦。
2. 選擇自己的 iPad，載入 `MarineEncyclopedia-unsigned.ipa`。
3. 自行在工具中登入免費 Apple 帳號並完成驗證。
4. 完成簽章和安裝；依 iPadOS 提示信任開發者，必要時啟用開發者模式。
5. 啟動圖鑑，驗證翻頁、分類、相機、辨識與橫向顯示。

免費帳號簽章通常有效 7 天，需要重新簽章／更新。
請勿把 Apple 密碼或驗證碼放到 GitHub Secrets、原始碼或聊天。

Sideloadly FAQ：https://sideloadly.io/faq.html
