using Microsoft.Xrm.Sdk;
using plugins_avaEdu.repository.occurrence.Impl;
using plugins_avaEdu.service.occurrence.Impl;
using plugins_avaEdu.service.occurrence.Interface;
using System;

namespace plugins_avaEdu.plugins.occurrence
{
    public class UpdatePlugin : PluginBase
    {
        private readonly IUpdateService _updateService;

        public UpdatePlugin()
            : base(typeof(UpdatePlugin))
        {
            _updateService = new UpdateService(new Repository());
        }

        protected override void ExecuteDataversePlugin(ILocalPluginContext localContext)
        {
            var serviceProvider = localContext.ServiceProvider;

            var context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            var tracing = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            var serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            var service = serviceFactory.CreateOrganizationService(context.UserId);

            try
            {
                _updateService.Execute(context, service);
            }
            catch (Exception e)
            {
                tracing.Trace("Erro UpdatePlugin: {0}", e.ToString());
                throw new InvalidPluginExecutionException(e.Message);
            }
        }
    }
}
