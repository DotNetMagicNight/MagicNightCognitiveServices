using System;
using System.Drawing;
using System.Windows.Forms;

namespace MagicNightAzureApplication {

    public enum GenderEnum {
        Undefined,
        Male,
        Female
    }

    public enum EmotionEnum {
        Undefined,
        Anger,
        Contempt,
        Disgust,
        Fear,
        Happiness,
        Neutral,
        Sadness,
        Surprise
    } 

    public partial class Form1 : System.Windows.Forms.Form {
        private readonly MagicNight _magicNight;

        public Form1() {
            InitializeComponent();
            Application.UseWaitCursor = true;
            _magicNight = new MagicNight(new[] { this.pictureBox1, this.pictureBox2, this.pictureBox3 }, () => Application.UseWaitCursor = false);
        }

        private void SendChangeEvent(object sender, EventArgs e) {
            MagicNight.GenderEnum gender;
            MagicNight.EmotionEnum emotion;
            Enum.TryParse(this.GenderCombo.Text, out gender);
            Enum.TryParse(this.emotionCombo.Text, out emotion);
            _magicNight.ChangeFilters(gender, emotion);
        }

        private void ResizePictureBoxes(object sender = null, EventArgs e = null) {
            int width = (this.Width - 54) / 3;
            var hieght = this.Height - 108;
            var y = 57;
            pictureBox1.Width = pictureBox2.Width = pictureBox3.Width = width;
            pictureBox1.Height = pictureBox2.Height = pictureBox3.Height = hieght;
            pictureBox1.Location = new Point(12, y);
            pictureBox2.Location = new Point(12 + 7 + width, y);
            pictureBox3.Location = new Point(12 + 7 + 7 + width * 2, y);
        }
    }
}
