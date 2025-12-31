using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace APK_editor_Sigma
{
    public partial class Manifest : Form
    {
        public Manifest()
        {
            InitializeComponent();
        }

        private void Manifest_Load(object sender, EventArgs e)
        {
            richTextBox1.Text = App.manifest;
            richTextBox1.ReadOnly = true;
            richTextBox1.WordWrap = false;
            richTextBox1.ScrollBars = RichTextBoxScrollBars.Both;
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void vScrollBar1_Scroll(object sender, ScrollEventArgs e)
        {

        }
    }
}
