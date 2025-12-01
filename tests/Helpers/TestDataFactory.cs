using Microsoft.Xrm.Sdk;
using plugins_avaEdu.utils;
using System;

namespace plugins_avaEdu.Tests.Helpers
{
    /// <summary>
    /// Factory class for creating test data entities.
    /// </summary>
    public static class TestDataFactory
    {
        /// <summary>
        /// Creates a test occurrence entity with default or specified values.
        /// </summary>
        public static Entity CreateOccurrence(
            Guid? id = null,
            string cpf = "12345678901",
            string nome = "Test User",
            string email = "test@example.com",
            string descricao = "Test Description",
            EntityReference tipo = null,
            OptionSetValue assunto = null,
            OptionSetValue status = null,
            DateTime? dataCriacao = null,
            DateTime? dataExpiracao = null,
            DateTime? dataConclusao = null)
        {
            var entity = new Entity(LogicalNames.ENTITYLOGICALNAME)
            {
                Id = id ?? Guid.NewGuid()
            };

            entity[LogicalNames.FIElDCPF] = cpf;
            entity[LogicalNames.FIElDNOME] = nome;
            entity[LogicalNames.FIElDEMAIL] = email;
            entity[LogicalNames.FIElDDESCRICAO] = descricao;
            
            if (tipo != null)
                entity[LogicalNames.FIELDTIPO] = tipo;
            
            if (assunto != null)
                entity[LogicalNames.FIElDASSUNTOABERTO] = assunto;
            
            if (status != null)
                entity[LogicalNames.FIElDSTATUS] = status;
            
            if (dataCriacao.HasValue)
                entity[LogicalNames.FIElDDATACRIACAO] = dataCriacao.Value;
            
            if (dataExpiracao.HasValue)
                entity[LogicalNames.FIElDDATAEXPIRACAO] = dataExpiracao.Value;
            
            if (dataConclusao.HasValue)
                entity[LogicalNames.FIElDDATACONCLUSAO] = dataConclusao.Value;

            return entity;
        }

        /// <summary>
        /// Creates a test occurrence type entity with response deadline.
        /// </summary>
        public static Entity CreateOccurrenceType(Guid? id = null, int prazoHoras = 48)
        {
            var entity = new Entity(LogicalNames.FIELDTIPO)
            {
                Id = id ?? Guid.NewGuid()
            };

            entity[LogicalNames.PRAZO] = prazoHoras;
            entity["name"] = $"Tipo Test {prazoHoras}h";

            return entity;
        }

        /// <summary>
        /// Creates an EntityReference for an occurrence type.
        /// </summary>
        public static EntityReference CreateOccurrenceTypeReference(Guid? id = null)
        {
            return new EntityReference(LogicalNames.FIELDTIPO, id ?? Guid.NewGuid());
        }

        /// <summary>
        /// Creates an OptionSetValue for occurrence status.
        /// </summary>
        public static OptionSetValue CreateStatusOptionSet(int statusValue)
        {
            return new OptionSetValue(statusValue);
        }

        /// <summary>
        /// Creates an OptionSetValue for occurrence subject.
        /// </summary>
        public static OptionSetValue CreateSubjectOptionSet(int subjectValue = 100000000)
        {
            return new OptionSetValue(subjectValue);
        }

        /// <summary>
        /// Creates an open occurrence (status = STATUSABERTO).
        /// </summary>
        public static Entity CreateOpenOccurrence(
            Guid? id = null,
            string cpf = "12345678901",
            EntityReference tipo = null,
            OptionSetValue assunto = null)
        {
            return CreateOccurrence(
                id: id,
                cpf: cpf,
                tipo: tipo ?? CreateOccurrenceTypeReference(),
                assunto: assunto ?? CreateSubjectOptionSet(),
                status: CreateStatusOptionSet(LogicalNames.STATUSABERTO),
                dataCriacao: DateTime.UtcNow.AddHours(-1),
                dataExpiracao: DateTime.UtcNow.AddHours(23));
        }

        /// <summary>
        /// Creates a closed occurrence (status = STATUSFECHADO).
        /// </summary>
        public static Entity CreateClosedOccurrence(
            Guid? id = null,
            string cpf = "12345678901",
            string nome = "Test User",
            EntityReference tipo = null,
            OptionSetValue assunto = null)
        {
            return CreateOccurrence(
                id: id,
                cpf: cpf,
                nome: nome,
                tipo: tipo ?? CreateOccurrenceTypeReference(),
                assunto: assunto ?? CreateSubjectOptionSet(),
                status: CreateStatusOptionSet(LogicalNames.STATUSFECHADO),
                dataCriacao: DateTime.UtcNow.AddDays(-2),
                dataExpiracao: DateTime.UtcNow.AddDays(-1),
                dataConclusao: DateTime.UtcNow.AddHours(-1));
        }
    }
}
