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
    public partial class Settings : Form
    {
        SettingsManager lol = new SettingsManager();
        public Settings()
        {
            InitializeComponent();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            App.TXTposlocation = textBox1.Text.ToString();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            lol.ChangeAnime(!SettingsManager.Anime);
            RefreshAnimeButton();
        }

        private void Settings_Load(object sender, EventArgs e)
        {
            lol.Instantiate();
            if (SettingsManager.Anime == false)
            {
                button1.BackColor = Color.Green;
            }
            else
            {
                button1.BackColor = Color.Red;
            }
        }

        private void RefreshAnimeButton()
        {
            if (SettingsManager.Anime == false)
                button1.BackColor = Color.Green;
            else
                button1.BackColor = Color.Red;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            App f = new App();
            f.Show();
            this.Hide();
        }
    }
}
