using System;
using FakeXrmEasy;
using Microsoft.Xrm.Sdk;

namespace plugins_avaEdu.tests
{

    namespace AvaEdu.Tests
    {
        internal interface ICrmTestContext
        {
            IOrganizationService Service { get; }
            void Seed(params Entity[] entities);
            IPluginExecutionContext CreatePluginContext(string messageName, object target, Guid? primaryId = null);
        }

        internal class CrmTestContext : ICrmTestContext
        {
            private readonly XrmFakedContext _inner = new XrmFakedContext();
            public IOrganizationService Service { get; }

            public CrmTestContext()
            {
                Service = _inner.GetOrganizationService();
            }

            public void Seed(params Entity[] entities)
            {
                if (entities != null && entities.Length > 0)
                {
                    _inner.Initialize(entities);
                }
            }

            public IPluginExecutionContext CreatePluginContext(string messageName, object target, Guid? primaryId = null)
            {
                var ctx = _inner.GetDefaultPluginContext();
                ctx.InputParameters.Clear();
                if (target != null)
                {
                    if (target is Entity e)
                    {
                        if (primaryId.HasValue && primaryId.Value != Guid.Empty)
                        {
                            e.Id = primaryId.Value;
                        }
                        ctx.InputParameters["Target"] = e;
                        ((XrmFakedPluginExecutionContext)ctx).PrimaryEntityId = e.Id;
                    }
                    else if (target is EntityReference er)
                    {
                        ctx.InputParameters["Target"] = er;
                        ((XrmFakedPluginExecutionContext)ctx).PrimaryEntityId = er.Id;
                    }
                }
                ((XrmFakedPluginExecutionContext)ctx).MessageName = messageName;
                return ctx;
            }
        }
    }

}
