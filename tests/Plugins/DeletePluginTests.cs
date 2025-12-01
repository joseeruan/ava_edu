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
    public class DeletePluginTests : FakeXrmEasyTestBase
    {
        private DeletePlugin _plugin;

        [SetUp]
        public override void Setup()
        {
            base.Setup();
            _plugin = new DeletePlugin();
        }

        [Test]
        public void Execute_Should_AllowDeletion_When_OccurrenceIsOpen()
        {
            // Arrange
            var occurrenceId = Guid.NewGuid();
            var occurrence = TestDataFactory.CreateOpenOccurrence(id: occurrenceId);
            InitializeContext(occurrence);

            var entityRef = new EntityReference(LogicalNames.ENTITYLOGICALNAME, occurrenceId);

            var pluginContext = Context.GetDefaultPluginContext();
            pluginContext.MessageName = "Delete";
            pluginContext.Stage = 20;
            pluginContext.InputParameters["Target"] = entityRef;

            // Act & Assert
            Assert.DoesNotThrow(() => Context.ExecutePluginWith<DeletePlugin>(pluginContext));
        }

        [Test]
        public void Execute_Should_ThrowException_When_OccurrenceIsClosed()
        {
            // Arrange
            var occurrenceId = Guid.NewGuid();
            var occurrence = TestDataFactory.CreateClosedOccurrence(id: occurrenceId);
            InitializeContext(occurrence);

            var entityRef = new EntityReference(LogicalNames.ENTITYLOGICALNAME, occurrenceId);

            var pluginContext = Context.GetDefaultPluginContext();
            pluginContext.MessageName = "Delete";
            pluginContext.Stage = 20;
            pluginContext.InputParameters["Target"] = entityRef;

            // Act & Assert
            var ex = Assert.Throws<InvalidPluginExecutionException>(() => 
                Context.ExecutePluginWith<DeletePlugin>(pluginContext));
            Assert.That(ex.Message, Does.Contain("fechada não pode ser apagada"));
        }

        [Test]
        public void Execute_Should_PreventDeletion_When_StatusIsFechado()
        {
            // Arrange
            var occurrenceId = Guid.NewGuid();
            var occurrence = TestDataFactory.CreateOccurrence(
                id: occurrenceId,
                status: TestDataFactory.CreateStatusOptionSet(LogicalNames.STATUSFECHADO),
                dataConclusao: DateTime.UtcNow);
            InitializeContext(occurrence);

            var entityRef = new EntityReference(LogicalNames.ENTITYLOGICALNAME, occurrenceId);

            var pluginContext = Context.GetDefaultPluginContext();
            pluginContext.MessageName = "Delete";
            pluginContext.Stage = 20;
            pluginContext.InputParameters["Target"] = entityRef;

            // Act & Assert
            Assert.Throws<InvalidPluginExecutionException>(() => 
                Context.ExecutePluginWith<DeletePlugin>(pluginContext));
        }
    }
}
