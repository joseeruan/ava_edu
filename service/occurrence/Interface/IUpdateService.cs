using Microsoft.Xrm.Sdk;

namespace plugins_avaEdu.service.occurrence.Interface
{
    public interface IUpdateService
    {
        void Execute(IPluginExecutionContext context, IOrganizationService service);
    }
}
