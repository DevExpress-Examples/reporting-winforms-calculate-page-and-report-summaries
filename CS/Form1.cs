using System;
using System.Drawing;
using System.Drawing.Printing;
using System.Windows.Forms;
using DevExpress.XtraPrinting;
// ...

namespace docDynamicFooterSummaries {
    public partial class Form1 : Form {

        private PrintingSystem ps = new PrintingSystem();
        private string[] Operation = new string[] { "Count", "Sum" };
        private Brick[] sbriks = new Brick[2];


        public Form1() {
            InitializeComponent();
            ps.BeforePagePaint += new PagePaintEventHandler(PagePainting);
        }


        private void PagePainting(object sender, PagePaintEventArgs e) {
            double sum = 0, v;
            int count = 0;

            // Perform actions for all bricks on the current page.
            foreach (TextBrick brick in e.Page) {
                if (brick != null) {
                    double d;
                    if (!(brick.TextValue is string) || !double.TryParse((string)brick.TextValue, out d)) 
                        continue;
                    v = Convert.ToDouble(brick.TextValue);

                    // Check the brick identification and perform calculations accordingly. 
                    switch (brick.ID) {
                        case "Count":
                            count++;
                            break;
                        case "Sum":
                            sum += v;
                            break;
                    }
                }
            }

            // Assign the resulting values to the bricks, 
            // which represent totals in the page footer.
            ((TextBrick)sbriks[0]).Text = String.Format("{0:0.##}", count);
            ((TextBrick)sbriks[1]).Text = String.Format("{0:0.##}", sum);
        }


        private void button1_Click(object sender, EventArgs e) {
            CreateDocument();
            ps.PageSettings.Assign(new Margins(15, 15, 15, 35),
                PaperKind.Custom, new Size(200, 500), true);
        }


        private void CreateDocument() {
            Cursor currentCursor = Cursor.Current;
            Cursor.Current = Cursors.WaitCursor;

            CreateTableOfValues();
            ps.PreviewFormEx.Show();

            Cursor.Current = currentCursor;
        }


        private void CreateTableOfValues() {
            BrickGraphics gr = ps.Graph;
            ps.Begin();
            double sum = 0;
            int count = 0;
            int h = 20;

            int X = Convert.ToInt32(Math.Floor(gr.ClientPageSize.Width / 5));
            gr.StringFormat = new BrickStringFormat(StringAlignment.Center, 
                StringAlignment.Center);

            // Display column headers.
            gr.Modifier = BrickModifier.DetailHeader;

            for (int i = 0; i < 2; i++) {
                gr.DrawString(Operation[i], Color.Black,
                    new RectangleF(i * X, 0, X, h), BorderSide.All);
            }

            // Display  random values.
            gr.Modifier = BrickModifier.Detail;
            gr.BackColor = SystemColors.Window;
            gr.Font = new Font("Arial", 8);

            double v;
            Random rand = new Random();
            for (int j = 0; j < 10; j++) {
                v = rand.NextDouble() * 100;
                for (int i = 0; i < 2; i++) {
                    TextBrick brick = gr.DrawString(String.Format("{0:0.##}", v), Color.Black,
                          new RectangleF(i * X, j * h, X, h), BorderSide.All);

                    // Assign the column identificator.
                    brick.ID = Operation[i];

                    // Assign the value used for subsequent calculations.
                    brick.TextValue = v.ToString();
                }

                count++;
                sum += v;
            }

            // Display the count and sum values in the ReportFooter section 
            // (at the end of the report).
            gr.Modifier = BrickModifier.ReportFooter;
            gr.BackColor = SystemColors.ControlDark;
            var font = new Font("Arial", 10, FontStyle.Bold);
            gr.Font = font;
            h = font.Height;
            gr.DrawString(String.Format("{0:0.##}", count), Color.White,
                new RectangleF(0, 0, X, h), BorderSide.All);
            gr.DrawString(String.Format("{0:0.##}", sum), Color.White,
                new RectangleF(X, 0, X, h), BorderSide.All);

            // Display summaries for the current page items. 
            gr.Modifier = BrickModifier.MarginalFooter;
            gr.StringFormat = gr.StringFormat.ChangeAlignment(StringAlignment.Far);
            for (int i = 0; i < 2; i++) {
                sbriks[i] = gr.DrawString("", Color.Black,
                    new RectangleF(i * X, 0, X, h), BorderSide.All);
            }

            ps.End();
        }

    }

}

