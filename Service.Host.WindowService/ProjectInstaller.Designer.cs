namespace Roniz.Networking.Service.Host.WindowService
{
    partial class ProjectInstaller
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ProjectInstaller));
            this.serviceInstaller = new System.ServiceProcess.ServiceInstaller();
            // 
            // serviceInstaller
            // 
            this.serviceInstaller.DelayedAutoStart = true;
            this.serviceInstaller.Description = resources.GetString("serviceInstaller.Description");
            this.serviceInstaller.DisplayName = "Networking Service";
            this.serviceInstaller.ServiceName = "Roniz.Networking.Service";
            this.serviceInstaller.ServicesDependedOn = new string[] {
        "PNRPsvc",
        "nsi",
        "netprofm"};
            this.serviceInstaller.StartType = System.ServiceProcess.ServiceStartMode.Automatic;
            // 
            // ProjectInstaller
            // 
            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.serviceInstaller});

        }

        #endregion

        private System.ServiceProcess.ServiceInstaller serviceInstaller;
    }
}