using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace КГ_2
{
    public partial class Form1 : Form
    {
        private Color brush = Color.RoyalBlue;
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e) {}

        private void pictureBox1_Click(object sender, EventArgs e) { }

        private void button2_Click(object sender, EventArgs e)
        {
            colorDialog1.ShowDialog();
            brush = colorDialog1.Color;
            button1_Click(sender, e);
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            button1_Click(sender, e);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Scene s = new Scene(pictureBox1.Height, pictureBox1.Width);
            s.brush = brush;
            s.addTrapezoid(Double.Parse(textBoxHeight.Text), Double.Parse(textBoxBase.Text), Int32.Parse(textBoxSides.Text));
            s.addCamera(new Camera(new Point3D(textBoxCamPos.Text), new Point3D(textBoxCamDir.Text).minus(new Point3D(textBoxCamPos.Text)), (double)numUpDownAngle.Value));
            s.lightPoint = new Point3D(textBox4.Text);
            s.Render();
            pictureBox1.Image = s.pic;
        }
    }
}
