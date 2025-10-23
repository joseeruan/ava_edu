using Microsoft.Xrm.Sdk;
using plugins_avaEdu.repository.occurrence.Impl;
using plugins_avaEdu.service.occurrence.Impl;
using plugins_avaEdu.service.occurrence.Interface;
using System;

namespace plugins_avaEdu.plugins.occurrence
{
    public class CreatePlugin : PluginBase
    {
        private readonly ICreateService _createService;

        public CreatePlugin()
            : base(typeof(CreatePlugin))
        {
            _createService = new CreateService(new Repository());
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
                _createService.Execute(context, service);
            }
            catch (Exception e)
            {
                tracing.Trace("Erro CreatePlugin: {0}", e.ToString());
                throw new InvalidPluginExecutionException(e.Message);
            }
        }
    }
}
