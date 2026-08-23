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
using UnityEngine;
using UnityEditor;

public class ScreenCaptureWindow : EditorWindow {
    private Camera targetCamera;
    private Vector2Int customResolution = new Vector2Int(1920, 1080);
    private int selectedPresetIndex = 0;
    private string[] presetNames = new string[] {
        "Game View Size",
        "Full HD (1920x1080)",
        "WQHD (2560x1440)",
        "4K UHD (3840x2160)",
        "8K UHD (7680x4320)",
        "2x Super Sample",
        "4x Super Sample",
        "Custom"
    };

    private RenderTexture previewRenderTexture;
    private Vector2 scrollPosition;

    [MenuItem("Tools/Screen Capture")]
    public static void OpenWindow() {
        var window = GetWindow<ScreenCaptureWindow>("Screen Capture");
        window.minSize = new Vector2(360, 420);
        window.Show();
    }

    private void OnEnable() {
        if (targetCamera == null) {
            targetCamera = Camera.main;
        }
    }

    private void OnDisable() {
        CleanupPreviewTexture();
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

    private Vector2Int GetTargetResolution() {
        Vector2 gameViewSize = Handles.GetMainGameViewSize();
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

        EditorGUILayout.Space(5);

        // 解像度プリセット選択
        selectedPresetIndex = EditorGUILayout.Popup("Resolution Preset", selectedPresetIndex, presetNames);

        // カスタム解像度入力
        if (selectedPresetIndex == 7) {
            EditorGUI.indentLevel++;
            customResolution = EditorGUILayout.Vector2IntField("Custom Resolution", customResolution);
            customResolution.x = Mathf.Max(16, customResolution.x);
            customResolution.y = Mathf.Max(16, customResolution.y);
            EditorGUI.indentLevel--;
        }

        Vector2Int targetRes = GetTargetResolution();
        EditorGUILayout.HelpBox($"Output Resolution: {targetRes.x} x {targetRes.y}", MessageType.Info);

        if (EditorGUI.EndChangeCheck()) {
            Repaint();
        }

        EditorGUILayout.Space(10);

        // プレビュー表示
        EditorGUILayout.LabelField("Preview", EditorStyles.boldLabel);
        Rect previewRect = GUILayoutUtility.GetRect(320, 180, GUILayout.ExpandWidth(true));
        DrawPreview(previewRect);

        EditorGUILayout.Space(15);

        // 撮影ボタン
        GUI.backgroundColor = new Color(0.3f, 0.8f, 0.4f);
        if (GUILayout.Button("Capture Screenshot", GUILayout.Height(40))) {
            Capture(targetCamera != null ? targetCamera : Camera.main, targetRes.x, targetRes.y);
        }
        GUI.backgroundColor = Color.white;

        EditorGUILayout.EndScrollView();
    }

    private void DrawPreview(Rect rect) {
        Camera cam = targetCamera != null ? targetCamera : Camera.main;
        if (cam == null) {
            EditorGUI.DrawRect(rect, Color.black);
            GUI.Label(rect, "No Camera Available", new GUIStyle(EditorStyles.boldLabel) { alignment = TextAnchor.MiddleCenter, normal = { textColor = Color.white } });
            return;
        }

        int previewWidth = Mathf.Max(64, (int)rect.width);
        int previewHeight = Mathf.Max(64, (int)rect.height);

        if (previewRenderTexture == null || previewRenderTexture.width != previewWidth || previewRenderTexture.height != previewHeight) {
            CleanupPreviewTexture();
            previewRenderTexture = new RenderTexture(previewWidth, previewHeight, 16, RenderTextureFormat.ARGB32);
        }

        RenderTexture prevTarget = cam.targetTexture;
        cam.targetTexture = previewRenderTexture;
        cam.Render();
        cam.targetTexture = prevTarget;

        GUI.DrawTexture(rect, previewRenderTexture, ScaleMode.ScaleToFit);
    }

    public static void Capture(Camera camera, int width, int height) {
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

        try {
            camera.targetTexture = render;
            camera.Render();

            RenderTexture.active = render;
            texture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            texture.Apply();
        }
        finally {
            camera.targetTexture = prevTarget;
            RenderTexture.active = prevActive;
            DestroyImmediate(render);
        }

        if (!Directory.Exists(folderName)) {
            Directory.CreateDirectory(folderName);
        }

        string fullPath = $"{folderName}/{fileName}";
        File.WriteAllBytes(fullPath, texture.EncodeToPNG());
        DestroyImmediate(texture);

        Debug.Log($"[Screen Capture] Saved screenshot ({width}x{height}): {fullPath}");

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