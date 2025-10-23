using Microsoft.Xrm.Sdk;

namespace plugins_avaEdu.service.occurrence.Interface
{
    public interface ICreateService
    {
        void Execute(IPluginExecutionContext context, IOrganizationService service);
    }
}
