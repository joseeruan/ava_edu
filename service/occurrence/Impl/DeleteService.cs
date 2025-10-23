using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using plugins_avaEdu.repository.occurrence.Interface;
using plugins_avaEdu.service.occurrence.Interface;
using plugins_avaEdu.utils;

namespace plugins_avaEdu.service.occurrence.Impl
{
    public class DeleteService : IDeleteService
    {
        private readonly IRepository _repo;

        public DeleteService(IRepository repo)
        {
            _repo = repo;
        }

        public void Execute(IPluginExecutionContext context, IOrganizationService service)
        {
            var entityReference = GetTargetEntityReference(context);
            if (entityReference == null) return;

            var entity = _repo.Retrieve(entityReference.Id, service, new ColumnSet(LogicalNames.FIElDSTATUS));

            if (_repo.IsClosed(entity))
            {
                throw new InvalidPluginExecutionException("Ocorrência fechada não pode ser apagada.");
            }
        }

        private EntityReference GetTargetEntityReference(IPluginExecutionContext context)
        {
            if (context.InputParameters.Contains("Target") &&
                context.InputParameters["Target"] is EntityReference entityRef &&
                entityRef.LogicalName == LogicalNames.ENTITYLOGICALNAME)
            {
                return entityRef;
            }
            return null;
        }
    }
}
