Imports System.Data.SqlClient
Imports System.Net.Mail
Imports System.Collections.Specialized


Public Class dbEEP_Points
    Public CompanyName As String = ""
    Public CompanyID As String = ""
    Public PointIDForSum As String = ""
    Public LoginUser As String = ""
    Public LoginPW As String = ""
    Public LastGetDataTime As DateTime
    Public LastKwhByHr As Double = 0
    Public LoginCID As String = ""
    Public MinKwhByHr As Double = -1
    Public MaxKwhByHr As Double = 0
    Public MinKwhRecTime As DateTime
    Public MaxKwhRecTime As DateTime
    Public LastPointListValue As Dictionary(Of String, PowermeterData)
    Public LastGetAllDataTime As DateTime
    Public LastDayReportTime As DateTime
End Class
Public Class PowermeterData
    Public MinKwhByHr As Double = -1
    Public MaxKwhByHr As Double = 0
    Public LastValue As Double = 0
    Public MinKwhRecTime As DateTime
    Public MaxKwhRecTime As DateTime
End Class
Public Class clsDBUtility
    Private logger As NLog.Logger = NLog.LogManager.GetCurrentClassLogger

    Private cn As New SqlConnection()

    Public OpenDB As Boolean = False

    Public iDBErrorCount As Integer = 0
    ''' <summary>
    ''' 連接DB系統初始化
    ''' </summary>
    ''' <param name="DB_IP">DB主機 IP</param>
    ''' <param name="LoginID">DB登入帳號</param>
    ''' <param name="LoginPassword">DB登入帳號</param>
    ''' <remarks></remarks>
    '''  
    Public Sub New(ByVal DB_IP As String, ByVal Catalog As String, ByVal LoginID As String, ByVal LoginPassword As String)
        Try
            Dim DBConnectString As String = "Data Source=" & DB_IP & ";Initial Catalog=" & Catalog & ";Persist Security Info=True;User ID=" & LoginID & ";Password=" & LoginPassword
            cn.ConnectionString = DBConnectString
            If cn.State = ConnectionState.Closed Then
                cn.Open()
                OpenDB = True

            End If
        Catch ex As Exception
            iDBErrorCount = iDBErrorCount + 1
            logger.Error("[clsDBUtility New Error]" & ex.ToString)
        End Try
    End Sub
    Protected Overridable Sub Dispose(ByVal disposing As Boolean)
        Try
            If cn.State = ConnectionState.Open Then
                cn.Close()
                OpenDB = False
            End If
        Catch ex As Exception
            logger.Error("[clsDBUtility Dispose New Error]" & ex.ToString)
        End Try
    End Sub
    Public Function CheckSettingChange() As Boolean
        Try
            CheckSettingChange = False
            Dim sSql2 As String
            Dim i As Integer
            sSql2 = "select count(*) from SettingChange"
            i = ExecSQLGetCount(sSql2)
            If i > 0 Then
                CheckSettingChange = True
                sSql2 = "delete SettingChange"
                ExecSQLNonQuery(sSql2)
            End If
        Catch ex As Exception
            logger.Error("[CheckSettingChange New Error]" & ex.ToString)
        End Try
    End Function

    Public Function GetEEP_Points() As Dictionary(Of String, dbEEP_Points)
        Try
            Dim dt As New DataTable
            Dim i As Integer
            Dim j As Integer
            Dim odbEEP_Points As dbEEP_Points
            Dim odbEEP_PointsList As New Dictionary(Of String, dbEEP_Points)
            Dim stSumID As New Dictionary(Of String, PowermeterData)
            Dim sSql2 As String
            sSql2 = "select * from EEP_Points"
            dt = ExecSQLToDataTable(sSql2)
            If dt Is Nothing Then
                Return odbEEP_PointsList
            End If

            If dt.Rows.Count > 0 Then
                For i = 0 To dt.Rows.Count - 1
                    stSumID = New Dictionary(Of String, PowermeterData)
                    odbEEP_Points = New dbEEP_Points
                    odbEEP_Points.CompanyName = dt.Rows(i)("CompanyName").ToString
                    odbEEP_Points.CompanyID = dt.Rows(i)("CompanyID").ToString
                    odbEEP_Points.PointIDForSum = dt.Rows(i)("PointIDForSum").ToString
                    odbEEP_Points.LoginUser = dt.Rows(i)("LoginUser").ToString
                    odbEEP_Points.LoginPW = dt.Rows(i)("LoginPW").ToString
                    odbEEP_Points.LastGetDataTime = dt.Rows(i)("LastGetDataTime").ToString
                    odbEEP_Points.LoginCID = dt.Rows(i)("LoginCID")
                    odbEEP_Points.LastGetAllDataTime = dt.Rows(i)("LastGetAllDataTime").ToString
                    odbEEP_Points.LastDayReportTime = dt.Rows(i)("LastDayReportTime").ToString
                    For j = 0 To odbEEP_Points.PointIDForSum.Split(",").Length - 1
                        If stSumID.ContainsKey(odbEEP_Points.PointIDForSum.Split(",")(j)) = False Then
                            Dim oPowermeterData As New PowermeterData
                            stSumID.Add(odbEEP_Points.PointIDForSum.Split(",")(j), oPowermeterData)
                        End If
                    Next
                    odbEEP_Points.LastPointListValue = stSumID

                    If odbEEP_PointsList.ContainsKey(odbEEP_Points.CompanyID) = False Then
                        odbEEP_PointsList.Add(odbEEP_Points.CompanyID, odbEEP_Points)
                    End If
                Next
                GetEEP_Points = odbEEP_PointsList
            End If
        Catch ex As Exception
            iDBErrorCount = iDBErrorCount + 1
            logger.Error("[GetEEP_Points Error]" & ex.ToString)
        End Try
    End Function



    ''' <summary>
    ''' 更新資料庫 SysDeviceStatus 跟ServiceAlive table相關資訊
    ''' </summary>
    ''' <param name="ServiceName">ServiceName</param>
    ''' <returns>False=失敗，True=成功</returns>
    ''' <remarks></remarks>
    Public Function UpdateServiceAliveInfo(ByVal ServiceName As String, ByVal SiPassStatus As Boolean) As Boolean
        Try
            UpdateServiceAliveInfo = False
            Dim sSql2 As String

            Dim sSiPassStatus As String = "0"
            If SiPassStatus = True Then
                sSiPassStatus = "0"
            Else
                sSiPassStatus = "1"
            End If
            sSql2 = "delete ServiceAlive where ServiceName='" & ServiceName & "'"
            ExecSQLNonQuery(sSql2)
            sSql2 = "insert into ServiceAlive(ServiceName,LatestAlive) values('" & ServiceName & _
                       "','" & Now.ToString("yyyy/MM/dd HH:mm:ss") & "')"
            ExecSQLNonQuery(sSql2)


            UpdateServiceAliveInfo = True
            'logger.Debug("[UpdateServiceAliveInfo Success]")
        Catch ex As Exception
            iDBErrorCount = iDBErrorCount + 1
            logger.Error("[UpdateServiceAliveInfo Error]" & ex.ToString)
        End Try
    End Function
    ''' <summary>
    ''' 提供資料庫查詢結果
    ''' </summary>
    ''' <param name="SQL">SQL Script</param> 
    ''' <returns>DataTable</returns>
    ''' <remarks></remarks>
    Private Function ExecSQLToDataTable(ByVal SQL As String) As DataTable
        Try
            Dim da As New SqlDataAdapter(SQL, cn)
            Dim cm As New SqlCommand(SQL, cn)
            Dim dt As New DataTable

            If cn.State = ConnectionState.Closed Then
                cn.Open()
            End If
            da.SelectCommand.CommandText = SQL
            da.Fill(dt)
            ExecSQLToDataTable = dt
            iDBErrorCount = 0
        Catch ex As Exception
            iDBErrorCount = iDBErrorCount + 1
            logger.Error("[ExecSQLToDataTable ErrorCount=" & iDBErrorCount & "]SQL=" & SQL & ",Error=" & ex.ToString)
        End Try
    End Function
    ''' <summary>
    ''' 提供資料庫執行結果
    ''' </summary>
    ''' <param name="SQL">SQL Script</param>
    ''' <returns>False=失敗，True=成功</returns>
    ''' <remarks></remarks>
    Private Function ExecSQLNonQuery(ByVal SQL As String) As Boolean
        Try
            Dim dt As New DataTable
            Dim da As New SqlDataAdapter(SQL, cn)
            Dim cm As New SqlCommand(SQL, cn)
            If cn.State = ConnectionState.Closed Then
                cn.Open()
            End If
            cm.CommandText = SQL
            cm.ExecuteNonQuery()
            iDBErrorCount = 0
        Catch ex As Exception
            iDBErrorCount = iDBErrorCount + 1
            logger.Error("[ExecSQLNonQuery Error]SQL=" & SQL & ",Error=" & ex.ToString)
        End Try
    End Function
    ''' <summary>
    ''' 提供資料庫查詢結果的筆數
    ''' </summary>
    ''' <param name="SQL">SQL Script</param>
    ''' <returns>Integer</returns>
    ''' <remarks></remarks>
    Private Function ExecSQLGetCount(ByVal SQL As String) As Integer
        Try
            ExecSQLGetCount = 0
            Dim da As New SqlDataAdapter(SQL, cn)
            Dim cm As New SqlCommand(SQL, cn)
            If cn.State = ConnectionState.Closed Then
                cn.Open()
            End If
            cm.CommandText = SQL
            ExecSQLGetCount = cm.ExecuteScalar
            iDBErrorCount = 0
        Catch ex As Exception
            iDBErrorCount = iDBErrorCount + 1
            logger.Error("[ExecSQLGetCount Error]SQL=" & SQL & ",Error=" & ex.ToString)

        End Try
    End Function
    Private Function ExecSQLGetValue(ByVal SQL As String) As String
        Try
            ExecSQLGetValue = ""
            Dim da As New SqlDataAdapter(SQL, cn)
            Dim cm As New SqlCommand(SQL, cn)
            If cn.State = ConnectionState.Closed Then
                cn.Open()
            End If
            cm.CommandText = SQL
            ExecSQLGetValue = cm.ExecuteScalar
            iDBErrorCount = 0
        Catch ex As Exception
            iDBErrorCount = iDBErrorCount + 1
            logger.Error("[ExecSQLGetCount Error]SQL=" & SQL & ",Error=" & ex.ToString)

        End Try
    End Function
    Public Function InsertHistoryData(ByVal oPowerMeterDatas As PowerMeterDatas, ByVal odbEEP_Points As dbEEP_Points) As Boolean
        Try
            InsertHistoryData = False
            Dim sSql2 As String
            If IsNumeric(oPowerMeterDatas.KWH) = True Then
                sSql2 = "insert into EEP_HistoryData_EIMS_DataCenter_" & odbEEP_Points.LastGetDataTime.Year & "(CompanyID,PointID,PointValue,RecTime) values('" & _
                             odbEEP_Points.CompanyID & "','" & oPowerMeterDatas.PMID & "','" & oPowerMeterDatas.KWH & "','" & oPowerMeterDatas.RECTIME & "')"
                ExecSQLNonQuery(sSql2)

                sSql2 = "update EEP_Points set LastGetDataTime='" & odbEEP_Points.LastGetDataTime.AddMinutes(15).ToString("yyyy/MM/dd HH:mm:ss") & "' where CompanyID='" & odbEEP_Points.CompanyID & "'"
                ExecSQLNonQuery(sSql2)
                InsertHistoryData = True
            Else
                logger.Error("[InsertHistoryData KWH not Numeric]CompanyID=" & odbEEP_Points.CompanyID & ",PMID=" & oPowerMeterDatas.PMID & ",KWH=" & oPowerMeterDatas.KWH & ",RECTIME=" & oPowerMeterDatas.RECTIME)
            End If

            'logger.Debug("[InsertHistoryData Success]")
        Catch ex As Exception
            iDBErrorCount = iDBErrorCount + 1
            logger.Error("[InsertHistoryData Error]" & ex.ToString)
        End Try
    End Function
    Public Function Update_EEP_Points_LastGetAllDataTime(ByVal odbEEP_Points As dbEEP_Points) As Boolean
        Try
            Update_EEP_Points_LastGetAllDataTime = False
            Dim sSql2 As String

            sSql2 = "update EEP_Points set LastGetAllDataTime='" & odbEEP_Points.LastGetAllDataTime.ToString("yyyy-MM-dd HH:mm:ss") & "' where CompanyID='" & odbEEP_Points.CompanyID & "'"
            ExecSQLNonQuery(sSql2)

        Catch ex As Exception
            iDBErrorCount = iDBErrorCount + 1
            logger.Error("[Update_EEP_Points_LastGetAllDataTime Error]" & ex.ToString)
        End Try
    End Function
    Public Function Update_EEP_Points_LastDayReportTime(ByVal odbEEP_Points As dbEEP_Points) As Boolean
        Try
            Update_EEP_Points_LastDayReportTime = False
            Dim sSql2 As String

            sSql2 = "update EEP_Points set LastDayReportTime='" & odbEEP_Points.LastDayReportTime.ToString("yyyy-MM-dd HH:mm:ss") & "' where CompanyID='" & odbEEP_Points.CompanyID & "'"
            ExecSQLNonQuery(sSql2)

        Catch ex As Exception
            iDBErrorCount = iDBErrorCount + 1
            logger.Error("[Update_EEP_Points_LastDayReportTime Error]" & ex.ToString)
        End Try
    End Function
    Public Function InsertRawData(ByVal oPowerMeterDatas As PowerMeterDatas, ByVal odbEEP_Points As dbEEP_Points) As Boolean
        Try
            InsertRawData = False
            Dim sSql2 As String

            sSql2 = "insert into BwAnalogTable(TagName,RecTime,MaxValue) values('" & _
                    oPowerMeterDatas.PMID & ".AA','" & oPowerMeterDatas.RECTIME & "','" & oPowerMeterDatas.AA & "')"
            ExecSQLNonQuery(sSql2)

            sSql2 = "insert into BwAnalogTable(TagName,RecTime,MaxValue) values('" & _
                  oPowerMeterDatas.PMID & ".AB','" & oPowerMeterDatas.RECTIME & "','" & oPowerMeterDatas.AB & "')"
            ExecSQLNonQuery(sSql2)

            sSql2 = "insert into BwAnalogTable(TagName,RecTime,MaxValue) values('" & _
                 oPowerMeterDatas.PMID & ".AC','" & oPowerMeterDatas.RECTIME & "','" & oPowerMeterDatas.AC & "')"
            ExecSQLNonQuery(sSql2)

            sSql2 = "insert into BwAnalogTable(TagName,RecTime,MaxValue) values('" & _
                 oPowerMeterDatas.PMID & ".KWH','" & oPowerMeterDatas.RECTIME & "','" & oPowerMeterDatas.KWH & "')"
            ExecSQLNonQuery(sSql2)

            sSql2 = "insert into BwAnalogTable(TagName,RecTime,MaxValue) values('" & _
                 oPowerMeterDatas.PMID & ".PFA','" & oPowerMeterDatas.RECTIME & "','" & oPowerMeterDatas.PFA & "')"
            ExecSQLNonQuery(sSql2)

            sSql2 = "insert into BwAnalogTable(TagName,RecTime,MaxValue) values('" & _
                 oPowerMeterDatas.PMID & ".PFB','" & oPowerMeterDatas.RECTIME & "','" & oPowerMeterDatas.PFB & "')"
            ExecSQLNonQuery(sSql2)

            sSql2 = "insert into BwAnalogTable(TagName,RecTime,MaxValue) values('" & _
                 oPowerMeterDatas.PMID & ".PFC','" & oPowerMeterDatas.RECTIME & "','" & oPowerMeterDatas.PFC & "')"
            ExecSQLNonQuery(sSql2)

            sSql2 = "insert into BwAnalogTable(TagName,RecTime,MaxValue) values('" & _
                 oPowerMeterDatas.PMID & ".PLA','" & oPowerMeterDatas.RECTIME & "','" & oPowerMeterDatas.PLA & "')"
            ExecSQLNonQuery(sSql2)

            sSql2 = "insert into BwAnalogTable(TagName,RecTime,MaxValue) values('" & _
                 oPowerMeterDatas.PMID & ".PLB','" & oPowerMeterDatas.RECTIME & "','" & oPowerMeterDatas.PLB & "')"
            ExecSQLNonQuery(sSql2)

            sSql2 = "insert into BwAnalogTable(TagName,RecTime,MaxValue) values('" & _
                 oPowerMeterDatas.PMID & ".PLC','" & oPowerMeterDatas.RECTIME & "','" & oPowerMeterDatas.PLC & "')"
            ExecSQLNonQuery(sSql2)

            sSql2 = "insert into BwAnalogTable(TagName,RecTime,MaxValue) values('" & _
                 oPowerMeterDatas.PMID & ".PLT','" & oPowerMeterDatas.RECTIME & "','" & oPowerMeterDatas.PLT & "')"
            ExecSQLNonQuery(sSql2)

            sSql2 = "insert into BwAnalogTable(TagName,RecTime,MaxValue) values('" & _
              oPowerMeterDatas.PMID & ".TEMP','" & oPowerMeterDatas.RECTIME & "','" & oPowerMeterDatas.TEMP & "')"
            ExecSQLNonQuery(sSql2)

            sSql2 = "insert into BwAnalogTable(TagName,RecTime,MaxValue) values('" & _
            oPowerMeterDatas.PMID & ".VA','" & oPowerMeterDatas.RECTIME & "','" & oPowerMeterDatas.VA & "')"
            ExecSQLNonQuery(sSql2)

            sSql2 = "insert into BwAnalogTable(TagName,RecTime,MaxValue) values('" & _
            oPowerMeterDatas.PMID & ".VB','" & oPowerMeterDatas.RECTIME & "','" & oPowerMeterDatas.VB & "')"
            ExecSQLNonQuery(sSql2)

            sSql2 = "insert into BwAnalogTable(TagName,RecTime,MaxValue) values('" & _
            oPowerMeterDatas.PMID & ".VC','" & oPowerMeterDatas.RECTIME & "','" & oPowerMeterDatas.VC & "')"
            ExecSQLNonQuery(sSql2)

            sSql2 = "insert into BwAnalogTable(TagName,RecTime,MaxValue) values('" & _
           oPowerMeterDatas.PMID & ".PFT','" & oPowerMeterDatas.RECTIME & "','" & oPowerMeterDatas.PFT & "')"
            ExecSQLNonQuery(sSql2)

            InsertRawData = True

        Catch ex As Exception
            iDBErrorCount = iDBErrorCount + 1
            logger.Error("[InsertRawData Error]" & ex.ToString)
        End Try
    End Function
    Public Function InsertRealTimeData(ByVal oPowerMeterDatas As PowerMeterDatas, ByVal odbEEP_Points As dbEEP_Points) As Boolean
        Try
            InsertRealTimeData = False
            Dim sSql2 As String
            If IsNumeric(oPowerMeterDatas.KWH) = True Then
                sSql2 = "delete EEP_RealData_EIMS_DataCenter where CompanyID='" & odbEEP_Points.CompanyID & "' and PointID='" & oPowerMeterDatas.PMID & "'"
                ExecSQLNonQuery(sSql2)

                sSql2 = "insert into EEP_RealData_EIMS_DataCenter(CompanyID,PointID,PointValue,RecTime) values('" & _
                             odbEEP_Points.CompanyID & "','" & oPowerMeterDatas.PMID & "','" & oPowerMeterDatas.KWH & "','" & oPowerMeterDatas.RECTIME & "')"
                ExecSQLNonQuery(sSql2)
            Else
                logger.Error("[InsertRealTimeData KWH not Numeric]CompanyID=" & odbEEP_Points.CompanyID & ",PMID=" & oPowerMeterDatas.PMID & ",KWH=" & oPowerMeterDatas.KWH & ",RECTIME=" & oPowerMeterDatas.RECTIME)
            End If

            'logger.Debug("[InsertHistoryData Success]")
        Catch ex As Exception
            iDBErrorCount = iDBErrorCount + 1
            logger.Error("[InsertRealTimeData Error]" & ex.ToString)
        End Try
    End Function
    Public Function GetHistoryData(ByVal oPowerMeterDatas As PowerMeterDatas, ByVal odbEEP_Points As dbEEP_Points) As Double
        Try
            Dim sSql2 As String
            Dim RecTime As DateTime '= odbEEP_Points.LastGetDataTime.ToString("yyyy-MM-dd HH:00:00")
            Dim sResult As New DataTable
            Dim PointID As String
            Dim PointValue As String

            Dim i As Integer
            GetHistoryData = 0
            If odbEEP_Points.MinKwhByHr = -1 Then
                'sSql2 = "select top 1 PointValue from EEP_HistoryData_EIMS_DataCenter_" & odbEEP_Points.LastGetDataTime.Year & _
                '                 " where CompanyID='" & odbEEP_Points.CompanyID & "' and PointID='" & oPowerMeterDatas.PMID & "'" & _
                '                 " and RecTime<='" & RecTime & "' order by RecTime desc"
                sSql2 = "select * from EEP_RealData_EIMS_DataCenter" & _
                                " where CompanyID='" & odbEEP_Points.CompanyID & "' and PointID='" & oPowerMeterDatas.PMID & "'"
                sResult = ExecSQLToDataTable(sSql2)
                If sResult.Rows.Count > 0 Then
                    For i = 0 To sResult.Rows.Count - 1
                        PointID = sResult.Rows(i)("PointID").ToString
                        PointValue = sResult.Rows(i)("PointValue").ToString
                        RecTime = sResult.Rows(i)("RecTime")
                        gdbEEP_Points(odbEEP_Points.CompanyID).LastPointListValue(PointID).LastValue = CDbl(PointValue)
                        gdbEEP_Points(odbEEP_Points.CompanyID).LastPointListValue(PointID).MinKwhRecTime = RecTime
                        gdbEEP_Points(odbEEP_Points.CompanyID).LastPointListValue(PointID).MinKwhByHr = CDbl(PointValue)
                    Next

                End If
                'If IsNumeric(sResult) = True Then
                '    gdbEEP_Points(odbEEP_Points.CompanyID).MinKwhByHr = CDbl(sResult)
                '    gdbEEP_Points(odbEEP_Points.CompanyID).MinKwhRecTime = CDate(RecTime)
                'End If
            End If

        Catch ex As Exception
            iDBErrorCount = iDBErrorCount + 1
            logger.Error("[GetHistoryData Error]" & ex.ToString)
        End Try
    End Function
    Public Function InsertHistoryDataByDay(ByVal CompanyID As String, ByVal RecTime As DateTime, ByVal KWH As String) As Boolean
        Try
            InsertHistoryDataByDay = False
            Dim sSql2 As String
            If IsNumeric(KWH) = True Then
                sSql2 = "insert into PowerMeter_Day_" & RecTime.Year & "(CompanyID,RecTime,KWH) values('" & _
                             CompanyID & "','" & RecTime.ToString("yyyy-MM-dd HH:mm:ss") & "','" & KWH & "')"
                ExecSQLNonQuery(sSql2)
                InsertHistoryDataByDay = True
            Else
                logger.Error("[InsertHistoryDataByDay KWH not Numeric]CompanyID=" & CompanyID & ",RecTime=" & RecTime & ",KWH=" & KWH)
            End If

        Catch ex As Exception
            iDBErrorCount = iDBErrorCount + 1
            logger.Error("[InsertHistoryDataByDay Error]" & ex.ToString)
        End Try
    End Function

    Public Function ElectricDayReport(ByVal LastUpdateTime As DateTime) As Boolean
        Try
            ElectricDayReport = False
            Dim sSql2 As String
            Dim dt As New DataTable
            Dim dt2 As New DataTable
            Dim dt3 As New DataTable
            Dim i As Integer
            Dim j As Integer
            Dim stPMID As New Dictionary(Of String, CalKWH)
            Dim clsDayReport As DayReport
            Dim clsCalKWH As CalKWH
            Dim TagName As String
            Dim stTagName As New StringDictionary
            Dim TagNameList As String
            sSql2 = "delete ElectricDayReport where RecTime>='" & LastUpdateTime.ToString("yyyy-MM-dd") & _
                   "' and RecTime<'" & LastUpdateTime.AddDays(1).ToString("yyyy-MM-dd") & "'"
            ExecSQLNonQuery(sSql2)

            sSql2 = "select * from ElectricCostType"
            dt = ExecSQLToDataTable(sSql2)

            sSql2 = "select distinct TagName from BwAnalogTable with(nolock) where RecTime>='" & LastUpdateTime.ToString("yyyy-MM-dd") & _
                    "' and RecTime<='" & LastUpdateTime.AddDays(1).ToString("yyyy-MM-dd") & "'"
            dt3 = ExecSQLToDataTable(sSql2)
            If dt3.Rows.Count = 0 Then

            End If
            For i = 0 To dt3.Rows.Count - 1
                TagName = dt3.Rows(i)("TagName").ToString
                If InStr(TagName, "KWH") > 0 Then
                    If stTagName.ContainsKey(TagName) = False Then
                        stTagName.Add(TagName, TagName)
                        If TagNameList = "" Then
                            TagNameList = TagName
                        Else
                            TagNameList = TagNameList & "," & TagName
                        End If
                    End If
                End If
            Next
            If dt3.Rows.Count = 0 Then
                glogger.Debug("[DayReport Start2] Can not find Data,LastUpdateTime=" & LastUpdateTime)
                Return True
            End If
            TagNameList = TagNameList.Replace(",", "','")
            sSql2 = "select * from BwAnalogTable with(nolock) where RecTime>='" & LastUpdateTime.ToString("yyyy-MM-dd") & _
                    "' and RecTime<='" & LastUpdateTime.AddDays(1).ToString("yyyy-MM-dd") & _
                    "' and TagName in ('" & TagNameList & "') order by TagName,RecTime"
            dt2 = ExecSQLToDataTable(sSql2)
            For i = 0 To dt2.Rows.Count - 1

                TagName = dt2.Rows(i)("TagName").ToString
                If stPMID.ContainsKey(TagName) = False Then
                    clsCalKWH = New CalKWH
                    clsCalKWH.MinKWh = dt2.Rows(i)("MaxValue")
                    clsCalKWH.HH = CDate(dt2.Rows(i)("RecTime")).Hour
                    stPMID.Add(TagName, clsCalKWH)
                Else
                    clsCalKWH = stPMID(TagName)
                    clsCalKWH.MaxKWh = dt2.Rows(i)("MaxValue")
                    If clsCalKWH.HH <> CDate(dt2.Rows(i)("RecTime")).Hour Then

                        clsDayReport = New DayReport
                        clsDayReport.TagID = TagName
                        clsDayReport.RecTime = LastUpdateTime.ToString("yyyy-MM-dd") & " " & clsCalKWH.HH.ToString.PadLeft(2, "0") & ":00"
                        clsDayReport.KWH = clsCalKWH.MaxKWh - clsCalKWH.MinKWh
                        GetElectricCostType(clsDayReport, dt)
                        If InsertDayReport(clsDayReport) = True Then

                        Else
                            Exit Function
                        End If
                        clsCalKWH.HH = CDate(dt2.Rows(i)("RecTime")).Hour
                        clsCalKWH.MinKWh = dt2.Rows(i)("MaxValue")
                    End If
                End If
            Next
            ElectricDayReport = True
        Catch ex As Exception
            iDBErrorCount = iDBErrorCount + 1
            logger.Error("[ElectricDayReport Error]" & ex.ToString)
        End Try
    End Function
    Private Sub GetElectricCostType(ByRef clsDayReport As DayReport, ByRef dt As DataTable)
        Try
            Dim i As Integer
            Dim ElectricCostType, ElectricCostTypeName, StartTime, EndTime, UnitCost, OldUnitCost As String
            Dim RecTime As String

            For i = 0 To dt.Rows.Count - 1
                ElectricCostType = dt.Rows(i)("ElectricCostType")
                ElectricCostTypeName = dt.Rows(i)("ElectricCostTypeName")
                StartTime = dt.Rows(i)("StartTime")
                EndTime = dt.Rows(i)("EndTime")
                UnitCost = dt.Rows(i)("UnitCost")
                OldUnitCost = dt.Rows(i)("OldUnitCost")
                RecTime = CDate(clsDayReport.RecTime).ToString("HH:mm")
                If RecTime >= StartTime And RecTime < EndTime Then
                    clsDayReport.UnitElectricCost = UnitCost
                    clsDayReport.OldUnitElectricCost = OldUnitCost
                    clsDayReport.ElectricCost = clsDayReport.KWH * CDbl(UnitCost)
                    clsDayReport.oldElectricCost = clsDayReport.KWH * CDbl(OldUnitCost)
                    clsDayReport.ElectricCostType01 = ElectricCostType
                    clsDayReport.ElectricCostTypePCT01 = 1
                    clsDayReport.ElectricCostType02 = ""
                    clsDayReport.ElectricCostTypePCT02 = 0
                    Exit For
                End If
            Next
        Catch ex As Exception
            logger.Error("[GetElectricCostType Error]" & ex.ToString)
        End Try
    End Sub
    Public Function InsertDayReport(ByVal clsDayReport As DayReport) As Boolean
        Dim sSql2 As String = ""
        Try
            InsertDayReport = False


            sSql2 = "insert into ElectricDayReport(TagID,RecTime,KWH,UnitElectricCost,OldUnitElectricCost,ElectricCost" & _
                    ",oldElectricCost,ElectricCostType01,ElectricCostTypePCT01,ElectricCostType02,ElectricCostTypePCT02) values('" & _
                    clsDayReport.TagID & "','" & clsDayReport.RecTime & "','" & clsDayReport.KWH & _
                    "','" & clsDayReport.UnitElectricCost & "','" & clsDayReport.OldUnitElectricCost & _
                    "','" & clsDayReport.ElectricCost & "','" & clsDayReport.oldElectricCost & "','" & _
                    clsDayReport.ElectricCostType01 & "','" & clsDayReport.ElectricCostTypePCT01 & _
                    "','" & clsDayReport.ElectricCostType02 & "','" & clsDayReport.ElectricCostTypePCT02 & "')"
            ExecSQLNonQuery(sSql2)


            InsertDayReport = True

        Catch ex As Exception
            iDBErrorCount = iDBErrorCount + 1
            logger.Error("[InsertDayReport Error]SQL=" & sSql2 & ",Error=" & ex.ToString)
        End Try
    End Function
    Public Class DayReport
        Public TagID As String
        Public RecTime As String
        Public KWH As Double
        Public UnitElectricCost As Double
        Public OldUnitElectricCost As Double
        Public ElectricCost As Double
        Public oldElectricCost As Double
        Public ElectricCostType01 As String
        Public ElectricCostTypePCT01 As Double
        Public ElectricCostType02 As String
        Public ElectricCostTypePCT02 As Double
    End Class
    Public Class CalKWH
        Public HH As Integer = 0
        Public MinKWh As Double = 0
        Public MaxKWh As Double = 0
    End Class
End Class
