<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Form1
    Inherits System.Windows.Forms.Form

    'Form 覆寫 Dispose 以清除元件清單。
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    '為 Windows Form 設計工具的必要項
    Private components As System.ComponentModel.IContainer

    '注意:  以下為 Windows Form 設計工具所需的程序
    '可以使用 Windows Form 設計工具進行修改。
    '請不要使用程式碼編輯器進行修改。
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Me.Button1 = New System.Windows.Forms.Button()
        Me.Button2 = New System.Windows.Forms.Button()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.Timer1 = New System.Windows.Forms.Timer(Me.components)
        Me.Timer2 = New System.Windows.Forms.Timer(Me.components)
        Me.Label2 = New System.Windows.Forms.Label()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.Timer01 = New System.Windows.Forms.Label()
        Me.Timer02 = New System.Windows.Forms.Label()
        Me.Button3 = New System.Windows.Forms.Button()
        Me.BackgroundWorker1 = New System.ComponentModel.BackgroundWorker()
        Me.SuspendLayout()
        '
        'Button1
        '
        Me.Button1.Location = New System.Drawing.Point(182, 84)
        Me.Button1.Name = "Button1"
        Me.Button1.Size = New System.Drawing.Size(96, 21)
        Me.Button1.TabIndex = 0
        Me.Button1.Text = "iniServiceStart"
        Me.Button1.UseVisualStyleBackColor = True
        '
        'Button2
        '
        Me.Button2.Location = New System.Drawing.Point(103, 31)
        Me.Button2.Name = "Button2"
        Me.Button2.Size = New System.Drawing.Size(96, 21)
        Me.Button2.TabIndex = 1
        Me.Button2.Text = "Button2"
        Me.Button2.UseVisualStyleBackColor = True
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(144, 110)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(37, 12)
        Me.Label1.TabIndex = 2
        Me.Label1.Text = "Label1"
        '
        'Timer1
        '
        Me.Timer1.Interval = 1000
        '
        'Timer2
        '
        Me.Timer2.Interval = 1000
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Location = New System.Drawing.Point(40, 173)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(45, 12)
        Me.Label2.TabIndex = 3
        Me.Label2.Text = "Timer01"
        '
        'Label3
        '
        Me.Label3.AutoSize = True
        Me.Label3.Location = New System.Drawing.Point(40, 209)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(45, 12)
        Me.Label3.TabIndex = 4
        Me.Label3.Text = "Timer02"
        '
        'Timer01
        '
        Me.Timer01.AutoSize = True
        Me.Timer01.Location = New System.Drawing.Point(101, 173)
        Me.Timer01.Name = "Timer01"
        Me.Timer01.Size = New System.Drawing.Size(11, 12)
        Me.Timer01.TabIndex = 5
        Me.Timer01.Text = "0"
        '
        'Timer02
        '
        Me.Timer02.AutoSize = True
        Me.Timer02.Location = New System.Drawing.Point(101, 209)
        Me.Timer02.Name = "Timer02"
        Me.Timer02.Size = New System.Drawing.Size(11, 12)
        Me.Timer02.TabIndex = 6
        Me.Timer02.Text = "0"
        '
        'Button3
        '
        Me.Button3.Location = New System.Drawing.Point(146, 191)
        Me.Button3.Name = "Button3"
        Me.Button3.Size = New System.Drawing.Size(96, 21)
        Me.Button3.TabIndex = 7
        Me.Button3.Text = "Button3"
        Me.Button3.UseVisualStyleBackColor = True
        '
        'BackgroundWorker1
        '
        Me.BackgroundWorker1.WorkerReportsProgress = True
        Me.BackgroundWorker1.WorkerSupportsCancellation = True
        '
        'Form1
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 12.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(284, 262)
        Me.Controls.Add(Me.Button3)
        Me.Controls.Add(Me.Timer02)
        Me.Controls.Add(Me.Timer01)
        Me.Controls.Add(Me.Label3)
        Me.Controls.Add(Me.Label2)
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.Button2)
        Me.Controls.Add(Me.Button1)
        Me.Name = "Form1"
        Me.Text = "Form1"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents Button1 As System.Windows.Forms.Button
    Friend WithEvents Button2 As System.Windows.Forms.Button
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents Timer1 As System.Windows.Forms.Timer
    Friend WithEvents Timer2 As System.Windows.Forms.Timer
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents Label3 As System.Windows.Forms.Label
    Friend WithEvents Timer01 As System.Windows.Forms.Label
    Friend WithEvents Timer02 As System.Windows.Forms.Label
    Friend WithEvents Button3 As System.Windows.Forms.Button
    Friend WithEvents BackgroundWorker1 As System.ComponentModel.BackgroundWorker
End Class
