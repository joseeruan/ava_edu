using System;
using Microsoft.Xrm.Sdk;
using NUnit.Framework;
using plugins_avaEdu.service.occurrence.Impl;
using plugins_avaEdu.utils;
using plugins_avaEdu.repository.occurrence.Impl;

namespace plugins_avaEdu.tests.AvaEdu.Tests
{
    [TestFixture]
    public class UpdateServiceTests
    {
        private CrmTestContext _ctx;
        private UpdateService _service;

        [SetUp]
        public void Setup()
        {
            _ctx = new CrmTestContext();
            _service = new UpdateService(new Repository());
        }

        private Entity NewOccurrence(Guid id, Guid typeId, string cpf = "123", int subject = 10, int status = LogicalNames.STATUSABERTO)
        {
            var ent = new Entity(LogicalNames.ENTITYLOGICALNAME) { Id = id };
            ent[LogicalNames.FIELDTIPO] = new EntityReference(LogicalNames.ENTITYLOGICALNAME, typeId);
            ent[LogicalNames.FIElDTIPOCORRENCIA] = new EntityReference(LogicalNames.ENTITYLOGICALNAME, typeId);
            ent[LogicalNames.FIElDCPF] = cpf;
            ent[LogicalNames.FIElDASSUNTOABERTO] = new OptionSetValue(subject);
            ent[LogicalNames.FIElDDATACRIACAO] = DateTime.UtcNow.AddHours(-1);
            ent[LogicalNames.FIElDSTATUS] = new OptionSetValue(status);
            ent[LogicalNames.FIElDNOME] = "Nome Original";
            ent[LogicalNames.FIElDEMAIL] = "email@orig.com";
            ent[LogicalNames.FIElDDESCRICAO] = "Desc Original";
            return ent;
        }

        [Test]
        public void Should_Set_ConclusionDate_When_Closing()
        {
            var type = Guid.NewGuid();
            var id = Guid.NewGuid();
            var existing = NewOccurrence(id, type);
            _ctx.Seed(existing);
            var target = new Entity(LogicalNames.ENTITYLOGICALNAME) { Id = id };
            target[LogicalNames.FIElDSTATUS] = new OptionSetValue(LogicalNames.STATUSFECHADO);

            var pluginCtx = _ctx.CreatePluginContext("Update", target, id);
            pluginCtx.PreEntityImages.Add("PreImage", existing);

            _service.Execute(pluginCtx, _ctx.Service);

            Assert.IsTrue(target.Contains(LogicalNames.FIElDDATACONCLUSAO));
        }

        [Test]
        public void OnUpdate_Fechada_TentandoAlterarCep_DeveLancarExcecao()
        {
            var id = Guid.NewGuid();
            var preImage = new Entity(LogicalNames.ENTITYLOGICALNAME) { Id = id };
            preImage[LogicalNames.FIElDSTATUS] = new OptionSetValue(LogicalNames.STATUSFECHADO);
            preImage[LogicalNames.FIELDCEP] = "52000000";

            var target = new Entity(LogicalNames.ENTITYLOGICALNAME) { Id = id };
            target[LogicalNames.FIELDCEP] = "52000001"; 

            var pluginCtx = _ctx.CreatePluginContext("Update", target, id);
            pluginCtx.PreEntityImages.Add("PreImage", preImage);

            var ex = Assert.Throws<InvalidPluginExecutionException>(() => _service.Execute(pluginCtx, _ctx.Service));
            Assert.That(ex.Message, Does.Contain("CEP"));
        }

        [Test]
        public void Should_Not_Set_ConclusionDate_If_Already_Set()
        {
            var type = Guid.NewGuid();
            var id = Guid.NewGuid();
            var existing = NewOccurrence(id, type);
            existing[LogicalNames.FIElDDATACONCLUSAO] = DateTime.UtcNow.AddMinutes(-5);
            _ctx.Seed(existing);
            var target = new Entity(LogicalNames.ENTITYLOGICALNAME) { Id = id };
            target[LogicalNames.FIElDSTATUS] = new OptionSetValue(LogicalNames.STATUSFECHADO);

            var pluginCtx = _ctx.CreatePluginContext("Update", target, id);
            pluginCtx.PreEntityImages.Add("PreImage", existing);

            _service.Execute(pluginCtx, _ctx.Service);

            Assert.IsFalse(target.Contains(LogicalNames.FIElDDATACONCLUSAO));
        }

