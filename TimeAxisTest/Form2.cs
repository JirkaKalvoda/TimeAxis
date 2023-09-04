using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TimeAxis;

namespace TimeAxisTest
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
            ScrollScaleBar scrollScaleBar = new ScrollScaleBar();
            scrollScaleBar.IsVertical = false;
            scrollScaleBar.Thickness = 14;
            scrollScaleBar.Length = 300;
            scrollScaleBar.Start = 1;
            scrollScaleBar.Stop = 4;
            scrollScaleBar.DisplayStart = 1;
            scrollScaleBar.DisplayStop = 3;
            this.Controls.Add(scrollScaleBar);
            scrollScaleBar.Dock = DockStyle.None;
            scrollScaleBar.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            //scrollScaleBar.Margin  = new Padding(50, 50, 50, 50);
            scrollScaleBar.Location = new Point(50, 50);
        }
    }
}
