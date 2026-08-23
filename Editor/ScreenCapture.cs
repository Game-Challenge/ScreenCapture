/*
参考文献
～コードのベース～
[Unity]Unity Editor上でスクリーンショットを撮る方法 - https://nobushiueshi.com/unityunity-editor上でスクリーンショットを撮る方法/

～プラットフォーム判定～
Unity プラットフォーム判別 - https://qiita.com/Ubermensch/items/75072ef89249cb3b30e7
RuntimePlatform - https://docs.unity3d.com/ScriptReference/RuntimePlatform.html

～ファイラーで表示する方法～
【Unity】 セーブファイルのパスを開くメニューを作る - https://www.urablog.xyz/entry/2021/11/05/070000
How to Open a File or Folder in Terminal on Mac - https://www.switchingtomac.com/tutorials/how-to-open-a-file-or-folder-in-terminal-on-mac/
【 xdg-open 】コマンド――ファイルをデフォルトアプリケーションで開く - https://atmarkit.itmedia.co.jp/ait/articles/1906/06/news007.html

～ディレクトリの有無判定～
Unity フォルダやファイルの有無を確認し無ければ生成する - https://hakase0274.hatenablog.com/entry/2019/07/27/223840
[unity]指定されたファイルがどのパスにあるか調べる - https://www.create-forever.games/unity-directory-getfiles/
*/

