using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static Test.Form2;

namespace Test
{
    public partial class UserDisplay : Form
    {
        MoodleUser CurrentUser { get; set; }
        public UserDisplay(MoodleUser user, Form2 HostForm)
        {
            InitializeComponent();
            CurrentUser = user;
            this.Text = user.Name;
            label1.Text = user.Name;
            pictureBox1.Image = HostForm.GetProfileImage(user);
        }
    }
}