        [Test]
        public void Should_Recalculate_ExpirationDate_When_Type_Changes()
        {
            var type1 = Guid.NewGuid();
            var type2 = Guid.NewGuid();
            var existing = NewOccurrence(Guid.NewGuid(), type1);

            var typeEntity1 = new Entity(LogicalNames.ENTITYLOGICALNAME) { Id = type1 };
            typeEntity1[LogicalNames.PRAZO] = 2;
            var typeEntity2 = new Entity(LogicalNames.ENTITYLOGICALNAME) { Id = type2 };
            typeEntity2[LogicalNames.PRAZO] = 5;

            _ctx.Seed(existing, typeEntity1, typeEntity2);

            var target = new Entity(LogicalNames.ENTITYLOGICALNAME) { Id = existing.Id };
            target[LogicalNames.FIELDTIPO] = new EntityReference(LogicalNames.ENTITYLOGICALNAME, type2);

            var pluginCtx = _ctx.CreatePluginContext("Update", target, existing.Id);
            pluginCtx.PreEntityImages.Add("PreImage", existing);

            _service.Execute(pluginCtx, _ctx.Service);

            Assert.IsTrue(target.Contains(LogicalNames.FIElDDATAEXPIRACAO));
            var creation = existing.GetAttributeValue<DateTime>(LogicalNames.FIElDDATACRIACAO);
            var exp = target.GetAttributeValue<DateTime>(LogicalNames.FIElDDATAEXPIRACAO);
            Assert.That(Math.Round((exp - creation).TotalHours), Is.EqualTo(5));
        }

        [Test]
        public void Should_Validate_Duplicate_On_Update()
        {
            var type = Guid.NewGuid();
            var subject = 10;
            var cpf = "123";
            var existing1 = NewOccurrence(Guid.NewGuid(), type, cpf, subject);
            var existing2 = NewOccurrence(Guid.NewGuid(), type, cpf, subject);
            _ctx.Seed(existing1, existing2);
            var target = new Entity(LogicalNames.ENTITYLOGICALNAME) { Id = existing2.Id };
            target[LogicalNames.FIElDCPF] = cpf;

            var pluginCtx = _ctx.CreatePluginContext("Update", target, existing2.Id);
            pluginCtx.PreEntityImages.Add("PreImage", existing2);

            Assert.Throws<InvalidPluginExecutionException>(() => _service.Execute(pluginCtx, _ctx.Service));
        }

        [Test]
        public void Closed_Occurrence_Changing_Name_Should_Throw_Exception()
        {
            var type = Guid.NewGuid();
            var id = Guid.NewGuid();
            var existing = NewOccurrence(id, type, status: LogicalNames.STATUSFECHADO);
            _ctx.Seed(existing);
            var target = new Entity(LogicalNames.ENTITYLOGICALNAME) { Id = id };
            target[LogicalNames.FIElDNOME] = "Nome Alterado";

            var pluginCtx = _ctx.CreatePluginContext("Update", target, id);
            pluginCtx.PreEntityImages.Add("PreImage", existing);

            var ex = Assert.Throws<InvalidPluginExecutionException>(() => _service.Execute(pluginCtx, _ctx.Service));
            Assert.That(ex.Message, Does.Contain("Nome"));
        }

        [Test]
        public void Closed_Occurrence_Changing_Email_Should_Throw_Exception()
        {
            var type = Guid.NewGuid();
            var id = Guid.NewGuid();
            var existing = NewOccurrence(id, type, status: LogicalNames.STATUSFECHADO);
            _ctx.Seed(existing);
            var target = new Entity(LogicalNames.ENTITYLOGICALNAME) { Id = id };
            target[LogicalNames.FIElDEMAIL] = "novo@email.com";

            var pluginCtx = _ctx.CreatePluginContext("Update", target, id);
            pluginCtx.PreEntityImages.Add("PreImage", existing);

            var ex = Assert.Throws<InvalidPluginExecutionException>(() => _service.Execute(pluginCtx, _ctx.Service));
            Assert.That(ex.Message, Does.Contain("Email"));
        }

