﻿using System.Diagnostics;

using Bloxstrap.Helpers;
using Bloxstrap.Helpers.RSMM;

namespace Bloxstrap.Dialogs.BootstrapperStyles
{
    // TODO - universal implementation for winforms-based styles? (to reduce duplicate code)

    public partial class ProgressDialog : Form
    {
        private readonly Bootstrapper? Bootstrapper;

        public ProgressDialog(Bootstrapper? bootstrapper = null)
        {
            InitializeComponent();

            Bootstrapper = bootstrapper;

            this.Text = Program.ProjectName;
            this.Icon = IconManager.GetIconResource();
            this.IconBox.BackgroundImage = IconManager.GetBitmapResource();

            if (Bootstrapper is null)
            {
                this.Message.Text = "Click the Cancel button to return to preferences";
                this.ButtonCancel.Enabled = true;
                this.ButtonCancel.Visible = true;
            }
            else
            {
                Bootstrapper.CloseDialogEvent += new EventHandler(CloseDialog);
                Bootstrapper.PromptShutdownEvent += new EventHandler(PromptShutdown);
                Bootstrapper.ShowSuccessEvent += new ChangeEventHandler<string>(ShowSuccess);
                Bootstrapper.MessageChanged += new ChangeEventHandler<string>(MessageChanged);
                Bootstrapper.ProgressBarValueChanged += new ChangeEventHandler<int>(ProgressBarValueChanged);
                Bootstrapper.ProgressBarStyleChanged += new ChangeEventHandler<ProgressBarStyle>(ProgressBarStyleChanged);
                Bootstrapper.CancelEnabledChanged += new ChangeEventHandler<bool>(CancelEnabledChanged);

                Task.Run(() => RunBootstrapper());
            }
        }

        public async void RunBootstrapper()
        {
            if (Bootstrapper is null)
                return;

            try
            {
                await Bootstrapper.Run();
            }
            catch (Exception ex)
            {
                // string message = String.Format("{0}: {1}", ex.GetType(), ex.Message);
                string message = ex.ToString();
                ShowError(message);

                Program.Exit();
            }
        }

        private void ShowError(string message)
        {
            MessageBox.Show(
                $"An error occurred while starting Roblox\n\nDetails: {message}", 
                Program.ProjectName, 
                MessageBoxButtons.OK, 
                MessageBoxIcon.Error
            );
        }

        private void ShowSuccess(object sender, ChangeEventArgs<string> e)
        {
            MessageBox.Show(
                e.Value,
                Program.ProjectName,
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );
        }

        private void CloseDialog(object? sender, EventArgs e)
        {
            this.Hide();
        }

        private void PromptShutdown(object? sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show(
                "Roblox is currently running, but needs to close. Would you like close Roblox now?",
                Program.ProjectName,
                MessageBoxButtons.OKCancel,
                MessageBoxIcon.Information
            );

            if (result != DialogResult.OK)
                Environment.Exit(0);
        }

        private void MessageChanged(object sender, ChangeEventArgs<string> e)
        {
            if (this.InvokeRequired)
            {
                ChangeEventHandler<string> handler = new(MessageChanged);
                this.Message.Invoke(handler, sender, e);
            }
            else
            {
                this.Message.Text = e.Value;
            }
        }

        private void ProgressBarValueChanged(object sender, ChangeEventArgs<int> e)
        {
            if (this.ProgressBar.InvokeRequired)
            {
                ChangeEventHandler<int> handler = new(ProgressBarValueChanged);
                this.ProgressBar.Invoke(handler, sender, e);
            }
            else
            {
                this.ProgressBar.Value = e.Value;
            }
        }

        private void ProgressBarStyleChanged(object sender, ChangeEventArgs<ProgressBarStyle> e)
        {
            if (this.ProgressBar.InvokeRequired)
            {
                ChangeEventHandler<ProgressBarStyle> handler = new(ProgressBarStyleChanged);
                this.ProgressBar.Invoke(handler, sender, e);
            }
            else
            {
                this.ProgressBar.Style = e.Value;
            }
        }

        private void CancelEnabledChanged(object sender, ChangeEventArgs<bool> e)
        {
            if (this.ButtonCancel.InvokeRequired)
            {
                ChangeEventHandler<bool> handler = new(CancelEnabledChanged);
                this.ButtonCancel.Invoke(handler, sender, e);
            }
            else
            {
                this.ButtonCancel.Enabled = e.Value;
                this.ButtonCancel.Visible = e.Value;
            }
        }

        private void ButtonCancel_Click(object sender, EventArgs e)
        {
            if (Bootstrapper is null)
                this.Close();
            else
                Task.Run(() => Bootstrapper.CancelButtonClicked());
        }

        private void ButtonCancel_MouseEnter(object sender, EventArgs e)
        {
            this.ButtonCancel.Image = Properties.Resources.CancelButtonHover;
        }

        private void ButtonCancel_MouseLeave(object sender, EventArgs e)
        {
            this.ButtonCancel.Image = Properties.Resources.CancelButton;
        }
    }
}