using System;
using System.ServiceProcess;
using Roniz.Diagnostics.Logging;

namespace Roniz.Networking.Service.Host.WindowService
{
    public partial class ServiceHost : ServiceBase
    {
        #region members
        
        private ServiceResolver serviceResolver;

        #endregion

        #region constructores
        
        public ServiceHost()
        {
            InitializeComponent();
        }

        #endregion

        #region methods
        
        protected override void OnStart(string[] args)
        {
            try
            {
                serviceResolver = new ServiceResolver();
                serviceResolver.Open();
                LogManager.GetCurrentClassLogger().Debug("ServiceResolver is opened");
            }
            catch (Exception exception)
            {
                LogManager.GetCurrentClassLogger().Fatal(exception);
            }
        }

        protected override void OnStop()
        {
            try
            {
                if (serviceResolver != null)
                {
                    serviceResolver.Close();
                    LogManager.GetCurrentClassLogger().Debug("ServiceResolver is closed");
                }
            }
            catch (Exception exception)
            {
                LogManager.GetCurrentClassLogger().Fatal(exception);
            }
        }

        #endregion
    }
}
