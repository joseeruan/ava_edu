using Microsoft.Xrm.Sdk;
using Moq;
using NUnit.Framework;
using plugins_avaEdu.repository.occurrence.Interface;
using plugins_avaEdu.service.occurrence.Impl;
using plugins_avaEdu.Tests.Helpers;
using plugins_avaEdu.utils;
using System;

namespace plugins_avaEdu.Tests.Services
{
    [TestFixture]
    public class UpdateServiceTests : FakeXrmEasyTestBase
    {
        private UpdateService _service;
        private Mock<IRepository> _mockRepository;

        [SetUp]
        public override void Setup()
        {
            base.Setup();
            _mockRepository = new Mock<IRepository>();
            _service = new UpdateService(_mockRepository.Object);
        }

        #region Execute Tests - Conclusion Date

        [Test]
        public void Execute_Should_SetConclusionDate_When_StatusChangedToFechado()
        {
            // Arrange
            var occurrenceId = Guid.NewGuid();
            var target = new Entity(LogicalNames.ENTITYLOGICALNAME) { Id = occurrenceId };
            target[LogicalNames.FIElDSTATUS] = TestDataFactory.CreateStatusOptionSet(LogicalNames.STATUSFECHADO);

            var preImage = TestDataFactory.CreateOpenOccurrence(id: occurrenceId);
            
            var context = Context.GetDefaultPluginContext();
            context.InputParameters["Target"] = target;
            context.PreEntityImages["PreImage"] = preImage;

            _mockRepository.Setup(r => r.ExistsOpenSameCpfTypeSubject(
                It.IsAny<string>(), 
                It.IsAny<EntityReference>(), 
                It.IsAny<OptionSetValue>(), 
                It.IsAny<IOrganizationService>(), 
                It.IsAny<Guid>()))
                .Returns(false);

            // Act
            _service.Execute(context, Service);

            // Assert
            Assert.That(target.Contains(LogicalNames.FIElDDATACONCLUSAO), Is.True);
            var conclusionDate = target.GetAttributeValue<DateTime>(LogicalNames.FIElDDATACONCLUSAO);
            Assert.That(conclusionDate, Is.EqualTo(DateTime.UtcNow).Within(TimeSpan.FromSeconds(5)));
        }

        [Test]
        public void Execute_Should_ThrowException_When_ClosedOccurrenceNameChanged()
        {
            // Arrange
            var occurrenceId = Guid.NewGuid();
            var target = new Entity(LogicalNames.ENTITYLOGICALNAME) { Id = occurrenceId };
            target[LogicalNames.FIElDNOME] = "New Name";

            var preImage = TestDataFactory.CreateClosedOccurrence(id: occurrenceId, nome: "Original Name");
            
            var context = Context.GetDefaultPluginContext();
            context.InputParameters["Target"] = target;
            context.PreEntityImages["PreImage"] = preImage;

            // Act & Assert
            var ex = Assert.Throws<InvalidPluginExecutionException>(() => _service.Execute(context, Service));
            Assert.That(ex.Message, Does.Contain("fechada não pode"));
        }

        [Test]
        public void Execute_Should_RecalculateExpirationDate_When_TypeChanged()
        {
            // Arrange
            var occurrenceId = Guid.NewGuid();
            var newTipo = TestDataFactory.CreateOccurrenceTypeReference();
            var target = new Entity(LogicalNames.ENTITYLOGICALNAME) { Id = occurrenceId };
            target[LogicalNames.FIELDTIPO] = newTipo;

            var preImage = TestDataFactory.CreateOpenOccurrence(id: occurrenceId);
            
            var context = Context.GetDefaultPluginContext();
            context.InputParameters["Target"] = target;
            context.PreEntityImages["PreImage"] = preImage;

            _mockRepository.Setup(r => r.RetrieveResponseDeadlineHours(newTipo, Service))
                .Returns(72);

            _mockRepository.Setup(r => r.ExistsOpenSameCpfTypeSubject(
                It.IsAny<string>(), 
                It.IsAny<EntityReference>(), 
                It.IsAny<OptionSetValue>(), 
                It.IsAny<IOrganizationService>(), 
                It.IsAny<Guid>()))
                .Returns(false);

            // Act
            _service.Execute(context, Service);

            // Assert
            Assert.That(target.Contains(LogicalNames.FIElDDATAEXPIRACAO), Is.True);
        }

        [Test]
        public void Execute_Should_ThrowException_When_DuplicateExistsOnUpdate()
        {
            // Arrange
            var occurrenceId = Guid.NewGuid();
            var cpf = "12345678901";
            var tipo = TestDataFactory.CreateOccurrenceTypeReference();
            var assunto = TestDataFactory.CreateSubjectOptionSet();

            var target = new Entity(LogicalNames.ENTITYLOGICALNAME) { Id = occurrenceId };
            target[LogicalNames.FIElDCPF] = cpf;

            var preImage = TestDataFactory.CreateOpenOccurrence(id: occurrenceId, cpf: cpf, tipo: tipo, assunto: assunto);
            
            var context = Context.GetDefaultPluginContext();
            context.InputParameters["Target"] = target;
            context.PreEntityImages["PreImage"] = preImage;

            _mockRepository.Setup(r => r.ExistsOpenSameCpfTypeSubject(cpf, tipo, assunto, Service, occurrenceId))
                .Returns(true);

            // Act & Assert
            var ex = Assert.Throws<InvalidPluginExecutionException>(() => _service.Execute(context, Service));
            Assert.That(ex.Message, Does.Contain("Já existe outra ocorrência"));
        }

        [Test]
        public void Execute_Should_DoNothing_When_TargetIsNotOccurrence()
        {
            // Arrange
            var target = new Entity("another_entity") { Id = Guid.NewGuid() };
            var context = Context.GetDefaultPluginContext();
            context.InputParameters["Target"] = target;

            // Act & Assert
            Assert.DoesNotThrow(() => _service.Execute(context, Service));
        }

        #endregion
    }
}
