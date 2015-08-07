﻿Public Class Service1

    Protected Overrides Sub OnStart(ByVal args() As String)
        ' 在此加入啟動服務的程式碼。這個方法必須設定已啟動的
        ' 事項，否則可能導致服務無法工作。
        Try
            iniServiceStart()
        Catch ex As Exception
            glogger.Error("EEP_EIMS_Taigu_PowerMeter_Service OnStart Error=" & ex.ToString)
        End Try
    End Sub

    Protected Overrides Sub OnStop()
        ' 在此加入停止服務所需執行的終止程式碼。
        Try
            ServiceStop()
        Catch ex As Exception
            glogger.Error("EEP_EIMS_Taigu_PowerMeter_Service OnStop Error=" & ex.ToString)
        End Try
    End Sub

End Class
