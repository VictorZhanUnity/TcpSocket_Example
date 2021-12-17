using System;
using System.Collections.Generic;
using UnityEngine.Events;

namespace Managers.DebugHandler
{
    /// <summary>
    /// 負責Debug.Log相關處理(用static架構設計)。
    /// 若要引用，可直接在Script開頭使用"Debug = Managers.DebugHandler.DebugManager;"來取代Debug關鍵字
    /// </summary>
    public class DebugManager
    {
        /// <summary>
        /// 是否啟動Log
        /// </summary>
        public static bool IsActivated = true;
        /// <summary>
        /// 是否將Log字串存放於HistoryList裡
        /// </summary>
        public static bool isRecord = false;
        /// <summary>
        /// 每當Debug.Log(訊息)時觸發
        /// </summary>
        public static UnityAction<string> onLogEvent;
        private static string separater = "========== Separater ==========";

        #region {========== Log歷史訊息: LogHistoryList ==========}
        private static List<string> logList = new List<string>();
        public static List<string> LogHistoryList
        {
            get { return logList; }
        }
        public static string LogHistory
        {
            get
            {
                string result = "";
                foreach (string str in logList)
                {
                    result += str + "\n";
                }
                return result;
            }
        }

        public static void ClearHistory()
        {
            logList.RemoveRange(0, logList.Count);
            onLogEvent?.Invoke("");
        }
        #endregion

        /// <summary>
        /// Debug.Log 指定訊息
        /// </summary>
        public static void Log(string msg, TextColor? color = null, bool timeStamp = true)
        {
            if (!IsActivated) return;
            msg = SetTextColor(msg, color);
            UnityEngine.Debug.Log(msg);
            if (isRecord && !msg.Contains(separater))
            {
                RecordMsg(msg, timeStamp);
            }
            onLogEvent?.Invoke(msg);
        }

        /// <summary>
        /// Debug.Log 分隔行
        /// </summary>
        public static void LogSeparater()
        {
            if (!IsActivated) return;
            separater = SetTextSize(separater, 16);
            separater = SetTextBold(separater);
            separater = SetTextItalics(separater);
            Log(separater, TextColor.orange, false);
        }

        #region {========== 設定RicthText樣式標籤，可供外部直接調用來設定文字樣式  ==========}
        /// <summary>
        /// RichText設定文字顏色
        /// </summary>
        public static string SetTextColor(string msg, TextColor? color)
        {
            string colorStr = (color ?? TextColor.cyan).ToString();
            return $"<color={colorStr}>{msg}</color>";
        }
        /// <summary>
        /// RichText設定文字大小
        /// </summary>
        public static string SetTextSize(string msg, int size = 14)
        {
            return $"<size={size}>{msg}</size>";
        }
        /// <summary>
        /// RichText設定文字粗體
        /// </summary>
        public static string SetTextBold(string msg)
        {
            return $"<b>{msg}</b>";
        }
        /// <summary>
        /// RichText設定文字斜體
        /// </summary>
        public static string SetTextItalics(string msg)
        {
            return $"<i>{msg}</i>";
        }
        #endregion

        /// <summary>
        /// 記錄Log訊息
        /// </summary>
        private static void RecordMsg(string msg, bool isTimeStamp)
        {
            if (isTimeStamp) msg = TimeStamp + msg;
            logList.Add(msg);
        }

        /// <summary>
        /// 目前時間點
        /// </summary>
        private static string TimeStamp
        {
            get
            {
                return "[" + DateTime.Now.ToString("HH:mm:ss") + "] => ";
            }
        }

        /// <summary>
        /// 文字顏色列表
        /// </summary>
        public enum TextColor
        {
            cyan, blac, blue, green, lime, olive, orange, purple, red, teal, white, yellow
        }
    }
}


