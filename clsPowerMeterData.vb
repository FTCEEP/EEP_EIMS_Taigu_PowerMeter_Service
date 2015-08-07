
Public Class clsPowerMeter
    Public Property dataList As IList(Of PowerMeterDatas)
    Public Property code As String
End Class
Public Class PowerMeterDatas
    Public Property PLA As String = 0 'A相负荷
    Public Property PLB As String = 0 'B相负荷
    Public Property PLC As String = 0 'C相负荷
    Public Property KWH As String = 0 '累积电量

    Public Property PFA As String = 0 'A相功率因数
    Public Property PFB As String = 0 'B相功率因数
    Public Property PFC As String = 0 'C相功率因数
    Public Property TEMP As String = 0 '温度
    Public Property AA As String = 0 'A相电流
    Public Property AB As String = 0 'B相电流
    Public Property AC As String = 0 'C相电流
    Public Property RECTIME As String = Now.ToString("yyyy-MM-dd HH:mm:ss")
    Public Property PMID As String = 0
    Public Property VA As String = 0 'A相电压
    Public Property VB As String = 0 'B相电压
    Public Property VC As String = 0 'C相电压

    Public Property PFT As String = 0 '总功率因数
    Public Property PLT As String = 0 '总负荷
End Class

