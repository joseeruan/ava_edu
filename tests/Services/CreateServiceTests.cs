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
    public class CreateServiceTests : FakeXrmEasyTestBase
    {
        private CreateService _service;
        private Mock<IRepository> _mockRepository;

        [SetUp]
        public override void Setup()
        {
            base.Setup();
            _mockRepository = new Mock<IRepository>();
            _service = new CreateService(_mockRepository.Object);
        }

        #region Execute Tests

        [Test]
        public void Execute_Should_SetCreationDate_When_NotPresent()
        {
            // Arrange
            var entity = TestDataFactory.CreateOccurrence(dataCriacao: null);
            var context = Context.GetDefaultPluginContext();
            context.InputParameters["Target"] = entity;

            _mockRepository.Setup(r => r.RetrieveResponseDeadlineHours(It.IsAny<EntityReference>(), It.IsAny<IOrganizationService>()))
                .Returns(24);

            _mockRepository.Setup(r => r.ExistsOpenSameCpfTypeSubject(
                It.IsAny<string>(), 
                It.IsAny<EntityReference>(), 
                It.IsAny<OptionSetValue>(), 
                It.IsAny<IOrganizationService>(), 
                null))
                .Returns(false);

            // Act
            _service.Execute(context, Service);

            // Assert
            Assert.That(entity.Contains(LogicalNames.FIElDDATACRIACAO), Is.True);
            Assert.That(entity.GetAttributeValue<DateTime>(LogicalNames.FIElDDATACRIACAO), Is.EqualTo(DateTime.UtcNow).Within(TimeSpan.FromSeconds(5)));
        }

        [Test]
        public void Execute_Should_CalculateExpirationDate_When_TypeHasDeadline()
        {
            // Arrange
            var tipoRef = TestDataFactory.CreateOccurrenceTypeReference();
            var entity = TestDataFactory.CreateOccurrence(tipo: tipoRef);
            var context = Context.GetDefaultPluginContext();
            context.InputParameters["Target"] = entity;

            var expectedHours = 48;
            _mockRepository.Setup(r => r.RetrieveResponseDeadlineHours(tipoRef, Service))
                .Returns(expectedHours);

            _mockRepository.Setup(r => r.ExistsOpenSameCpfTypeSubject(
                It.IsAny<string>(), 
                It.IsAny<EntityReference>(), 
                It.IsAny<OptionSetValue>(), 
                It.IsAny<IOrganizationService>(), 
                null))
                .Returns(false);

            // Act
            _service.Execute(context, Service);

            // Assert
            Assert.That(entity.Contains(LogicalNames.FIElDDATAEXPIRACAO), Is.True);
            var expiracao = entity.GetAttributeValue<DateTime>(LogicalNames.FIElDDATAEXPIRACAO);
            var expected = DateTime.UtcNow.AddHours(expectedHours);
            Assert.That(expiracao, Is.EqualTo(expected).Within(TimeSpan.FromSeconds(5)));
        }

        [Test]
        public void Execute_Should_UseDefaultDeadline_When_TypeHasNoDeadline()
        {
            // Arrange
            var tipoRef = TestDataFactory.CreateOccurrenceTypeReference();
            var entity = TestDataFactory.CreateOccurrence(tipo: tipoRef);
            var context = Context.GetDefaultPluginContext();
            context.InputParameters["Target"] = entity;

            _mockRepository.Setup(r => r.RetrieveResponseDeadlineHours(tipoRef, Service))
                .Returns((int?)null);

            _mockRepository.Setup(r => r.ExistsOpenSameCpfTypeSubject(
                It.IsAny<string>(), 
                It.IsAny<EntityReference>(), 
                It.IsAny<OptionSetValue>(), 
                It.IsAny<IOrganizationService>(), 
                null))
                .Returns(false);

            // Act
            _service.Execute(context, Service);

            // Assert
            var expiracao = entity.GetAttributeValue<DateTime>(LogicalNames.FIElDDATAEXPIRACAO);
            var expected = DateTime.UtcNow.AddHours(LogicalNames.PRAZOPADRAOHORAS);
            Assert.That(expiracao, Is.EqualTo(expected).Within(TimeSpan.FromSeconds(5)));
        }

        [Test]
        public void Execute_Should_ThrowException_When_DuplicateExists()
        {
            // Arrange
            var cpf = "12345678901";
            var tipoRef = TestDataFactory.CreateOccurrenceTypeReference();
            var assunto = TestDataFactory.CreateSubjectOptionSet();
            var entity = TestDataFactory.CreateOccurrence(cpf: cpf, tipo: tipoRef, assunto: assunto);
            
            var context = Context.GetDefaultPluginContext();
            context.InputParameters["Target"] = entity;

            _mockRepository.Setup(r => r.RetrieveResponseDeadlineHours(It.IsAny<EntityReference>(), It.IsAny<IOrganizationService>()))
                .Returns(24);

            _mockRepository.Setup(r => r.ExistsOpenSameCpfTypeSubject(cpf, tipoRef, assunto, Service, null))
                .Returns(true);

            // Act & Assert
            var ex = Assert.Throws<InvalidPluginExecutionException>(() => _service.Execute(context, Service));
            Assert.That(ex.Message, Does.Contain("Já existe uma ocorrência em aberto"));
        }

        [Test]
        public void Execute_Should_NotValidateDuplicate_When_CpfIsEmpty()
        {
            // Arrange
            var tipoRef = TestDataFactory.CreateOccurrenceTypeReference();
            var assunto = TestDataFactory.CreateSubjectOptionSet();
            var entity = TestDataFactory.CreateOccurrence(cpf: "", tipo: tipoRef, assunto: assunto);
            
            var context = Context.GetDefaultPluginContext();
            context.InputParameters["Target"] = entity;

            _mockRepository.Setup(r => r.RetrieveResponseDeadlineHours(It.IsAny<EntityReference>(), It.IsAny<IOrganizationService>()))
                .Returns(24);

            // Act & Assert
            Assert.DoesNotThrow(() => _service.Execute(context, Service));
            _mockRepository.Verify(r => r.ExistsOpenSameCpfTypeSubject(
                It.IsAny<string>(), 
                It.IsAny<EntityReference>(), 
                It.IsAny<OptionSetValue>(), 
                It.IsAny<IOrganizationService>(), 
                null), Times.Never);
        }

        [Test]
        public void Execute_Should_NotValidateDuplicate_When_TypeIsNull()
        {
            // Arrange
            var cpf = "12345678901";
            var assunto = TestDataFactory.CreateSubjectOptionSet();
            var entity = TestDataFactory.CreateOccurrence(cpf: cpf, tipo: null, assunto: assunto);
            
            var context = Context.GetDefaultPluginContext();
            context.InputParameters["Target"] = entity;

            _mockRepository.Setup(r => r.RetrieveResponseDeadlineHours(It.IsAny<EntityReference>(), It.IsAny<IOrganizationService>()))
                .Returns(24);

            // Act & Assert
            Assert.DoesNotThrow(() => _service.Execute(context, Service));
            _mockRepository.Verify(r => r.ExistsOpenSameCpfTypeSubject(
                It.IsAny<string>(), 
                It.IsAny<EntityReference>(), 
                It.IsAny<OptionSetValue>(), 
                It.IsAny<IOrganizationService>(), 
                null), Times.Never);
        }

        [Test]
        public void Execute_Should_DoNothing_When_TargetIsNotOccurrence()
        {
            // Arrange
            var entity = new Entity("another_entity") { Id = Guid.NewGuid() };
            var context = Context.GetDefaultPluginContext();
            context.InputParameters["Target"] = entity;

            // Act & Assert
            Assert.DoesNotThrow(() => _service.Execute(context, Service));
            Assert.That(entity.Contains(LogicalNames.FIElDDATACRIACAO), Is.False);
        }

        [Test]
        public void Execute_Should_DoNothing_When_TargetIsNotInInputParameters()
        {
            // Arrange
            var context = Context.GetDefaultPluginContext();
            // Not adding Target to InputParameters

            // Act & Assert
            Assert.DoesNotThrow(() => _service.Execute(context, Service));
        }

        #endregion
    }
}
