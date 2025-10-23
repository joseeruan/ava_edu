using System;
using Microsoft.Xrm.Sdk;
using NUnit.Framework;
using plugins_avaEdu.service.occurrence.Impl;
using plugins_avaEdu.utils;
using plugins_avaEdu.repository.occurrence.Impl;

namespace plugins_avaEdu.tests.AvaEdu.Tests
{
    [TestFixture]
    public class CreateServiceTests
    {
        private CrmTestContext _ctx;
        private CreateService _service;

        [SetUp]
        public void Setup()
        {
            _ctx = new CrmTestContext();
            _service = new CreateService(new Repository());
        }

        private Entity NewType(Guid id, int? deadlineHours = null)
        {
            var type = new Entity(LogicalNames.ENTITYLOGICALNAME) { Id = id };
            if (deadlineHours.HasValue)
            {
                type[LogicalNames.PRAZO] = deadlineHours.Value;
            }
            return type;
        }

        private Entity NewOccurrence(Guid? id = null, Guid? typeId = null, string cpf = null, int? subject = null)
        {
            var ent = new Entity(LogicalNames.ENTITYLOGICALNAME);
            if (id.HasValue) ent.Id = id.Value;
            if (typeId.HasValue)
            {
                ent[LogicalNames.FIElDTIPOCORRENCIA] = new EntityReference(LogicalNames.ENTITYLOGICALNAME, typeId.Value);
                ent[LogicalNames.FIELDTIPO] = new EntityReference(LogicalNames.ENTITYLOGICALNAME, typeId.Value);
            }
            if (!string.IsNullOrWhiteSpace(cpf)) ent[LogicalNames.FIElDCPF] = cpf;
            if (subject.HasValue) ent[LogicalNames.FIElDASSUNTOABERTO] = new OptionSetValue(subject.Value);
            return ent;
        }

        [Test]
        public void Should_Set_CreationDate_If_Not_Defined()
        {
            var typeId = Guid.NewGuid();
            _ctx.Seed(NewType(typeId));
            var occurrence = NewOccurrence(typeId: typeId, cpf: "123", subject: 10);
            var pluginCtx = _ctx.CreatePluginContext("Create", occurrence);

            _service.Execute(pluginCtx, _ctx.Service);

            Assert.IsTrue(occurrence.Contains(LogicalNames.FIElDDATACRIACAO));
            var creation = occurrence.GetAttributeValue<DateTime>(LogicalNames.FIElDDATACRIACAO);
            Assert.That((DateTime.UtcNow - creation).TotalMinutes, Is.LessThan(1));
        }

        [Test]
        public void Should_Not_Overwrite_CreationDate_If_Already_Defined()
        {
            var typeId = Guid.NewGuid();
            var originalCreation = DateTime.UtcNow.AddHours(-5);
            _ctx.Seed(NewType(typeId));
            var occurrence = NewOccurrence(typeId: typeId, cpf: "999", subject: 20);
            occurrence[LogicalNames.FIElDDATACRIACAO] = originalCreation;
            var pluginCtx = _ctx.CreatePluginContext("Create", occurrence);

            _service.Execute(pluginCtx, _ctx.Service);

            var after = occurrence.GetAttributeValue<DateTime>(LogicalNames.FIElDDATACRIACAO);
            Assert.AreEqual(originalCreation, after);
        }

        [Test]
        public void Should_Calculate_ExpirationDate_With_Type_Deadline()
        {
            var typeId = Guid.NewGuid();
            _ctx.Seed(NewType(typeId, deadlineHours: 5));
            var occurrence = NewOccurrence(typeId: typeId, cpf: "123", subject: 10);
            var pluginCtx = _ctx.CreatePluginContext("Create", occurrence);

            _service.Execute(pluginCtx, _ctx.Service);

            Assert.IsTrue(occurrence.Contains(LogicalNames.FIElDDATAEXPIRACAO));
            var creation = occurrence.GetAttributeValue<DateTime>(LogicalNames.FIElDDATACRIACAO);
            var exp = occurrence.GetAttributeValue<DateTime>(LogicalNames.FIElDDATAEXPIRACAO);
            Assert.That(Math.Round((exp - creation).TotalHours), Is.EqualTo(5));
        }

        [Test]
        public void Should_Calculate_ExpirationDate_With_Default_Deadline_If_Type_Has_No_Deadline()
        {
            var typeId = Guid.NewGuid();
            _ctx.Seed(NewType(typeId));
            var occurrence = NewOccurrence(typeId: typeId, cpf: "123", subject: 10);
            var pluginCtx = _ctx.CreatePluginContext("Create", occurrence);

            _service.Execute(pluginCtx, _ctx.Service);

            var creation = occurrence.GetAttributeValue<DateTime>(LogicalNames.FIElDDATACRIACAO);
            var exp = occurrence.GetAttributeValue<DateTime>(LogicalNames.FIElDDATAEXPIRACAO);
            Assert.That(Math.Round((exp - creation).TotalHours), Is.EqualTo(LogicalNames.PRAZOPADRAOHORAS));
        }

        [Test]
        public void Should_Validate_Duplicate()
        {
            var typeId = Guid.NewGuid();
            var subject = 10;
            var cpf = "123";
            var existing = NewOccurrence(Guid.NewGuid(), typeId, cpf, subject);
            existing[LogicalNames.FIElDSTATUS] = new OptionSetValue(LogicalNames.STATUSABERTO);
            _ctx.Seed(NewType(typeId), existing);

            var @new = NewOccurrence(typeId: typeId, cpf: cpf, subject: subject);
            var pluginCtx = _ctx.CreatePluginContext("Create", @new);

            Assert.Throws<InvalidPluginExecutionException>(() => _service.Execute(pluginCtx, _ctx.Service));
        }

        [Test]
        public void Should_Not_Validate_Duplicate_If_Existing_Occurrence_Is_Closed()
        {
            var typeId = Guid.NewGuid();
            var subject = 10;
            var cpf = "555";
            var existing = NewOccurrence(Guid.NewGuid(), typeId, cpf, subject);
            existing[LogicalNames.FIElDSTATUS] = new OptionSetValue(LogicalNames.STATUSFECHADO);
            _ctx.Seed(NewType(typeId), existing);

            var @new = NewOccurrence(typeId: typeId, cpf: cpf, subject: subject);
            var pluginCtx = _ctx.CreatePluginContext("Create", @new);

            Assert.DoesNotThrow(() => _service.Execute(pluginCtx, _ctx.Service));
        }
    }
}
