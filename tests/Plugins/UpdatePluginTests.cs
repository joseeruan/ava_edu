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
    public class UpdatePluginTests : FakeXrmEasyTestBase
    {
        private UpdatePlugin _plugin;

        [SetUp]
        public override void Setup()
        {
            base.Setup();
            _plugin = new UpdatePlugin();
        }

        [Test]
        public void Execute_Should_SetConclusionDate_When_StatusChangedToClosed()
        {
            // Arrange
            var occurrenceId = Guid.NewGuid();
            var preImage = TestDataFactory.CreateOpenOccurrence(id: occurrenceId);

            var target = new Entity(LogicalNames.ENTITYLOGICALNAME) { Id = occurrenceId };
            target[LogicalNames.FIElDSTATUS] = TestDataFactory.CreateStatusOptionSet(LogicalNames.STATUSFECHADO);

            InitializeContext(preImage);

            var pluginContext = Context.GetDefaultPluginContext();
            pluginContext.MessageName = "Update";
            pluginContext.Stage = 20;
            pluginContext.InputParameters["Target"] = target;
            pluginContext.PreEntityImages["PreImage"] = preImage;

            // Act
            Context.ExecutePluginWith<UpdatePlugin>(pluginContext);

            // Assert
            Assert.That(target.Contains(LogicalNames.FIElDDATACONCLUSAO), Is.True);
        }

        [Test]
        public void Execute_Should_ThrowException_When_ClosedOccurrenceModified()
        {
            // Arrange
            var occurrenceId = Guid.NewGuid();
            var preImage = TestDataFactory.CreateClosedOccurrence(id: occurrenceId, nome: "Original Name");

            var target = new Entity(LogicalNames.ENTITYLOGICALNAME) { Id = occurrenceId };
            target[LogicalNames.FIElDNOME] = "Modified Name";

            InitializeContext(preImage);

            var pluginContext = Context.GetDefaultPluginContext();
            pluginContext.MessageName = "Update";
            pluginContext.Stage = 20;
            pluginContext.InputParameters["Target"] = target;
            pluginContext.PreEntityImages["PreImage"] = preImage;

            // Act & Assert
            var ex = Assert.Throws<InvalidPluginExecutionException>(() => 
                Context.ExecutePluginWith<UpdatePlugin>(pluginContext));
            Assert.That(ex.Message, Does.Contain("fechada não pode"));
        }

        [Test]
        public void Execute_Should_RecalculateExpirationDate_When_TypeChanged()
        {
            // Arrange
            var occurrenceId = Guid.NewGuid();
            var oldTipoId = Guid.NewGuid();
            var newTipoId = Guid.NewGuid();

            var oldTipo = TestDataFactory.CreateOccurrenceType(id: oldTipoId, prazoHoras: 24);
            var newTipo = TestDataFactory.CreateOccurrenceType(id: newTipoId, prazoHoras: 72);

            var preImage = TestDataFactory.CreateOpenOccurrence(
                id: occurrenceId, 
                tipo: TestDataFactory.CreateOccurrenceTypeReference(oldTipoId));

            var target = new Entity(LogicalNames.ENTITYLOGICALNAME) { Id = occurrenceId };
            target[LogicalNames.FIELDTIPO] = TestDataFactory.CreateOccurrenceTypeReference(newTipoId);

            InitializeContext(oldTipo, newTipo, preImage);

            var pluginContext = Context.GetDefaultPluginContext();
            pluginContext.MessageName = "Update";
            pluginContext.Stage = 20;
            pluginContext.InputParameters["Target"] = target;
            pluginContext.PreEntityImages["PreImage"] = preImage;

            // Act
            Context.ExecutePluginWith<UpdatePlugin>(pluginContext);

            // Assert
            Assert.That(target.Contains(LogicalNames.FIElDDATAEXPIRACAO), Is.True);
        }
    }
}
