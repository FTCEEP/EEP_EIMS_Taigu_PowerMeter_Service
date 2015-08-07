Imports System.Data
Imports System.Data.SqlClient
Imports NLog
Imports System.Threading.Tasks
Imports System.Threading
Imports System.Net.Mail
Imports Newtonsoft.Json
Imports System.Collections.Specialized

''' <summary>
''' Utility Module
''' </summary>
''' <remarks></remarks>
Module Utility
    Public glogger As Logger = NLog.LogManager.GetCurrentClassLogger

    Public gclsDBUtility As clsDBUtility
    Public tkScanJog As Task
    Public gbCheckCustomerInfoUpdate As Boolean = True
    Public gbCheckJog As Boolean = True
    Public giCheckSettingChangeCount As Integer = 0
    Public giPassDBErrorCount As Integer = 0

    Public gdbEEP_Points As New Dictionary(Of String, dbEEP_Points)
    ''' <summary>
    ''' FTC VMS SiPass Service Stop
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub ServiceStop()
        Try

            gclsDBUtility = Nothing
            GC.Collect()
            glogger.Error("[EEP_EIMS_Taigu_PowerMeter_Service ServiceStop]Ver:" & My.Application.Info.Version.ToString)
        Catch ex As Exception
            glogger.Error("[EEP_EIMS_Taigu_PowerMeter_Service ServiceStop]Error=" & ex.ToString)
        End Try
    End Sub
    ''' <summary>
    ''' FTC VMS SiPass Service 初始化
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub iniServiceStart()
        Try
            glogger.Error("[EEP_EIMS_Taigu_PowerMeter_Service iniServiceStart]Ver:" & My.Application.Info.Version.ToString)
            gclsDBUtility = New clsDBUtility(My.Settings.DBIP, "EIMSDeclare", My.Settings.DBLoginID, My.Settings.DBLoginPassword)

            If gclsDBUtility.OpenDB = True Then
                iniSystemDefineInfo()
            End If
            tkScanJog = Task(Of String).Factory.StartNew(Function() ScanJobStart())
            'ScanJob()
        Catch ex As Exception
            glogger.Error("[EEP_EIMS_Taigu_PowerMeter_Service iniServiceStart]Error=" & ex.ToString)
        End Try
    End Sub


    Public Function iniSystemDefineInfo()
        Try
            glogger.Debug("[iniSystemDefineInfo Start]")
            gdbEEP_Points = New Dictionary(Of String, dbEEP_Points)
            gdbEEP_Points = gclsDBUtility.GetEEP_Points
            glogger.Debug("[iniSystemDefineInfo End]")
        Catch ex As Exception
            glogger.Error("[iniSystemDefineInfo]Error=" & ex.ToString)
        End Try
    End Function
    Public Function CheckSettingChange()
        Try
            Dim bCheckSettingChange As Boolean = False
            bCheckSettingChange = gclsDBUtility.CheckSettingChange
            If bCheckSettingChange = True Then
                iniSystemDefineInfo()
            End If
        Catch ex As Exception
            glogger.Error("[CheckSettingChange]Error=" & ex.ToString)
        End Try
    End Function
    ''' <summary>
    ''' 持續檢查是否有相關工作要執行
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function ScanJobStart() As String

        glogger.Info("[ScanJobStart]ManagedThreadId=" & Thread.CurrentThread.ManagedThreadId)
        Dim bSiPassStatus As Boolean = False
        Do While gbCheckJog
            Try
                System.Threading.Thread.Sleep(My.Settings.ScanDeviceInterval)
                If gclsDBUtility.iDBErrorCount > My.Settings.DBErrorCount Then
                    giPassDBErrorCount = giPassDBErrorCount + 1
                    If giPassDBErrorCount > My.Settings.DBErrorReConnectCount Then
                        gclsDBUtility.iDBErrorCount = 0
                        giPassDBErrorCount = 0
                    End If
                    Continue Do
                End If
                If giCheckSettingChangeCount = My.Settings.CheckSettingChange Then
                    giCheckSettingChangeCount = 0
                    CheckSettingChange()
                    UpdateServiceAlive(bSiPassStatus)
                End If

                ScanJob()

                giCheckSettingChangeCount = giCheckSettingChangeCount + 1

            Catch ex As Exception
                glogger.Error("[ScanJobStart]Error=" & ex.ToString)
            End Try
        Loop
        If gbCheckJog = False Then
            glogger.Info("ScanJobStart ReStart End ManagedThreadId=" & Thread.CurrentThread.ManagedThreadId)
            gbCheckJog = True
        End If
    End Function
    ''' <summary>
    ''' 更新資料庫 SysDeviceStatus table相關資訊
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function UpdateServiceAlive(ByVal SiPassStatus As Boolean)
        Try
            gclsDBUtility.UpdateServiceAliveInfo("EEP_EIMS_Taigu_PowerMeter_Service", SiPassStatus)
        Catch ex As Exception
            glogger.Error("[UpdateServiceAlive]Error=" & ex.ToString)
        End Try
    End Function
    ''' <summary>
    ''' 檢查是否有訪客拜訪工作
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function ScanJob()
        Try
            Dim Key As String
            Dim odbEEP_Points As dbEEP_Points
            For Each Key In gdbEEP_Points.Keys
                odbEEP_Points = gdbEEP_Points(Key)
             
                Get_Taigu_PowerMeter(odbEEP_Points)
                Get_Taigu_PowerMeter_All(odbEEP_Points)
                DayReport(odbEEP_Points)
            Next
        Catch ex As Exception
            glogger.Error("[ScanJob]Error=" & ex.ToString)
        End Try
    End Function
    Private Sub Get_Taigu_PowerMeter(ByVal odbEEP_Points As dbEEP_Points)
        Try
            Dim Result As String
            Dim sStartDay As String
            Dim ws As New Webservice_taigu.PowerMeterDatas

            Dim oPowerMeterDatas As PowerMeterDatas
            Dim i As Integer

            Dim iMinKwhByHr As Double = 0
            Dim iMaxKwhByHr As Double = 0
            Dim iKwhByHr As Double = 0
            Dim iTotalKwhByHr As Double = 0
            Dim bSum As Boolean = False
            Dim iSumCount As Integer = 0
            Dim oSumPowerMeterDatas As New PowerMeterDatas

            ws.Timeout = 10000

            If Now < odbEEP_Points.LastGetDataTime.AddHours(1) Then
                Exit Sub
            End If
         
            sStartDay = odbEEP_Points.LastGetDataTime.ToString("yyyyMMddHHmm00") '"20140807070000"
            glogger.Debug("[Get_Taigu_PowerMeter Start] CompanyName=" & odbEEP_Points.CompanyName & ",CID=" & odbEEP_Points.LoginCID & ",sStartDay=" & sStartDay)
            Result = ws.GetPowerMeterDatas(odbEEP_Points.LoginCID, odbEEP_Points.LoginUser, odbEEP_Points.LoginPW, sStartDay)

            'Dim oP As New List(Of clsPowerMeter)
            'glogger.Debug("[Get_Taigu_PowerMeter]Result=" & Result)
            Dim oP As clsPowerMeter

            oP = JsonConvert.DeserializeObject(Of clsPowerMeter)(Result)
            If oP.code = "1" Then
                glogger.Debug("[Get_Taigu_PowerMeter End] CompanyName=" & odbEEP_Points.CompanyName & ",CID=" & odbEEP_Points.LoginCID & ",sStartDay=" & sStartDay)

                For i = 0 To oP.dataList.Count - 1
                    Try
                        If i <= oP.dataList.Count - 1 Then
                            oPowerMeterDatas = oP.dataList(i)
                            If oPowerMeterDatas.PMID Is Nothing Then
                                oP.dataList.Remove(oPowerMeterDatas)
                                i = i - 1
                            End If
                        End If

                    Catch ex As Exception
                        Dim ss As String = i
                    End Try

                Next
            
                Dim Query = From contact In oP.dataList Where odbEEP_Points.LastPointListValue.ContainsKey(contact.PMID)

                For Each oPowerMeterDatas In Query
                    If oPowerMeterDatas Is Nothing Then
                        glogger.Debug("[Get_Taigu_PowerMeter Return Vaule OK] CompanyName=" & odbEEP_Points.CompanyName & ",CID=" & odbEEP_Points.LoginCID & ",PMID=" & oPowerMeterDatas.PMID & ",RecTime=" & oPowerMeterDatas.RECTIME & ",KWH=" & oPowerMeterDatas.KWH)
                    End If
                    If IsNumeric(oPowerMeterDatas.KWH) = True Then
                        oSumPowerMeterDatas.KWH = CDbl(oSumPowerMeterDatas.KWH) + CDbl(oPowerMeterDatas.KWH)
                        gdbEEP_Points(odbEEP_Points.CompanyID).LastPointListValue(oPowerMeterDatas.PMID).LastValue = CDbl(oPowerMeterDatas.KWH)
                        glogger.Debug("[Get_Taigu_PowerMeter Return Vaule OK] CompanyName=" & odbEEP_Points.CompanyName & ",CID=" & odbEEP_Points.LoginCID & ",PMID=" & oPowerMeterDatas.PMID & ",RecTime=" & oPowerMeterDatas.RECTIME & ",KWH=" & oPowerMeterDatas.KWH)
                    Else
                        oPowerMeterDatas.KWH = gdbEEP_Points(odbEEP_Points.CompanyID).LastPointListValue(oPowerMeterDatas.PMID).LastValue
                        oPowerMeterDatas.RECTIME = odbEEP_Points.LastGetDataTime.ToString("yyyy-MM-dd HH:mm:ss")
                        oSumPowerMeterDatas.KWH = CDbl(oSumPowerMeterDatas.KWH) + gdbEEP_Points(odbEEP_Points.CompanyID).LastPointListValue(oPowerMeterDatas.PMID).LastValue
                        glogger.Debug("[Get_Taigu_PowerMeter Return Vaule Fail] CompanyName=" & odbEEP_Points.CompanyName & ",CID=" & odbEEP_Points.LoginCID & ",PMID=" & oPowerMeterDatas.PMID & ",RecTime=" & oPowerMeterDatas.RECTIME & ",KWH=" & oPowerMeterDatas.KWH)
                    End If
                    gclsDBUtility.InsertRealTimeData(oPowerMeterDatas, odbEEP_Points)

                    If gdbEEP_Points(odbEEP_Points.CompanyID).LastPointListValue(oPowerMeterDatas.PMID).MinKwhByHr = -1 Then
                        gclsDBUtility.GetHistoryData(oPowerMeterDatas, odbEEP_Points)
                    Else
                        gdbEEP_Points(odbEEP_Points.CompanyID).LastPointListValue(oPowerMeterDatas.PMID).MaxKwhByHr = oPowerMeterDatas.KWH
                        gdbEEP_Points(odbEEP_Points.CompanyID).LastPointListValue(oPowerMeterDatas.PMID).MaxKwhRecTime = odbEEP_Points.LastGetDataTime
                        If gdbEEP_Points(odbEEP_Points.CompanyID).LastPointListValue(oPowerMeterDatas.PMID).MinKwhByHr = 0 Then
                            gdbEEP_Points(odbEEP_Points.CompanyID).LastPointListValue(oPowerMeterDatas.PMID).MinKwhByHr = oPowerMeterDatas.KWH
                        End If
                    End If


                    oSumPowerMeterDatas.RECTIME = odbEEP_Points.LastGetDataTime.ToString("yyyy-MM-dd HH:mm:ss") 'oPowerMeterDatas.RECTIME
                    iSumCount = iSumCount + 1
                    If odbEEP_Points.LastPointListValue.Count = 1 Then
                        oSumPowerMeterDatas.PMID = oPowerMeterDatas.PMID
                    Else
                        oSumPowerMeterDatas.PMID = "Sum"
                    End If
                    If odbEEP_Points.LastPointListValue.Count = iSumCount Then
                        bSum = True
                    End If

                    If bSum = True Then
                        If gclsDBUtility.InsertHistoryData(oSumPowerMeterDatas, odbEEP_Points) = True Then
                            If gdbEEP_Points(odbEEP_Points.CompanyID).MinKwhByHr = -1 Then
                                gdbEEP_Points(odbEEP_Points.CompanyID).MinKwhByHr = 0
                                gdbEEP_Points(odbEEP_Points.CompanyID).MinKwhRecTime = odbEEP_Points.LastGetDataTime.ToString("yyyy-MM-dd HH:mm:ss")
                            Else
                                gdbEEP_Points(odbEEP_Points.CompanyID).MaxKwhByHr = oSumPowerMeterDatas.KWH
                                gdbEEP_Points(odbEEP_Points.CompanyID).MaxKwhRecTime = oSumPowerMeterDatas.RECTIME
                            End If
                            gdbEEP_Points(odbEEP_Points.CompanyID).LastGetDataTime = odbEEP_Points.LastGetDataTime.AddMinutes(15)
                        End If
                        If DateDiff(DateInterval.Hour, gdbEEP_Points(odbEEP_Points.CompanyID).MinKwhRecTime, gdbEEP_Points(odbEEP_Points.CompanyID).MaxKwhRecTime) >= 1 Then
                            'iMinKwhByHr = gdbEEP_Points(odbEEP_Points.CompanyID).MinKwhByHr
                            'iMaxKwhByHr = gdbEEP_Points(odbEEP_Points.CompanyID).MaxKwhByHr
                            'If iMaxKwhByHr < iMinKwhByHr Then
                            '    glogger.Debug("[Get_Taigu_PowerMeter iMaxKwhByHr < iMinKwhByHr] iMaxKwhByHr=" & iMaxKwhByHr & ",iMinKwhByHr=" & iMinKwhByHr)
                            '    iKwhByHr = iMaxKwhByHr + ConverOverKwh(iMinKwhByHr)
                            '    glogger.Debug("[Get_Taigu_PowerMeter ConverOverKwh] iMaxKwhByHr=" & iMaxKwhByHr & ",iMinKwhByHr=" & iMinKwhByHr & ",iKwhByHr=" & iKwhByHr)
                            'Else
                            '    iKwhByHr = iMaxKwhByHr - iMinKwhByHr
                            'End If
                            'If gclsDBUtility.InsertHistoryDataByDay(odbEEP_Points.CompanyID, gdbEEP_Points(odbEEP_Points.CompanyID).MinKwhRecTime, iKwhByHr) = True Then
                            '    gdbEEP_Points(odbEEP_Points.CompanyID).MinKwhByHr = oSumPowerMeterDatas.KWH
                            '    gdbEEP_Points(odbEEP_Points.CompanyID).MinKwhRecTime = oSumPowerMeterDatas.RECTIME
                            'End If
                            Dim arr As Array = gdbEEP_Points(odbEEP_Points.CompanyID).LastPointListValue.Keys.ToArray
                            iTotalKwhByHr = 0
                            For i = 0 To arr.Length - 1
                                iMinKwhByHr = gdbEEP_Points(odbEEP_Points.CompanyID).LastPointListValue(arr(i)).MinKwhByHr
                                iMaxKwhByHr = gdbEEP_Points(odbEEP_Points.CompanyID).LastPointListValue(arr(i)).MaxKwhByHr
                                If iMinKwhByHr = 0 Then
                                    iMinKwhByHr = iMaxKwhByHr
                                End If
                                If iMaxKwhByHr < iMinKwhByHr Then
                                    glogger.Debug("[Get_Taigu_PowerMeter iMaxKwhByHr < iMinKwhByHr] iMaxKwhByHr=" & iMaxKwhByHr & ",iMinKwhByHr=" & iMinKwhByHr)
                                    iKwhByHr = iMaxKwhByHr + ConverOverKwh(iMinKwhByHr)
                                    glogger.Debug("[Get_Taigu_PowerMeter ConverOverKwh] iMaxKwhByHr=" & iMaxKwhByHr & ",iMinKwhByHr=" & iMinKwhByHr & ",iKwhByHr=" & iKwhByHr)
                                Else
                                    iKwhByHr = iMaxKwhByHr - iMinKwhByHr
                                End If
                                iTotalKwhByHr = iTotalKwhByHr + iKwhByHr
                            Next
                            If gclsDBUtility.InsertHistoryDataByDay(odbEEP_Points.CompanyID, gdbEEP_Points(odbEEP_Points.CompanyID).MinKwhRecTime, iTotalKwhByHr) = True Then
                                gdbEEP_Points(odbEEP_Points.CompanyID).MinKwhByHr = oSumPowerMeterDatas.KWH
                                gdbEEP_Points(odbEEP_Points.CompanyID).MinKwhRecTime = oSumPowerMeterDatas.RECTIME
                                For i = 0 To arr.Length - 1
                                    gdbEEP_Points(odbEEP_Points.CompanyID).LastPointListValue(arr(i)).MinKwhByHr = gdbEEP_Points(odbEEP_Points.CompanyID).LastPointListValue(arr(i)).MaxKwhByHr
                                    gdbEEP_Points(odbEEP_Points.CompanyID).LastPointListValue(arr(i)).MinKwhRecTime = gdbEEP_Points(odbEEP_Points.CompanyID).LastPointListValue(arr(i)).MaxKwhRecTime
                                Next
                            End If
                        End If
                    End If

                Next
            End If
        Catch ex As Exception
            glogger.Error("[Get_Taigu_PowerMeter]Error=" & ex.Message)
        End Try
    End Sub
    Private Sub Get_Taigu_PowerMeter_All(ByVal odbEEP_Points As dbEEP_Points)
        Try
            Dim Result As String
            Dim sStartDay As String
            Dim ws As New Webservice_taigu.PowerMeterDatas
            Dim mclsDBUtility As clsDBUtility
            Dim oPowerMeterDatas As PowerMeterDatas
            Dim i As Integer
            Dim bAddDBOk As Boolean = False
         
            ws.Timeout = 10000

            If Now < odbEEP_Points.LastGetAllDataTime.AddHours(1) Then
                Exit Sub
            End If
            mclsDBUtility = New clsDBUtility(My.Settings.DBIP, "EEP_Client_" & odbEEP_Points.CompanyID, My.Settings.DBLoginID, My.Settings.DBLoginPassword)

            If mclsDBUtility.OpenDB = True Then
                sStartDay = odbEEP_Points.LastGetAllDataTime.ToString("yyyyMMddHHmm00") '"20140807070000"
                glogger.Debug("[Get_Taigu_PowerMeter_All Start] CompanyName=" & odbEEP_Points.CompanyName & ",CID=" & odbEEP_Points.LoginCID & ",sStartDay=" & sStartDay)
                Result = ws.GetPowerMeterDatas(odbEEP_Points.LoginCID, odbEEP_Points.LoginUser, odbEEP_Points.LoginPW, sStartDay)
                Dim oP As clsPowerMeter
                oP = JsonConvert.DeserializeObject(Of clsPowerMeter)(Result)
                If oP.code = "1" Then

                    For i = 0 To oP.dataList.Count - 1
                        oPowerMeterDatas = oP.dataList(i)
                        If oPowerMeterDatas.PMID IsNot Nothing Then
                            If IsNumeric(oPowerMeterDatas.KWH) = True And IsDate(oPowerMeterDatas.RECTIME) = True Then
                                If mclsDBUtility.InsertRawData(oPowerMeterDatas, odbEEP_Points) = True Then
                                    bAddDBOk = True
                                End If
                            End If
                        End If
                    Next
                    If bAddDBOk = False Then
                        glogger.Debug("[Get_Taigu_PowerMeter_All Read Data Null] CompanyName=" & odbEEP_Points.CompanyName & ",CID=" & odbEEP_Points.LoginCID & ",sStartDay=" & sStartDay)
                    End If
                    gdbEEP_Points(odbEEP_Points.CompanyID).LastGetAllDataTime = odbEEP_Points.LastGetAllDataTime.AddMinutes(15)
                    gclsDBUtility.Update_EEP_Points_LastGetAllDataTime(odbEEP_Points)
                    glogger.Debug("[Get_Taigu_PowerMeter_All End] CompanyName=" & odbEEP_Points.CompanyName & ",CID=" & odbEEP_Points.LoginCID & ",sStartDay=" & sStartDay)

                End If
                mclsDBUtility = Nothing
            Else
                glogger.Debug("[Get_Taigu_PowerMeter_All Open DB Fail] DB=EEP_Client_" & odbEEP_Points.CompanyID & ",CompanyName=" & odbEEP_Points.CompanyName)
            End If

        Catch ex As Exception
            glogger.Error("[Get_Taigu_PowerMeter_All]Error=" & ex.Message)
        End Try
    End Sub
    Private Function ConvertJson(ByVal sJson As String) As clsPowerMeter
        Try
            'Dim oUVs As List(Of clsPowerMeter) = JsonConvert.DeserializeObject(Of List(Of clsPowerMeter))(sJson)
            'Return oUVs
            Dim oUVs As List(Of clsPowerMeter) = JsonConvert.DeserializeObject(Of List(Of clsPowerMeter))(sJson)


            ' Return oUVs
        Catch ex As Exception
            glogger.Error("[ConvertJson]Error=" & ex.ToString)

        End Try
    End Function
    Private Function ConverOverKwh(ByVal Value As Double) As Double
        Try
            Dim s As String = ""
            ConverOverKwh = Value
            s = "1" & s.PadLeft(Format(Value, "#").Length, "0")
            ConverOverKwh = CDbl(s) - Value
        Catch ex As Exception
            glogger.Error("[ConverOverKwh]Error=" & ex.ToString)
        End Try
    End Function
    Private Sub DayReport(ByVal odbEEP_Points As dbEEP_Points)
        Try
            Dim Result As String
            Dim sStartDay As String

            Dim mclsDBUtility As clsDBUtility
            Dim oPowerMeterDatas As PowerMeterDatas
            Dim i As Integer
            Dim bAddDBOk As Boolean = False

          
            If Now < odbEEP_Points.LastDayReportTime.AddHours(26) Then
                Exit Sub
            End If
            mclsDBUtility = New clsDBUtility(My.Settings.DBIP, "EEP_Client_" & odbEEP_Points.CompanyID, My.Settings.DBLoginID, My.Settings.DBLoginPassword)

            If mclsDBUtility.OpenDB = True Then
                glogger.Debug("[DayReport Start] CompanyName=" & odbEEP_Points.CompanyName & ",CID=" & odbEEP_Points.LoginCID & ",sStartDay=" & odbEEP_Points.LastDayReportTime)
                If mclsDBUtility.ElectricDayReport(odbEEP_Points.LastDayReportTime) = True Then
                    bAddDBOk = True
                End If
                mclsDBUtility = Nothing
                If bAddDBOk = True Then
                    gdbEEP_Points(odbEEP_Points.CompanyID).LastDayReportTime = odbEEP_Points.LastDayReportTime.AddDays(1)
                    gclsDBUtility.Update_EEP_Points_LastDayReportTime(odbEEP_Points)
                    glogger.Debug("[DayReport End] CompanyName=" & odbEEP_Points.CompanyName & ",CID=" & odbEEP_Points.LoginCID & ",sStartDay=" & sStartDay)
                Else
                    glogger.Debug("[DayReport Fail] CompanyName=" & odbEEP_Points.CompanyName & ",CID=" & odbEEP_Points.LoginCID & ",sStartDay=" & sStartDay)
                End If
             
            Else
                glogger.Debug("[DayReport Open DB Fail] DB=EEP_Client_" & odbEEP_Points.CompanyID & ",CompanyName=" & odbEEP_Points.CompanyName)
            End If

        Catch ex As Exception
            glogger.Error("[DayReport]Error=" & ex.Message)
        End Try
    End Sub
End Module
