using System;
using Microsoft.Xrm.Sdk;
using NUnit.Framework;
using plugins_avaEdu.service.occurrence.Impl;
using plugins_avaEdu.utils;
using plugins_avaEdu.repository.occurrence.Impl;

namespace plugins_avaEdu.tests.AvaEdu.Tests
{
    [TestFixture]
    public class DeleteServiceTests
    {
        private CrmTestContext _ctx;
        private DeleteService _service;

        [SetUp]
        public void Setup()
        {
            _ctx = new CrmTestContext();
            _service = new DeleteService(new Repository());
        }

        private Entity NewOccurrence(Guid id, int status)
        {
            var ent = new Entity(LogicalNames.ENTITYLOGICALNAME) { Id = id };
            ent[LogicalNames.FIElDSTATUS] = new OptionSetValue(status);
            return ent;
        }

        [Test]
        public void Should_Block_Delete_If_Closed()
        {
            var id = Guid.NewGuid();
            var ent = NewOccurrence(id, LogicalNames.STATUSFECHADO);
            _ctx.Seed(ent);

            var targetRef = new EntityReference(LogicalNames.ENTITYLOGICALNAME, id);
            var pluginCtx = _ctx.CreatePluginContext("Delete", targetRef, id);

            Assert.Throws<InvalidPluginExecutionException>(() => _service.Execute(pluginCtx, _ctx.Service));
        }

        [Test]
        public void Should_Allow_Delete_If_Open()
        {
            var id = Guid.NewGuid();
            var ent = NewOccurrence(id, LogicalNames.STATUSABERTO);
            _ctx.Seed(ent);

            var targetRef = new EntityReference(LogicalNames.ENTITYLOGICALNAME, id);
            var pluginCtx = _ctx.CreatePluginContext("Delete", targetRef, id);

            Assert.DoesNotThrow(() => _service.Execute(pluginCtx, _ctx.Service));
        }

        [Test]
        public void Should_Allow_Delete_If_Overdue()
        {
            var id = Guid.NewGuid();
            var ent = NewOccurrence(id, LogicalNames.STATUSATRASADO);
            _ctx.Seed(ent);

            var targetRef = new EntityReference(LogicalNames.ENTITYLOGICALNAME, id);
            var pluginCtx = _ctx.CreatePluginContext("Delete", targetRef, id);

            Assert.DoesNotThrow(() => _service.Execute(pluginCtx, _ctx.Service));
        }
    }
}
