using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;

namespace plugins_avaEdu.repository.occurrence.Interface
{
    public interface IRepository
    {
        Entity Retrieve(Guid id, IOrganizationService svc, ColumnSet cols = null);
        void Update(Entity entity, IOrganizationService svc);
        Guid Create(Entity entity, IOrganizationService svc);
        bool ExistsOpenSameCpfTypeSubject(string cpf, EntityReference typeRef, OptionSetValue subjectOs, IOrganizationService svc, Guid? ignoreId = null);
        bool IsClosed(Entity entity);
        int? RetrieveResponseDeadlineHours(EntityReference typeRef, IOrganizationService svc);
    }
}
