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
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            this.StartPosition = FormStartPosition.CenterScreen;
            TimeAxisMain timeAxisMain = new TimeAxisMain();
            this.Controls.Add(timeAxisMain);
            timeAxisMain.Dock = DockStyle.Fill;
            timeAxisMain.Ruler.Start = DateTime.Today.AddSeconds(2.2);
            Track t1 = new Track()
            {
                Height = 30,
            };
            t1.Segments.Add(new Segment()
            {
                DefaultColor = Color.Magenta,
                Start = DateTime.Today.AddHours(2),
                Stop = DateTime.Today.AddHours(4),
            });
            t1.Segments.Add(new Segment()
            {
                DefaultColor = Color.GreenYellow,
                Start = DateTime.Today.AddHours(7),
                Stop = DateTime.Today.AddHours(15),
            });
            Track t2 = new Track();
            t2.Segments.Add(new Segment()
            {
                Start = DateTime.Today.AddHours(3),
                Stop = DateTime.Today.AddHours(6),
            });
            Track t3 = new Track();
            t3.Segments.Add(new Segment()
            {
                Start = DateTime.Today.AddHours(4),
                Stop = DateTime.Today.AddHours(7),
            });
            Track t4 = new Track();
            t4.Segments.Add(new Segment()
            {
                Start = DateTime.Today.AddHours(5),
                Stop = DateTime.Today.AddHours(24),
            });
            Track t5 = new Track();
            t5.Segments.Add(new Segment()
            {
                Start = DateTime.Today.AddHours(6),
                Stop = DateTime.Today.AddHours(9),
            });
            timeAxisMain.Tracks.Add(t1);
            timeAxisMain.Tracks.Add(t2);
            timeAxisMain.Tracks.Add(t3);
            timeAxisMain.Tracks.Add(t4);
            timeAxisMain.Tracks.Add(t5);
        }
    }
}
