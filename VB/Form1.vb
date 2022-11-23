Imports System
Imports System.Drawing
Imports System.Drawing.Printing
Imports System.Windows.Forms
Imports DevExpress.XtraPrinting

' ...
Namespace docDynamicFooterSummaries

    Public Partial Class Form1
        Inherits Form

        Private ps As PrintingSystem = New PrintingSystem()

        Private Operation As String() = New String() {"Count", "Sum"}

        Private sbriks As Brick() = New Brick(1) {}

        Public Sub New()
            InitializeComponent()
            AddHandler ps.BeforePagePaint, New PagePaintEventHandler(AddressOf PagePainting)
        End Sub

        Private Sub PagePainting(ByVal sender As Object, ByVal e As PagePaintEventArgs)
            Dim v As Double, sum As Double = 0
            Dim count As Integer = 0
            ' Perform actions for all bricks on the current page.
            For Each brick As TextBrick In e.Page
                If brick IsNot Nothing Then
                    Dim d As Double
                    If Not(TypeOf brick.TextValue Is String) OrElse Not Double.TryParse(CStr(brick.TextValue), d) Then Continue For
                    v = Convert.ToDouble(brick.TextValue)
                    ' Check the brick identification and perform calculations accordingly. 
                    Select Case brick.ID
                        Case "Count"
                            count += 1
                        Case "Sum"
                            sum += v
                    End Select
                End If
            Next

            ' Assign the resulting values to the bricks, 
            ' which represent totals in the page footer.
            CType(sbriks(0), TextBrick).Text = String.Format("{0:0.##}", count)
            CType(sbriks(1), TextBrick).Text = String.Format("{0:0.##}", sum)
        End Sub

        Private Sub button1_Click(ByVal sender As Object, ByVal e As EventArgs)
            CreateDocument()
            ps.PageSettings.Assign(New Margins(15, 15, 15, 35), PaperKind.Custom, New Size(200, 500), True)
        End Sub

        Private Sub CreateDocument()
            Dim currentCursor As Cursor = Cursor.Current
            Cursor.Current = Cursors.WaitCursor
            CreateTableOfValues()
            ps.PreviewFormEx.Show()
            Cursor.Current = currentCursor
        End Sub

        Private Sub CreateTableOfValues()
            Dim gr As BrickGraphics = ps.Graph
            ps.Begin()
            Dim sum As Double = 0
            Dim count As Integer = 0
            Dim h As Integer = 20
            Dim X As Integer = Convert.ToInt32(Math.Floor(gr.ClientPageSize.Width / 5))
            gr.StringFormat = New BrickStringFormat(StringAlignment.Center, StringAlignment.Center)
            ' Display column headers.
            gr.Modifier = BrickModifier.DetailHeader
            For i As Integer = 0 To 2 - 1
                gr.DrawString(Operation(i), Color.Black, New RectangleF(i * X, 0, X, h), BorderSide.All)
            Next

            ' Display  random values.
            gr.Modifier = BrickModifier.Detail
            gr.BackColor = SystemColors.Window
            gr.Font = New Font("Arial", 8)
            Dim v As Double
            Dim rand As Random = New Random()
            For j As Integer = 0 To 10 - 1
                v = rand.NextDouble() * 100
                For i As Integer = 0 To 2 - 1
                    Dim brick As TextBrick = gr.DrawString(String.Format("{0:0.##}", v), Color.Black, New RectangleF(i * X, j * h, X, h), BorderSide.All)
                    ' Assign the column identificator.
                    brick.ID = Operation(i)
                    ' Assign the value used for subsequent calculations.
                    brick.TextValue = v.ToString()
                Next

                count += 1
                sum += v
            Next

            ' Display the count and sum values in the ReportFooter section 
            ' (at the end of the report).
            gr.Modifier = BrickModifier.ReportFooter
            gr.BackColor = SystemColors.ControlDark
            gr.Font = New Font("Arial", 10, FontStyle.Bold)
            h = gr.Font.Height
            gr.DrawString(String.Format("{0:0.##}", count), Color.White, New RectangleF(0, 0, X, h), BorderSide.All)
            gr.DrawString(String.Format("{0:0.##}", sum), Color.White, New RectangleF(X, 0, X, h), BorderSide.All)
            ' Display summaries for the current page items. 
            gr.Modifier = BrickModifier.MarginalFooter
            gr.StringFormat = gr.StringFormat.ChangeAlignment(StringAlignment.Far)
            For i As Integer = 0 To 2 - 1
                sbriks(i) = gr.DrawString("", Color.Black, New RectangleF(i * X, 0, X, h), BorderSide.All)
            Next

            ps.End()
        End Sub
    End Class
End Namespace
