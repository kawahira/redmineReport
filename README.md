# RedmineToReport

Redmineからチケットを参照してグラフを生成するツールです。pluginで作ればいいのにC#で書きたいと言う理由だけで
C#ツールです。

ツール自体を実行をすると大量のCSVを出力します。
このCSVをリポジトリ内のhighcharts\chartにコピーしてからhighcharts\index.htmlをブラウザーで見ると
グラフが確認出来ます。

どんなグラフがでるかは[Qiita](http://qiita.com/kawahira/private/0196c315642f4a72458f)を参照ください。

# 実行方法

RedmineToReport.exe settings.xml

# settings.xmlの内容

ツールの実行にはXML形式の設定ファイルが必要です。以下の設定を入れます。

* redmine-url : RedmineのURL
* redmine-project : Redmine上のAPIキーにアクセスするためのURL
* redmine-url-alias : RedmineのAPIキーのURLがブラザーで参照するURLと異なる場合、こちらにブラザで参照するURLを入れます。
* redmine-apikey : RedmineのAPIKEY
* redmine-query-id : 見たい範囲がフィルターされたクエリ
* redmine-query-id-all-remining : 現在の未完了のチケット全てが見えるクエリ
* sprint-size : グラフをある程度の期間でまとめる日数 2週間単位なら 14 一ヶ月単位なら 30
* bug-keyword : バグと判定するトラッカー名称 Redmineデフォルトなら「バグ」

  
