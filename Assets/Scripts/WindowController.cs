using System;
using System.Runtime.InteropServices;
using UnityEngine;

/// <summary>
/// سكربت التحكم بنافذة التطبيق في نظام ويندوز (WindowController).
/// يتيح للمستخدم:
/// 1. إنزال وتصغير النافذة لشريط المهام (Minimize to taskbar)
/// 2. تكبير النافذة أو جعلها ملء الشاشة والعودة للوضع العادي (Toggle Maximize/Restore)
/// 3. إغلاق اللعبة والخروج منها بأمان (Close/Quit)
/// </summary>
public class WindowController : MonoBehaviour
{
#if UNITY_STANDALONE_WIN
    [DllImport("user32.dll")]
    private static extern IntPtr GetActiveWindow();

    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    // أمر تصغير النافذة في ويندوز
    private const int SW_MINIMIZE = 6;
#endif

    /// <summary>
    /// تصغير النافذة إلى شريط المهام السفلي في ويندوز
    /// </summary>
    public void MinimizeWindow()
    {
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
        try
        {
            IntPtr hWnd = GetActiveWindow();
            if (hWnd != IntPtr.Zero)
            {
                ShowWindow(hWnd, SW_MINIMIZE);
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning("[WindowController] Minimize error: " + ex.Message);
        }
#else
        Debug.Log("[WindowController] Minimize clicked (Runs on Windows PC build).");
#endif
    }

    /// <summary>
    /// التبديل بين وضع ملء الشاشة ووضع النافذة العادية
    /// </summary>
    public void ToggleMaximizeWindow()
    {
        Screen.fullScreen = !Screen.fullScreen;
    }

    /// <summary>
    /// إغلاق التطبيق بالكامل
    /// </summary>
    public void CloseApplication()
    {
        Debug.Log("[WindowController] Quitting application...");
        Application.Quit();
    }
}
