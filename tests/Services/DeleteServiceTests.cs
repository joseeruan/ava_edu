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
    public class DeleteServiceTests : FakeXrmEasyTestBase
    {
        private DeleteService _service;
        private Mock<IRepository> _mockRepository;

        [SetUp]
        public override void Setup()
        {
            base.Setup();
            _mockRepository = new Mock<IRepository>();
            _service = new DeleteService(_mockRepository.Object);
        }

        #region Execute Tests

        [Test]
        public void Execute_Should_AllowDeletion_When_OccurrenceIsOpen()
        {
            // Arrange
            var occurrenceId = Guid.NewGuid();
            var entityRef = new EntityReference(LogicalNames.ENTITYLOGICALNAME, occurrenceId);
            var context = Context.GetDefaultPluginContext();
            context.InputParameters["Target"] = entityRef;

            var openOccurrence = TestDataFactory.CreateOpenOccurrence(id: occurrenceId);
            _mockRepository.Setup(r => r.Retrieve(occurrenceId, Service, It.IsAny<Microsoft.Xrm.Sdk.Query.ColumnSet>()))
                .Returns(openOccurrence);
            _mockRepository.Setup(r => r.IsClosed(openOccurrence))
                .Returns(false);

            // Act & Assert
            Assert.DoesNotThrow(() => _service.Execute(context, Service));
        }

        [Test]
        public void Execute_Should_ThrowException_When_OccurrenceIsClosed()
        {
            // Arrange
            var occurrenceId = Guid.NewGuid();
            var entityRef = new EntityReference(LogicalNames.ENTITYLOGICALNAME, occurrenceId);
            var context = Context.GetDefaultPluginContext();
            context.InputParameters["Target"] = entityRef;

            var closedOccurrence = TestDataFactory.CreateClosedOccurrence(id: occurrenceId);
            _mockRepository.Setup(r => r.Retrieve(occurrenceId, Service, It.IsAny<Microsoft.Xrm.Sdk.Query.ColumnSet>()))
                .Returns(closedOccurrence);
            _mockRepository.Setup(r => r.IsClosed(closedOccurrence))
                .Returns(true);

            // Act & Assert
            var ex = Assert.Throws<InvalidPluginExecutionException>(() => _service.Execute(context, Service));
            Assert.That(ex.Message, Does.Contain("Ocorrência fechada não pode ser apagada"));
        }

        [Test]
        public void Execute_Should_DoNothing_When_TargetIsNotOccurrence()
        {
            // Arrange
            var entityRef = new EntityReference("another_entity", Guid.NewGuid());
            var context = Context.GetDefaultPluginContext();
            context.InputParameters["Target"] = entityRef;

            // Act & Assert
            Assert.DoesNotThrow(() => _service.Execute(context, Service));
            _mockRepository.Verify(r => r.Retrieve(It.IsAny<Guid>(), It.IsAny<IOrganizationService>(), It.IsAny<Microsoft.Xrm.Sdk.Query.ColumnSet>()), Times.Never);
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

        [Test]
        public void Execute_Should_CallRepositoryRetrieve_When_ValidTargetProvided()
        {
            // Arrange
            var occurrenceId = Guid.NewGuid();
            var entityRef = new EntityReference(LogicalNames.ENTITYLOGICALNAME, occurrenceId);
            var context = Context.GetDefaultPluginContext();
            context.InputParameters["Target"] = entityRef;

            var openOccurrence = TestDataFactory.CreateOpenOccurrence(id: occurrenceId);
            _mockRepository.Setup(r => r.Retrieve(occurrenceId, Service, It.IsAny<Microsoft.Xrm.Sdk.Query.ColumnSet>()))
                .Returns(openOccurrence);
            _mockRepository.Setup(r => r.IsClosed(openOccurrence))
                .Returns(false);

            // Act
            _service.Execute(context, Service);

            // Assert
            _mockRepository.Verify(r => r.Retrieve(occurrenceId, Service, It.IsAny<Microsoft.Xrm.Sdk.Query.ColumnSet>()), Times.Once);
            _mockRepository.Verify(r => r.IsClosed(openOccurrence), Times.Once);
        }

        #endregion
    }
}
