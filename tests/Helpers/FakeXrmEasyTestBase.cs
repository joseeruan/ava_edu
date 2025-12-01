using FakeXrmEasy;
using Microsoft.Xrm.Sdk;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace plugins_avaEdu.Tests.Helpers
{
    /// <summary>
    /// Base class for all FakeXrmEasy tests.
    /// Provides common setup and helper methods for testing Dynamics 365 plugins.
    /// </summary>
    public abstract class FakeXrmEasyTestBase
    {
        protected XrmFakedContext Context { get; private set; }
        protected IOrganizationService Service { get; private set; }
        protected Guid UserId { get; private set; }

        [SetUp]
        public virtual void Setup()
        {
            Context = new XrmFakedContext();
            Service = Context.GetOrganizationService();
            UserId = Guid.NewGuid();
        }

        [TearDown]
        public virtual void TearDown()
        {
            Context = null;
            Service = null;
        }

        /// <summary>
        /// Initializes the context with test entities.
        /// </summary>
        protected void InitializeContext(params Entity[] entities)
        {
            if (entities != null && entities.Length > 0)
            {
                Context.Initialize(entities);
            }
        }

        /// <summary>
        /// Initializes the context with a collection of entities.
        /// </summary>
        protected void InitializeContext(IEnumerable<Entity> entities)
        {
            if (entities != null)
            {
                Context.Initialize(entities);
            }
        }

        /// <summary>
        /// Creates a basic Entity with Id and LogicalName.
        /// </summary>
        protected Entity CreateEntity(string logicalName, Guid? id = null)
        {
            var entity = new Entity(logicalName)
            {
                Id = id ?? Guid.NewGuid()
            };
            return entity;
        }

        /// <summary>
        /// Creates an EntityReference.
        /// </summary>
        protected EntityReference CreateEntityReference(string logicalName, Guid id)
        {
            return new EntityReference(logicalName, id);
        }

        /// <summary>
        /// Creates an OptionSetValue.
        /// </summary>
        protected OptionSetValue CreateOptionSetValue(int value)
        {
            return new OptionSetValue(value);
        }
    }
}
