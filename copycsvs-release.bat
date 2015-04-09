@echo off
del highcharts\charts\*.csv
cd bin\Release
RedmineToReport.exe ..\..\Settings.xml
copy *.csv ..\..\highcharts\charts\*.csv