        [Test]
        public void Closed_Occurrence_Changing_Description_Should_Throw_Exception()
        {
            var type = Guid.NewGuid();
            var id = Guid.NewGuid();
            var existing = NewOccurrence(id, type, status: LogicalNames.STATUSFECHADO);
            _ctx.Seed(existing);
            var target = new Entity(LogicalNames.ENTITYLOGICALNAME) { Id = id };
            target[LogicalNames.FIElDDESCRICAO] = "Desc Alterada";

            var pluginCtx = _ctx.CreatePluginContext("Update", target, id);
            pluginCtx.PreEntityImages.Add("PreImage", existing);

            var ex = Assert.Throws<InvalidPluginExecutionException>(() => _service.Execute(pluginCtx, _ctx.Service));
            Assert.That(ex.Message, Does.Contain("Descrição"));
        }

        [Test]
        public void Closed_Occurrence_Changing_CPF_Should_Throw_Exception()
        {
            var type = Guid.NewGuid();
            var id = Guid.NewGuid();
            var existing = NewOccurrence(id, type, cpf: "123", status: LogicalNames.STATUSFECHADO);
            _ctx.Seed(existing);
            var target = new Entity(LogicalNames.ENTITYLOGICALNAME) { Id = id };
            target[LogicalNames.FIElDCPF] = "999";

            var pluginCtx = _ctx.CreatePluginContext("Update", target, id);
            pluginCtx.PreEntityImages.Add("PreImage", existing);

            var ex = Assert.Throws<InvalidPluginExecutionException>(() => _service.Execute(pluginCtx, _ctx.Service));
            Assert.That(ex.Message, Does.Contain("CPF"));
        }

        [Test]
        public void Closed_Occurrence_Changing_Type_Should_Throw_Exception()
        {
            var type = Guid.NewGuid();
            var newType = Guid.NewGuid();
            var id = Guid.NewGuid();
            var existing = NewOccurrence(id, type, status: LogicalNames.STATUSFECHADO);
            _ctx.Seed(existing);
            var target = new Entity(LogicalNames.ENTITYLOGICALNAME) { Id = id };
            target[LogicalNames.FIELDTIPO] = new EntityReference(LogicalNames.ENTITYLOGICALNAME, newType);

            var pluginCtx = _ctx.CreatePluginContext("Update", target, id);
            pluginCtx.PreEntityImages.Add("PreImage", existing);

            var ex = Assert.Throws<InvalidPluginExecutionException>(() => _service.Execute(pluginCtx, _ctx.Service));
            Assert.That(ex.Message, Does.Contain("Tipo"));
        }

        [Test]
        public void Closed_Occurrence_Changing_Subject_Should_Throw_Exception()
        {
            var type = Guid.NewGuid();
            var id = Guid.NewGuid();
            var existing = NewOccurrence(id, type, subject: 10, status: LogicalNames.STATUSFECHADO);
            _ctx.Seed(existing);
            var target = new Entity(LogicalNames.ENTITYLOGICALNAME) { Id = id };
            target[LogicalNames.FIElDASSUNTOABERTO] = new OptionSetValue(11);

            var pluginCtx = _ctx.CreatePluginContext("Update", target, id);
            pluginCtx.PreEntityImages.Add("PreImage", existing);

            var ex = Assert.Throws<InvalidPluginExecutionException>(() => _service.Execute(pluginCtx, _ctx.Service));
            Assert.That(ex.Message, Does.Contain("Assunto"));
        }

        [Test]
        public void Closed_Occurrence_No_Changes_Should_Throw_Exception()
        {
            var type = Guid.NewGuid();
            var id = Guid.NewGuid();
            var existing = NewOccurrence(id, type, status: LogicalNames.STATUSFECHADO);
            _ctx.Seed(existing);
            var target = new Entity(LogicalNames.ENTITYLOGICALNAME) { Id = id };

            var pluginCtx = _ctx.CreatePluginContext("Update", target, id);
            pluginCtx.PreEntityImages.Add("PreImage", existing);

            Assert.Throws<InvalidPluginExecutionException>(() => _service.Execute(pluginCtx, _ctx.Service));
        }
    }
}
