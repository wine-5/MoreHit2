using UnityEngine;
using System.IO;
using System;

namespace MoreHit.Core
{
    /// <summary>
    /// ビルド版でもログをファイルに出力するデバッグツール
    /// </summary>
    public class DebugLogToFile : MonoBehaviour
    {
        [Header("設定")]
        [SerializeField] private bool enableInBuild = true;
        [SerializeField] private bool logToConsole = true;
        [SerializeField] private string logFileName = "game_log.txt";
        [SerializeField] private int maxLogLines = 1000;
        
        private string logFilePath;
        private StreamWriter logWriter;
        private int logLineCount = 0;
        
        private void Awake()
        {
            if (!enableInBuild && !Application.isEditor)
                return;
            
            // ログファイルのパスを設定（実行ファイルと同じフォルダ）
            logFilePath = Path.Combine(Application.dataPath, "..", logFileName);
            
            try
            {
                // 既存のログファイルを削除して新規作成
                if (File.Exists(logFilePath))
                    File.Delete(logFilePath);
                
                logWriter = new StreamWriter(logFilePath, false);
                logWriter.AutoFlush = true;
                
                // ヘッダー情報を書き込み
                WriteLog($"=== Game Log Started ===");
                WriteLog($"Time: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                WriteLog($"Unity Version: {Application.unityVersion}");
                WriteLog($"Platform: {Application.platform}");
                WriteLog($"Product Name: {Application.productName}");
                WriteLog($"Version: {Application.version}");
                WriteLog($"Data Path: {Application.dataPath}");
                WriteLog($"========================\n");
                
                // Unityのログイベントを購読
                Application.logMessageReceived += HandleLog;
                
                Debug.Log($"[DebugLogToFile] ログファイル作成: {logFilePath}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[DebugLogToFile] ログファイル作成失敗: {ex.Message}");
            }
        }
        
        private void OnDestroy()
        {
            Application.logMessageReceived -= HandleLog;
            
            if (logWriter != null)
            {
                WriteLog($"\n=== Game Log Ended ===");
                WriteLog($"Time: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                WriteLog($"======================");
                
                logWriter.Close();
                logWriter = null;
            }
        }
        
        private void OnApplicationQuit()
        {
            OnDestroy();
        }
        
        private void HandleLog(string logString, string stackTrace, LogType type)
        {
            if (logWriter == null || logLineCount >= maxLogLines)
                return;
            
            string timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
            string typePrefix = GetLogTypePrefix(type);
            
            WriteLog($"[{timestamp}] {typePrefix} {logString}");
            
            // エラーやExceptionの場合はスタックトレースも出力
            if ((type == LogType.Error || type == LogType.Exception) && !string.IsNullOrEmpty(stackTrace))
            {
                WriteLog($"Stack Trace:\n{stackTrace}");
            }
            
            logLineCount++;
            
            // 最大行数に達したら警告
            if (logLineCount >= maxLogLines)
            {
                WriteLog($"\n*** Maximum log lines ({maxLogLines}) reached. Logging stopped. ***");
            }
        }
        
        private string GetLogTypePrefix(LogType type)
        {
            switch (type)
            {
                case LogType.Error:
                    return "[ERROR]";
                case LogType.Assert:
                    return "[ASSERT]";
                case LogType.Warning:
                    return "[WARNING]";
                case LogType.Log:
                    return "[LOG]";
                case LogType.Exception:
                    return "[EXCEPTION]";
                default:
                    return "[UNKNOWN]";
            }
        }
        
        private void WriteLog(string message)
        {
            if (logWriter != null)
            {
                logWriter.WriteLine(message);
                
                // コンソールにも出力（エディタ用）
                if (logToConsole && Application.isEditor)
                    Debug.Log($"[LogFile] {message}");
            }
        }
    }
}
