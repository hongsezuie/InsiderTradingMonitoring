using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace Splendid
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            System.Threading.Tasks.Task.Factory.StartNew(() =>
            {
                try
                {
                    this.Invoke((Action)delegate
                    {
                        this.button1.Enabled = false;
                    });

                    Guardian TheGuardian = new Guardian(Directory.GetCurrentDirectory(), "Tickers.txt");
                    TheGuardian.DownloadInsiderTradingData();

                    this.Invoke((Action)delegate
                    {
                        MessageBox.Show("Complete.");
                        this.button1.Enabled = true;
                    });
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
                finally
                {
                    this.Invoke((Action)delegate
                    {
                        this.button1.Enabled = true;
                    });
                }
            });
        }
    }
}
