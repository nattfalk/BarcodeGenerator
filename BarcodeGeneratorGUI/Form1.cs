using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BarcodeGenerator.GUI
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            BarcodeGenerator.Code128GS1.Encoder c128 = new BarcodeGenerator.Code128GS1.Encoder();
            BarcodeGenerator.Code128GS1.BarcodeImage barcodeImage = new BarcodeGenerator.Code128GS1.BarcodeImage();
            picBarcode.Image = barcodeImage.CreateImage(
                c128.Encode(txtInput.Text),
                1,
                true);

            // 098X1234567Y23
            // [Start B] 16 25 24 56 17 [Code C] 23 45 67 [Code B] 57 18 19 [checksum] [Stop]

        }
    }
}
