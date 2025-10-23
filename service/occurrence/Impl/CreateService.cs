using Microsoft.Xrm.Sdk;
using plugins_avaEdu.repository.occurrence.Interface;
using plugins_avaEdu.service.occurrence.Interface;
using plugins_avaEdu.utils;
using System;

namespace plugins_avaEdu.service.occurrence.Impl
{
    public class CreateService : ICreateService
    {
        private readonly IRepository _repo;

        public CreateService(IRepository repo)
        {
            _repo = repo;
        }

        public void Execute(IPluginExecutionContext context, IOrganizationService service)
        {
            var entity = GetTargetEntity(context);
            if (entity == null) return;

            SetCreationDate(entity);
            CalculateExpirationDate(entity, service);
            ValidateDuplicate(entity, service);
        }

        private void CalculateExpirationDate(Entity entity, IOrganizationService service)
        {
            var type = entity.GetAttributeValue<EntityReference>(LogicalNames.FIElDTIPOCORRENCIA);
            var deadlineHours = _repo.RetrieveResponseDeadlineHours(type, service) ?? LogicalNames.PRAZOPADRAOHORAS;
            var now = DateTime.UtcNow;

            entity[LogicalNames.FIElDDATAEXPIRACAO] = now.AddHours(deadlineHours);
        }

        private Entity GetTargetEntity(IPluginExecutionContext context)
        {
            if (context.InputParameters.Contains("Target") &&
                context.InputParameters["Target"] is Entity entity &&
                entity.LogicalName == LogicalNames.ENTITYLOGICALNAME)
            {
                return entity;
            }
            return null;
        }

        private void ValidateDuplicate(Entity entity, IOrganizationService service)
        {
            var cpf = entity.Contains(LogicalNames.FIElDCPF)
                ? entity[LogicalNames.FIElDCPF]?.ToString()
                : null;
            var type = entity.GetAttributeValue<EntityReference>(LogicalNames.FIELDTIPO);
            var subject = entity.GetAttributeValue<OptionSetValue>(LogicalNames.FIElDASSUNTOABERTO);

            if (!string.IsNullOrWhiteSpace(cpf) && type != null && subject != null)
            {
                if (_repo.ExistsOpenSameCpfTypeSubject(cpf, type, subject, service))
                {
                    throw new InvalidPluginExecutionException(
                        "Já existe uma ocorrência em aberto para este CPF, Tipo e Assunto.");
                }
            }
        }

        private void SetCreationDate(Entity entity)
        {
            if (!entity.Contains(LogicalNames.FIElDDATACRIACAO))
            {
                entity[LogicalNames.FIElDDATACRIACAO] = DateTime.UtcNow;
            }
        }
    }
}
