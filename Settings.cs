#region

using System;
using System.Xml.Serialization;

#endregion

namespace RedmineToReport
{
    [XmlRoot("Settings")]
    public class Settings
    {
        /// <summary>
        ///     RedmineのURL
        /// </summary>
        [XmlElement("redmine-url")]
        public String RedmineUrl { get; set; }

        /// <summary>
        ///     RedmineのURL ( APIKEY取得用のURLが異なっる場合、こちらにブラウザ用のURLを入れる　）
        /// </summary>
        [XmlElement("redmine-url-alias")]
        public String RedmineUrlAlias { get; set; }

        /// <summary>
        ///     RedmineのURL
        /// </summary>
        [XmlElement("redmine-project")]
        public String RedmineProjectName { get; set; }

        /// <summary>
        ///     RedmineのAPIKEY
        /// </summary>
        [XmlElement("redmine-apikey")]
        public String RedmineApiKey { get; set; }

        /// <summary>
        ///     RedmineのQuery ID ( 対象レポート )
        /// </summary>
        [XmlElement("redmine-query-id")]
        public String RedmineQueryId { get; set; }

        /// <summary>
        ///     RedmineのQuery ID ( 全残りタスクのレポート )
        /// </summary>
        [XmlElement("redmine-query-id-all-remining")]
        public String RedmineAllRemainingQueryId { get; set; }

        /// <summary>
        ///     計測区間
        /// </summary>
        [XmlElement("sprint-size")]
        public int SipriteSize { get; set; }

        /// <summary>
        ///     Redmineの「バグ」を表すステータス名
        /// </summary>
        [XmlElement("bug-keyword")]
        public string BugKeyword { get; set; }

        /// <summary>
        ///     RedmineのURLを取るwrapper
        /// </summary>
        private string GetUrl(string url)
        {
            if (url.Length != 0 && url[url.Length - 1] != '/')
            {
                url += '/';
            }
            return url;
        }

        /// <summary>
        ///     プロジェクトのURLを取得
        /// </summary>
        public string GetRedmineProjectUrl()
        {
            return GetUrl(RedmineUrl) + "projects/" + RedmineProjectName;
        }

        /// <summary>
        ///     プロジェクトのクエリーのURLを取得
        /// </summary>
        public string GetRedmineQueryUrl()
        {
            return GetUrl(string.IsNullOrEmpty(RedmineUrlAlias) ? RedmineUrl : RedmineUrlAlias) + "projects/" +
                   RedmineProjectName + "/issues?query_id=" + RedmineQueryId;
        }

        /// <summary>
        ///     指定IssueのURLを取得
        /// </summary>
        public string GetRedmineIssueUrl(int issueid)
        {
            return GetUrl(string.IsNullOrEmpty(RedmineUrlAlias) ? RedmineUrl : RedmineUrlAlias) + "issues/" + issueid;
        }
    }
}