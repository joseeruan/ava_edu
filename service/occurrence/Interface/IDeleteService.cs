using Microsoft.Xrm.Sdk;

namespace plugins_avaEdu.service.occurrence.Interface
{
    public interface IDeleteService
    {
        void Execute(IPluginExecutionContext context, IOrganizationService service);
    }
}
