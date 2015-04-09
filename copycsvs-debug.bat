@echo off
del highcharts\charts\*.csv
cd bin\Debug
RedmineToReport.exe ..\..\Settings.xml
copy *.csv ..\..\highcharts\charts\*.csv
