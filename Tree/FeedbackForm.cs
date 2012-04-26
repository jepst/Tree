using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Web;
using System.IO;

namespace Tree
{
    public partial class FeedbackForm : Form
    {
        public FeedbackForm()
        {
            InitializeComponent();
        }

        private string Submit(String s)
        {
            HttpWebRequest wreq = (HttpWebRequest)HttpWebRequest.Create("http://www.ductape.net/~eppie/feedback.cgi");
            wreq.Method = "POST";
            using (Stream wreqs = wreq.GetRequestStream())
            {
                using (StreamWriter sq = new StreamWriter(wreqs))
                {
                    sq.Write(string.Format("data={0}",s));
                }
            }
            HttpWebResponse wres = (HttpWebResponse)wreq.GetResponse();
            if (wres.StatusCode == System.Net.HttpStatusCode.OK)
            {
                using (Stream st = wres.GetResponseStream())
                {
                    string foo = new StreamReader(st).ReadToEnd();
                    return foo;
                }
            }
            else
                return null;
        }
        private void Error(string s)
        {
            MessageBox.Show(this, s, this.Text, MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }

        private void FeedbackForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (DialogResult == DialogResult.OK)
            {
                if (nameTextBox.Text.Length < 4)
                {
                    Error("Please enter your name before submitting your feedback.");
                    e.Cancel = true;
                    return;
                }
                if (emailTextBox.Text.Length < 4 || !emailTextBox.Text.Contains("@") || !emailTextBox.Text.Contains("."))
                {
                    Error("Please enter your email address before submitting your feedback.");
                    e.Cancel = true;
                    return;
                }
                
                string text = nameTextBox.Text + ":" + emailTextBox.Text + ":" + textBox.Text + ":" + AboutBox.AssemblyVersion+":"+Environment.OSVersion.ToString();
                string result = null;
                try
                {
                    result = Submit(text);
                }
                catch (Exception)
                {
                }
                if (result == null)
                {
                    Error("Sorry, we couldn't submit your feedback right now. Please make sure your Internet connection is working.\nOr, you can send your feedback via email to the address in the About box.");
                    e.Cancel = true;
                    return;
                }
                MessageBox.Show(this, result, this.Text, MessageBoxButtons.OK);
            }
        }
     }
}