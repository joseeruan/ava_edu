using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using plugins_avaEdu.repository.occurrence.Interface;
using plugins_avaEdu.service.occurrence.Interface;
using plugins_avaEdu.utils;
using System;

namespace plugins_avaEdu.service.occurrence.Impl
{
    public class UpdateService : IUpdateService
    {
        private readonly IRepository _repo;

        public UpdateService(IRepository repo)
        {
            _repo = repo;
        }

        public void Execute(IPluginExecutionContext context, IOrganizationService service)
        {
            var target = GetTargetEntity(context);
            if (target == null) return;

            var preImage = GetPreImage(context, target, service);
            if (preImage == null) return;

            var previousStatus = preImage.GetAttributeValue<OptionSetValue>(LogicalNames.FIElDSTATUS)?.Value;

            if (previousStatus == LogicalNames.STATUSFECHADO)
            {
                ValidateModificationInClosedOccurrence(target, preImage);
            }

            UpdateConclusionDate(target, preImage);
            RecalculateExpirationDate(target, preImage, service);
            ValidateDuplicateOnUpdate(target, preImage, service);
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

        private Entity GetPreImage(IPluginExecutionContext context, Entity target, IOrganizationService service)
        {
            Entity preImage = null;

            if (context.PreEntityImages != null && context.PreEntityImages.Contains("PreImage"))
            {
                preImage = context.PreEntityImages["PreImage"];
            }

            if (preImage == null && target.Id != Guid.Empty)
            {
                preImage = _repo.Retrieve(target.Id, service, new ColumnSet(
                    LogicalNames.FIElDSTATUS,
                    LogicalNames.FIElDDATACONCLUSAO,
                    LogicalNames.FIElDDATACRIACAO,
                    LogicalNames.FIELDTIPO,
                    LogicalNames.FIElDCPF,
                    LogicalNames.FIElDNOME,
                    LogicalNames.FIElDEMAIL,
                    LogicalNames.FIElDDESCRICAO,
                    LogicalNames.FIElDASSUNTOABERTO));
            }

            return preImage;
        }

        private void ValidateModificationInClosedOccurrence(Entity target, Entity preImage)
        {
            ValidateUnchangedTextField(target, preImage, LogicalNames.FIElDNOME, "Nome");
            ValidateUnchangedTextField(target, preImage, LogicalNames.FIElDEMAIL, "Email");
            ValidateUnchangedTextField(target, preImage, LogicalNames.FIElDDESCRICAO, "Descrição");
            ValidateUnchangedTextField(target, preImage, LogicalNames.FIElDCPF, "CPF");
            ValidateUnchangedTextField(target, preImage, LogicalNames.FIELDCEP, "CEP");
            ValidateUnchangedReferenceOrOptionSet(target, preImage);

            throw new InvalidPluginExecutionException("Ocorrência fechada não pode ser editada.");
        }

        private void ValidateUnchangedTextField(Entity target, Entity preImage, string fieldName, string fieldLabel)
        {
            if (target.Contains(fieldName))
            {
                var originalValue = preImage.GetAttributeValue<string>(fieldName) ?? string.Empty;
                var newValue = target.GetAttributeValue<string>(fieldName) ?? string.Empty;

                if (!string.Equals(originalValue, newValue, StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidPluginExecutionException(
                        $"Ocorrência fechada não pode ter o {fieldLabel} alterado.");
                }
            }
        }

        private void ValidateUnchangedReferenceOrOptionSet(Entity target, Entity preImage)
        {
            if (target.Contains(LogicalNames.FIELDTIPO))
            {
                var originalType = preImage.GetAttributeValue<EntityReference>(LogicalNames.FIELDTIPO);
                var newType = target.GetAttributeValue<EntityReference>(LogicalNames.FIELDTIPO);

                if (originalType?.Id != newType?.Id)
                {
                    throw new InvalidPluginExecutionException("Ocorrência fechada não pode ter o Tipo alterado.");
                }
            }

            if (target.Contains(LogicalNames.FIElDASSUNTOABERTO))
            {
                var originalSubject = preImage.GetAttributeValue<OptionSetValue>(LogicalNames.FIElDASSUNTOABERTO);
                var newSubject = target.GetAttributeValue<OptionSetValue>(LogicalNames.FIElDASSUNTOABERTO);

                if (originalSubject?.Value != newSubject?.Value)
                {
                    throw new InvalidPluginExecutionException("Ocorrência fechada não pode ter o Assunto alterado.");
                }
            }
        }

        private void UpdateConclusionDate(Entity target, Entity preImage)
        {
            if (target.Contains(LogicalNames.FIElDSTATUS))
            {
                var newStatus = target.GetAttributeValue<OptionSetValue>(LogicalNames.FIElDSTATUS)?.Value;
                var hasConclusionDate = preImage.Contains(LogicalNames.FIElDDATACONCLUSAO);

                if (newStatus == LogicalNames.STATUSFECHADO && !hasConclusionDate)
                {
                    target[LogicalNames.FIElDDATACONCLUSAO] = DateTime.UtcNow;
                }
            }
        }

        private void RecalculateExpirationDate(Entity target, Entity preImage, IOrganizationService service)
        {
            if (target.Contains(LogicalNames.FIELDTIPO))
            {
                var newType = target.GetAttributeValue<EntityReference>(LogicalNames.FIELDTIPO)
                    ?? preImage.GetAttributeValue<EntityReference>(LogicalNames.FIELDTIPO);

                var deadlineHours = _repo.RetrieveResponseDeadlineHours(newType, service)
                    ?? LogicalNames.PRAZOPADRAOHORAS;

                var creationDate = preImage.GetAttributeValue<DateTime?>(LogicalNames.FIElDDATACRIACAO)
                    ?? DateTime.UtcNow;

                target[LogicalNames.FIElDDATAEXPIRACAO] = creationDate.AddHours(deadlineHours);
            }
        }

        private void ValidateDuplicateOnUpdate(Entity target, Entity preImage, IOrganizationService service)
        {
            var cpf = target.Contains(LogicalNames.FIElDCPF)
                ? target[LogicalNames.FIElDCPF]?.ToString()
                : preImage.GetAttributeValue<string>(LogicalNames.FIElDCPF);

            var type = target.GetAttributeValue<EntityReference>(LogicalNames.FIELDTIPO)
                ?? preImage.GetAttributeValue<EntityReference>(LogicalNames.FIELDTIPO);

            var subject = target.GetAttributeValue<OptionSetValue>(LogicalNames.FIElDASSUNTOABERTO)
                ?? preImage.GetAttributeValue<OptionSetValue>(LogicalNames.FIElDASSUNTOABERTO);

            if (!string.IsNullOrWhiteSpace(cpf) && type != null && subject != null)
            {
                if (_repo.ExistsOpenSameCpfTypeSubject(cpf, type, subject, service, preImage.Id))
                {
                    throw new InvalidPluginExecutionException(
                        "Já existe outra ocorrência em aberto para este CPF, Tipo e Assunto.");
                }
            }
        }
    }
}
