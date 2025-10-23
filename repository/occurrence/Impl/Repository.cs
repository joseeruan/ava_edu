using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using plugins_avaEdu.repository.occurrence.Interface;
using plugins_avaEdu.utils;
using System;

namespace plugins_avaEdu.repository.occurrence.Impl
{
    public class Repository : IRepository
    {
        public Guid Create(Entity entity, IOrganizationService svc)
        {
            return svc.Create(entity);
        }

        public Entity Retrieve(Guid id, IOrganizationService svc, ColumnSet cols = null)
        {
            return svc.Retrieve(
                LogicalNames.ENTITYLOGICALNAME,
                id,
                cols ?? new ColumnSet(true));
        }

        public void Update(Entity entity, IOrganizationService svc)
        {
            svc.Update(entity);
        }

        public bool ExistsOpenSameCpfTypeSubject(
            string cpf,
            EntityReference typeRef,
            OptionSetValue subjectOs,
            IOrganizationService svc,
            Guid? ignoreId = null)
        {
            if (string.IsNullOrWhiteSpace(cpf) || typeRef == null || subjectOs == null)
                return false;

            var query = BuildDuplicateQuery(cpf, typeRef, subjectOs, ignoreId);
            var result = svc.RetrieveMultiple(query);

            return result.Entities.Count > 0;
        }

        public bool IsClosed(Entity entity)
        {
            if (entity == null || !entity.Contains(LogicalNames.FIElDSTATUS))
                return false;

            var status = entity.GetAttributeValue<OptionSetValue>(LogicalNames.FIElDSTATUS);
            return status != null && status.Value == LogicalNames.STATUSFECHADO;
        }

        public int? RetrieveResponseDeadlineHours(EntityReference typeRef, IOrganizationService svc)
        {
            if (typeRef == null)
                return null;

            var type = svc.Retrieve(
                LogicalNames.FIELDTIPO,
                typeRef.Id,
                new ColumnSet(LogicalNames.PRAZO));

            if (type.Contains(LogicalNames.PRAZO))
            {
                return type.GetAttributeValue<int?>(LogicalNames.PRAZO);
            }

            return null;
        }

        private QueryExpression BuildDuplicateQuery(
            string cpf,
            EntityReference typeRef,
            OptionSetValue subjectOs,
            Guid? ignoreId)
        {
            var query = new QueryExpression(LogicalNames.ENTITYLOGICALNAME)
            {
                ColumnSet = new ColumnSet(false),
                TopCount = 1
            };

            query.Criteria.AddCondition(LogicalNames.FIElDCPF, ConditionOperator.Equal, cpf);
            query.Criteria.AddCondition(LogicalNames.FIELDTIPO, ConditionOperator.Equal, typeRef.Id);
            query.Criteria.AddCondition(LogicalNames.FIElDASSUNTOABERTO, ConditionOperator.Equal, subjectOs.Value);
            query.Criteria.AddCondition(LogicalNames.FIElDSTATUS, ConditionOperator.Equal, LogicalNames.STATUSABERTO);

            if (ignoreId.HasValue)
            {
                query.Criteria.AddCondition(LogicalNames.EntityPRIMARYID, ConditionOperator.NotEqual, ignoreId.Value);
            }

            return query;
        }
    }
}