#if UNITY_EDITOR
using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class ScreenCaptureWindow : EditorWindow {
    private Camera targetCamera;
    private bool overrideTransparentBackground = false;

    private Vector2Int customResolution = new Vector2Int(1920, 1080);
    private int selectedPresetIndex = 0;

    // 定義済み標準プリセット
    private readonly string[] builtInPresetNames = new string[] {
        "Game View Size",
        "Full HD (1920x1080)",
        "WQHD (2560x1440)",
        "4K UHD (3840x2160)",
        "8K UHD (7680x4320)",
        "2x Super Sample",
        "4x Super Sample",
        "Custom"
    };

    // ユーザー保存プリセット & 履歴 (EditorPrefs保存)
    private List<string> customPresets = new List<string>();
    private List<string> resolutionHistory = new List<string>();

    private RenderTexture previewRenderTexture;
    private Vector2 scrollPosition;

    private const string PREFS_CUSTOM_PRESETS = "ScreenCapture_CustomPresets";
    private const string PREFS_HISTORY = "ScreenCapture_ResolutionHistory";

    [MenuItem("Tools/Screen Capture")]
    public static void OpenWindow() {
        var window = GetWindow<ScreenCaptureWindow>("Screen Capture");
        window.minSize = new Vector2(380, 520);
        window.Show();
    }

    private void OnEnable() {
        if (targetCamera == null) {
            targetCamera = Camera.main;
        }
        LoadPreferences();
    }

    private void OnDisable() {
        CleanupPreviewTexture();
        SavePreferences();
    }

    private void LoadPreferences() {
        customPresets.Clear();
        string rawCustom = EditorPrefs.GetString(PREFS_CUSTOM_PRESETS, "");
        if (!string.IsNullOrEmpty(rawCustom)) {
            customPresets.AddRange(rawCustom.Split(';'));
        }

        resolutionHistory.Clear();
        string rawHistory = EditorPrefs.GetString(PREFS_HISTORY, "");
        if (!string.IsNullOrEmpty(rawHistory)) {
            resolutionHistory.AddRange(rawHistory.Split(';'));
        }
    }

    private void SavePreferences() {
        EditorPrefs.SetString(PREFS_CUSTOM_PRESETS, string.Join(";", customPresets));
        EditorPrefs.SetString(PREFS_HISTORY, string.Join(";", resolutionHistory));
    }

    private void AddToHistory(Vector2Int res) {
        string entry = $"{res.x}x{res.y}";
        resolutionHistory.Remove(entry);
        resolutionHistory.Insert(0, entry);
        if (resolutionHistory.Count > 5) {
            resolutionHistory.RemoveAt(resolutionHistory.Count - 1);
        }
        SavePreferences();
    }

    private void CleanupPreviewTexture() {
        if (previewRenderTexture != null) {
            if (targetCamera != null && targetCamera.targetTexture == previewRenderTexture) {
                targetCamera.targetTexture = null;
            }
            RenderTexture.active = null;
            DestroyImmediate(previewRenderTexture);
            previewRenderTexture = null;
        }
    }

    private List<string> GetAllPresetNames() {
        var list = new List<string>(builtInPresetNames);
        foreach (var cp in customPresets) {
            list.Add($"[Saved] {cp}");
        }
        return list;
    }

    private Vector2Int GetTargetResolution() {
        Vector2 gameViewSize = Handles.GetMainGameViewSize();
        if (selectedPresetIndex < builtInPresetNames.Length) {
            switch (selectedPresetIndex) {
                case 0: return new Vector2Int((int)gameViewSize.x, (int)gameViewSize.y);
                case 1: return new Vector2Int(1920, 1080);
                case 2: return new Vector2Int(2560, 1440);
                case 3: return new Vector2Int(3840, 2160);
                case 4: return new Vector2Int(7680, 4320);
                case 5: return new Vector2Int((int)gameViewSize.x * 2, (int)gameViewSize.y * 2);
                case 6: return new Vector2Int((int)gameViewSize.x * 4, (int)gameViewSize.y * 4);
                case 7: return customResolution;
                default: return new Vector2Int(1920, 1080);
            }
        }
        else {
            int customIndex = selectedPresetIndex - builtInPresetNames.Length;
            if (customIndex >= 0 && customIndex < customPresets.Count) {
                string[] parts = customPresets[customIndex].Split('x');
                if (parts.Length == 2 && int.TryParse(parts[0], out int w) && int.TryParse(parts[1], out int h)) {
                    return new Vector2Int(w, h);
                }
            }
            return new Vector2Int(1920, 1080);
        }
    }

    private void OnGUI() {
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("Screen Capture Settings", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);

        // カメラ選択
        EditorGUI.BeginChangeCheck();
        targetCamera = (Camera)EditorGUILayout.ObjectField("Target Camera", targetCamera, typeof(Camera), true);
        if (targetCamera == null) {
            EditorGUILayout.HelpBox("Please assign a Target Camera (Camera.main will be used if empty).", MessageType.Warning);
        }

        // 背景透過オーバーライド
        overrideTransparentBackground = EditorGUILayout.Toggle("Transparent Background", overrideTransparentBackground);

        EditorGUILayout.Space(10);

        // 解像度プリセット選択
        List<string> allPresets = GetAllPresetNames();
        if (selectedPresetIndex >= allPresets.Count) selectedPresetIndex = 0;
        selectedPresetIndex = EditorGUILayout.Popup("Resolution Preset", selectedPresetIndex, allPresets.ToArray());

        // Custom選択時のUI
        if (selectedPresetIndex == 7) {
            EditorGUI.indentLevel++;
            customResolution = EditorGUILayout.Vector2IntField("Custom Resolution", customResolution);
            customResolution.x = Mathf.Max(16, customResolution.x);
            customResolution.y = Mathf.Max(16, customResolution.y);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Save Preset", GUILayout.Width(110))) {
                string entry = $"{customResolution.x}x{customResolution.y}";
                if (!customPresets.Contains(entry)) {
                    customPresets.Add(entry);
                    SavePreferences();
                }
            }

            // 履歴ドロップダウン（最大5件）
            if (resolutionHistory.Count > 0) {
                if (EditorGUILayout.DropdownButton(new GUIContent("History (Max 5)"), FocusType.Keyboard, GUILayout.Width(130))) {
                    GenericMenu menu = new GenericMenu();
                    foreach (var hist in resolutionHistory) {
                        menu.AddItem(new GUIContent(hist), false, () => {
                            string[] parts = hist.Split('x');
                            if (parts.Length == 2 && int.TryParse(parts[0], out int w) && int.TryParse(parts[1], out int h)) {
                                customResolution = new Vector2Int(w, h);
                                Repaint();
                            }
                        });
                    }
                    menu.ShowAsContext();
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUI.indentLevel--;
        }
        else if (selectedPresetIndex >= builtInPresetNames.Length) {
            // 登録済みカスタムプリセットの削除ボタン
            int customIndex = selectedPresetIndex - builtInPresetNames.Length;
            if (GUILayout.Button("Remove Saved Preset", GUILayout.Width(150))) {
                customPresets.RemoveAt(customIndex);
                selectedPresetIndex = 0;
                SavePreferences();
            }
        }

        Vector2Int targetRes = GetTargetResolution();
        EditorGUILayout.HelpBox($"Output Resolution: {targetRes.x} x {targetRes.y}{(overrideTransparentBackground ? " (Transparent)" : "")}", MessageType.Info);

        if (EditorGUI.EndChangeCheck()) {
            Repaint();
        }

        EditorGUILayout.Space(10);

        // プレビュー表示
        EditorGUILayout.LabelField("Preview", EditorStyles.boldLabel);
        DrawPreview(targetRes);

        EditorGUILayout.Space(15);

        // 撮影ボタン
        GUI.backgroundColor = new Color(0.3f, 0.8f, 0.4f);
        if (GUILayout.Button("Capture Screenshot", GUILayout.Height(40))) {
            Vector2Int finalRes = GetTargetResolution();
            AddToHistory(finalRes);
            Capture(targetCamera != null ? targetCamera : Camera.main, finalRes.x, finalRes.y, overrideTransparentBackground);
        }
        GUI.backgroundColor = Color.white;

        EditorGUILayout.EndScrollView();
    }

    private void DrawPreview(Vector2Int targetRes) {
        Camera cam = targetCamera != null ? targetCamera : Camera.main;

        // 余白（左右マージン 15px）
        float marginHorizontal = 15f;
        float availableWidth = EditorGUIUtility.currentViewWidth - (marginHorizontal * 2f);

        // プレビューの最大高さを 260px に制限
        float maxHeight = 260f;

        // アスペクト比計算 (幅 / 高さ)
        float aspect = (float)targetRes.x / targetRes.y;

        float previewWidth = availableWidth;
        float previewHeight = previewWidth / aspect;

        if (previewHeight > maxHeight) {
            previewHeight = maxHeight;
            previewWidth = previewHeight * aspect;
        }

        // センタリング配置
        Rect previewRect = GUILayoutUtility.GetRect(previewWidth, previewHeight, GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(false));
        previewRect.x = marginHorizontal + (availableWidth - previewWidth) * 0.5f;

        if (cam == null) {
            EditorGUI.DrawRect(previewRect, Color.black);
            GUI.Label(previewRect, "No Camera Available", new GUIStyle(EditorStyles.boldLabel) { alignment = TextAnchor.MiddleCenter, normal = { textColor = Color.white } });
            return;
        }

        int renderWidth = Mathf.Max(64, (int)previewWidth);
        int renderHeight = Mathf.Max(64, (int)previewHeight);

        if (previewRenderTexture == null || previewRenderTexture.width != renderWidth || previewRenderTexture.height != renderHeight) {
            CleanupPreviewTexture();
            previewRenderTexture = new RenderTexture(renderWidth, renderHeight, 16, RenderTextureFormat.ARGB32);
        }

        RenderTexture prevTarget = cam.targetTexture;
        CameraClearFlags prevFlags = cam.clearFlags;
        Color prevBg = cam.backgroundColor;

        if (overrideTransparentBackground) {
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0, 0, 0, 0);
        }

        cam.targetTexture = previewRenderTexture;
        cam.Render();

        cam.targetTexture = prevTarget;
        cam.clearFlags = prevFlags;
        cam.backgroundColor = prevBg;

        GUI.DrawTexture(previewRect, previewRenderTexture, ScaleMode.ScaleToFit);
    }

    public static void Capture(Camera camera, int width, int height, bool transparent) {
        if (camera == null) {
            Debug.LogError("[Screen Capture] No camera assigned for capture.");
            return;
        }

        DateTime now = DateTime.Now;
        string folderName = "Screenshot";
        string headerName = "ScreenShot";
        string fileName = string.Format("{0}_{1}x{2}_{3}-{4:D2}-{5:D2}_{6:D2}{7:D2}{8:D2}.png",
            headerName, width, height, now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second);

        RenderTexture render = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
        Texture2D texture = new Texture2D(width, height, TextureFormat.ARGB32, false);

        RenderTexture prevTarget = camera.targetTexture;
        RenderTexture prevActive = RenderTexture.active;
        CameraClearFlags prevFlags = camera.clearFlags;
        Color prevBg = camera.backgroundColor;

        try {
            if (transparent) {
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = new Color(0, 0, 0, 0);
            }

            camera.targetTexture = render;
            camera.Render();

            RenderTexture.active = render;
            texture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            texture.Apply();
        }
        finally {
            camera.targetTexture = prevTarget;
            RenderTexture.active = prevActive;
            camera.clearFlags = prevFlags;
            camera.backgroundColor = prevBg;
            DestroyImmediate(render);
        }

        if (!Directory.Exists(folderName)) {
            Directory.CreateDirectory(folderName);
        }

        string fullPath = $"{folderName}/{fileName}";
        File.WriteAllBytes(fullPath, texture.EncodeToPNG());
        DestroyImmediate(texture);

        Debug.Log($"[Screen Capture] Saved screenshot ({width}x{height}{(transparent ? ", Transparent" : "")}): {fullPath}");

        if (Application.platform == RuntimePlatform.WindowsEditor) {
            string osPath = ($"{Application.dataPath}/../{folderName}/{fileName}").Replace('/', '\\');
            System.Diagnostics.Process.Start("explorer.exe", $"/select,{osPath}");
        }
        else if (Application.platform == RuntimePlatform.OSXEditor) {
            System.Diagnostics.Process.Start("open", fullPath);
        }
        else if (Application.platform == RuntimePlatform.LinuxEditor) {
            System.Diagnostics.Process.Start("xdg-open", fullPath);
        }
    }
}
#endif