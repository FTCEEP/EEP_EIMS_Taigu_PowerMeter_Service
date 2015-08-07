Public Class Form1

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Label1.Text = "Start"
        iniServiceStart()
        Label1.Text = "End"
        MsgBox("ok")
    End Sub

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        Label1.Text = "Start"
        ScanJob()
        Label1.Text = "End"
        MsgBox("ok")
    End Sub

    Private Sub Button3_Click(sender As Object, e As EventArgs) Handles Button3.Click
        Try
            Timer1.Enabled = True
            Timer2.Enabled = True
        Catch ex As Exception

        End Try
    End Sub

    Private Sub Timer1_Tick(sender As Object, e As EventArgs) Handles Timer1.Tick
        Try

            Timer01.Text = CInt(Timer01.Text) + 1
            System.Threading.Thread.Sleep(5100)
            'BackgroundWorker1.RunWorkerAsync()
            'Timer1.Enabled = False
        Catch ex As Exception

        End Try
    End Sub

    Private Sub Timer2_Tick(sender As Object, e As EventArgs) Handles Timer2.Tick
        Try
            Timer02.Text = CInt(Timer02.Text) + 1
        Catch ex As Exception

        End Try
    End Sub

    Private Sub BackgroundWorker1_DoWork(sender As Object, e As ComponentModel.DoWorkEventArgs) Handles BackgroundWorker1.DoWork
        Try
            System.Threading.Thread.Sleep(5000)
        Catch ex As Exception

        End Try
    End Sub

    Private Sub BackgroundWorker1_RunWorkerCompleted(sender As Object, e As ComponentModel.RunWorkerCompletedEventArgs) Handles BackgroundWorker1.RunWorkerCompleted
        Try
            Timer01.Text = CInt(Timer01.Text) + 1
            Timer1.Enabled = True
        Catch ex As Exception

        End Try
    End Sub
End Class