using FakeXrmEasy;
using Microsoft.Xrm.Sdk;
using NUnit.Framework;
using plugins_avaEdu.plugins.occurrence;
using plugins_avaEdu.Tests.Helpers;
using plugins_avaEdu.utils;
using System;

namespace plugins_avaEdu.Tests.Plugins
{
    [TestFixture]
    public class CreatePluginTests : FakeXrmEasyTestBase
    {
        private CreatePlugin _plugin;

        [SetUp]
        public override void Setup()
        {
            base.Setup();
            _plugin = new CreatePlugin();
        }

        [Test]
        public void Execute_Should_SetCreationDateAndExpirationDate_When_ValidOccurrenceCreated()
        {
            // Arrange
            var tipoId = Guid.NewGuid();
            var tipoEntity = TestDataFactory.CreateOccurrenceType(id: tipoId, prazoHoras: 48);
            var tipoRef = TestDataFactory.CreateOccurrenceTypeReference(tipoId);

            var occurrence = TestDataFactory.CreateOccurrence(
                tipo: tipoRef,
                assunto: TestDataFactory.CreateSubjectOptionSet(),
                dataCriacao: null);

            InitializeContext(tipoEntity);

            var pluginContext = Context.GetDefaultPluginContext();
            pluginContext.MessageName = "Create";
            pluginContext.Stage = 20; // PreOperation
            pluginContext.InputParameters["Target"] = occurrence;

            // Act
            Context.ExecutePluginWith<CreatePlugin>(pluginContext);

            // Assert
            Assert.That(occurrence.Contains(LogicalNames.FIElDDATACRIACAO), Is.True);
            Assert.That(occurrence.Contains(LogicalNames.FIElDDATAEXPIRACAO), Is.True);
        }

        [Test]
        public void Execute_Should_ThrowException_When_DuplicateOccurrenceExists()
        {
            // Arrange
            var cpf = "12345678901";
            var tipoId = Guid.NewGuid();
            var tipoRef = TestDataFactory.CreateOccurrenceTypeReference(tipoId);
            var assunto = TestDataFactory.CreateSubjectOptionSet(100000001);

            var tipoEntity = TestDataFactory.CreateOccurrenceType(id: tipoId);
            var existingOccurrence = TestDataFactory.CreateOpenOccurrence(cpf: cpf, tipo: tipoRef, assunto: assunto);
            InitializeContext(tipoEntity, existingOccurrence);

            var newOccurrence = TestDataFactory.CreateOccurrence(cpf: cpf, tipo: tipoRef, assunto: assunto);

            var pluginContext = Context.GetDefaultPluginContext();
            pluginContext.MessageName = "Create";
            pluginContext.Stage = 20;
            pluginContext.InputParameters["Target"] = newOccurrence;

            // Act & Assert
            var ex = Assert.Throws<InvalidPluginExecutionException>(() => 
                Context.ExecutePluginWith<CreatePlugin>(pluginContext));
            Assert.That(ex.Message, Does.Contain("Já existe uma ocorrência em aberto"));
        }

        [Test]
        public void Execute_Should_UseDefaultDeadline_When_TypeHasNoDeadline()
        {
            // Arrange
            var tipoId = Guid.NewGuid();
            var tipoEntity = new Entity(LogicalNames.FIELDTIPO) { Id = tipoId };
            // Not setting PRAZO field
            var tipoRef = TestDataFactory.CreateOccurrenceTypeReference(tipoId);

            var occurrence = TestDataFactory.CreateOccurrence(
                tipo: tipoRef,
                assunto: TestDataFactory.CreateSubjectOptionSet(),
                dataCriacao: null);

            InitializeContext(tipoEntity);

            var pluginContext = Context.GetDefaultPluginContext();
            pluginContext.MessageName = "Create";
            pluginContext.Stage = 20;
            pluginContext.InputParameters["Target"] = occurrence;

            // Act
            Context.ExecutePluginWith<CreatePlugin>(pluginContext);

            // Assert
            Assert.That(occurrence.Contains(LogicalNames.FIElDDATAEXPIRACAO), Is.True);
            var expiracao = occurrence.GetAttributeValue<DateTime>(LogicalNames.FIElDDATAEXPIRACAO);
            var expected = DateTime.UtcNow.AddHours(LogicalNames.PRAZOPADRAOHORAS);
            Assert.That(expiracao, Is.EqualTo(expected).Within(TimeSpan.FromMinutes(1)));
        }
    }
}
