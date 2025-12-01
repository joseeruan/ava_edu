# plugins_avaEdu.Tests

Projeto de testes unitários para o plugin Dynamics 365 **plugins_avaEdu**.

## 📋 Estrutura do Projeto

```
tests/
├── Helpers/
│   ├── FakeXrmEasyTestBase.cs    # Classe base para todos os testes
│   └── TestDataFactory.cs         # Factory para criar dados de teste
├── Plugins/
│   ├── CreatePluginTests.cs       # Testes para CreatePlugin
│   ├── UpdatePluginTests.cs       # Testes para UpdatePlugin
│   └── DeletePluginTests.cs       # Testes para DeletePlugin
├── Services/
│   ├── CreateServiceTests.cs      # Testes para CreateService
│   ├── UpdateServiceTests.cs      # Testes para UpdateService
│   └── DeleteServiceTests.cs      # Testes para DeleteService
├── Repository/
│   └── RepositoryTests.cs         # Testes para Repository
└── README.md                      # Este arquivo
```

## 🧪 Frameworks e Ferramentas

- **NUnit 3.13.3** - Framework de testes unitários
- **FakeXrmEasy.365 1.58.1** - Framework para mockar o Dynamics 365 CRM
- **Moq 4.18.4** - Framework para criar mocks de interfaces
- **Microsoft.NET.Test.Sdk 17.8.0** - SDK de testes do .NET
- **.NET Framework 4.6.2** - Target framework

## 🚀 Como Executar os Testes

### Via Linha de Comando (PowerShell)

```powershell
# Navegar até o diretório do projeto
cd "c:\Users\j.dos.santos.paiva\Downloads\BMAD - TESTEFINAL\.bmad\src\projeto\projeto"

# Restaurar pacotes NuGet
dotnet restore

# Compilar o projeto de testes
dotnet build tests/plugins_avaEdu.Tests.csproj

# Executar todos os testes
dotnet test tests/plugins_avaEdu.Tests.csproj

# Executar testes com mais detalhes
dotnet test tests/plugins_avaEdu.Tests.csproj --verbosity detailed

# Executar testes de um namespace específico
dotnet test tests/plugins_avaEdu.Tests.csproj --filter "FullyQualifiedName~Plugins"
dotnet test tests/plugins_avaEdu.Tests.csproj --filter "FullyQualifiedName~Services"
dotnet test tests/plugins_avaEdu.Tests.csproj --filter "FullyQualifiedName~Repository"
```

### Via Visual Studio 2022

1. Abra o arquivo `plugins_avaEdu.sln` no Visual Studio
2. O projeto de teste aparecerá como um projeto separado na solução
3. Abra o **Test Explorer** (Menu: Test → Test Explorer)
4. Clique em "Run All Tests" para executar todos os testes
5. Ou clique com o botão direito em um teste específico e selecione "Run"

### Via Visual Studio Code

```powershell
# Instale a extensão .NET Core Test Explorer
# Execute os testes via Command Palette (Ctrl+Shift+P):
.NET: Run All Tests
```

## 📊 Cobertura de Testes

### Resumo de Testes por Componente

| Componente | Classe | Número de Testes | Status |
|-----------|--------|------------------|--------|
| **Plugins** | CreatePlugin | 3 | ✅ |
| **Plugins** | UpdatePlugin | 3 | ✅ |
| **Plugins** | DeletePlugin | 3 | ✅ |
| **Services** | CreateService | 9 | ✅ |
| **Services** | UpdateService | 5 | ✅ |
| **Services** | DeleteService | 5 | ✅ |
| **Repository** | Repository | 21 | ✅ |
| **TOTAL** | - | **49 testes** | ✅ |

## 🎯 O que é Testado

### CreatePlugin / CreateService
- ✅ Definição automática da data de criação
- ✅ Cálculo da data de expiração baseado no tipo de ocorrência
- ✅ Uso do prazo padrão quando tipo não tem prazo definido
- ✅ Validação de duplicatas (CPF + Tipo + Assunto)
- ✅ Prevenção de criação de ocorrências duplicadas abertas

### UpdatePlugin / UpdateService
- ✅ Definição da data de conclusão ao fechar ocorrência
- ✅ Bloqueio de edição de ocorrências fechadas
- ✅ Recálculo da data de expiração quando tipo é alterado
- ✅ Validação de duplicatas na atualização
- ✅ Validação de campos protegidos (Nome, Email, CPF, etc.)

### DeletePlugin / DeleteService
- ✅ Permissão de exclusão de ocorrências abertas
- ✅ Bloqueio de exclusão de ocorrências fechadas

### Repository
- ✅ Operações CRUD (Create, Retrieve, Update)
- ✅ Consulta de duplicatas com múltiplos filtros
- ✅ Verificação de status (IsClosed)
- ✅ Recuperação de prazo de resposta do tipo
- ✅ Tratamento de casos extremos (null, empty, etc.)

## 🏗️ Padrões de Teste Utilizados

### Padrão AAA (Arrange-Act-Assert)
Todos os testes seguem o padrão AAA:

```csharp
[Test]
public void Execute_Should_SetCreationDate_When_NotPresent()
{
    // Arrange - Preparar o cenário de teste
    var entity = TestDataFactory.CreateOccurrence(dataCriacao: null);
    var context = Context.GetDefaultPluginContext();
    context.InputParameters["Target"] = entity;

    // Act - Executar a ação sendo testada
    _service.Execute(context, Service);

    // Assert - Verificar o resultado esperado
    Assert.That(entity.Contains(LogicalNames.FIElDDATACRIACAO), Is.True);
}
```

### Nomenclatura de Testes
Formato: `MethodName_Should_ExpectedBehavior_When_Condition`

Exemplos:
- `Create_Should_CreateEntity_When_ValidEntityProvided`
- `Execute_Should_ThrowException_When_DuplicateExists`
- `IsClosed_Should_ReturnTrue_When_StatusIsFechado`

### Uso de Mocks
Utilizamos Moq para mockar dependências:

```csharp
var mockRepository = new Mock<IRepository>();
mockRepository.Setup(r => r.ExistsOpenSameCpfTypeSubject(
    It.IsAny<string>(), 
    It.IsAny<EntityReference>(), 
    It.IsAny<OptionSetValue>(), 
    It.IsAny<IOrganizationService>(), 
    null))
    .Returns(false);
```

### Uso de FakeXrmEasy
Para testes de plugins, utilizamos FakeXrmEasy:

```csharp
var pluginContext = Context.GetDefaultPluginContext();
pluginContext.MessageName = "Create";
pluginContext.Stage = 20; // PreOperation
pluginContext.InputParameters["Target"] = occurrence;

Context.ExecutePluginWith<CreatePlugin>(pluginContext);
```

## 🔧 Troubleshooting

### Erro: "Could not load file or assembly FakeXrmEasy"
```powershell
dotnet restore tests/plugins_avaEdu.Tests.csproj
```

### Erro: "The type or namespace name 'NUnit' could not be found"
```powershell
dotnet build tests/plugins_avaEdu.Tests.csproj
```

### Testes não aparecem no Test Explorer do Visual Studio
1. Feche e reabra o Visual Studio
2. Limpe a solução: Build → Clean Solution
3. Rebuild: Build → Rebuild Solution
4. Reabra o Test Explorer

## 📝 Notas Importantes

### Separação de Projetos
Este projeto de teste está **separado do projeto principal** (`plugins_avaEdu.csproj`). Isso garante:

- ✅ Pacotes de teste não são incluídos no assembly de produção
- ✅ Build independente (compilar testes sem recompilar o plugin)
- ✅ Organização clara e separação de responsabilidades
- ✅ Melhor experiência no Visual Studio (projetos separados no Solution Explorer)
- ✅ Padrão da indústria para projetos .NET

### Referências de Projeto
O projeto de teste referencia o projeto principal através de `ProjectReference`:

```xml
<ItemGroup>
  <ProjectReference Include="..\plugins_avaEdu.csproj" />
</ItemGroup>
```

### .NET Framework 4.6.2
O projeto utiliza .NET Framework 4.6.2 (mesma versão do projeto principal) para garantir compatibilidade com Dynamics 365 on-premises.

## 🤝 Contribuindo

Ao adicionar novos testes:

1. Siga o padrão AAA (Arrange-Act-Assert)
2. Use nomes descritivos: `Method_Should_Behavior_When_Condition`
3. Crie um teste por cenário (não multiple asserts em um teste)
4. Use `TestDataFactory` para criar dados de teste
5. Herde de `FakeXrmEasyTestBase` para testes de integração
6. Use `Mock<T>` para mockar dependências em testes unitários
7. Documente cenários complexos com comentários

## 📚 Recursos Adicionais

- [NUnit Documentation](https://docs.nunit.org/)
- [FakeXrmEasy Documentation](https://dynamicsvalue.github.io/fake-xrm-easy-docs/)
- [Moq Documentation](https://github.com/moq/moq4)
- [Dynamics 365 Plugin Best Practices](https://docs.microsoft.com/en-us/power-apps/developer/data-platform/best-practices/business-logic/)

## 👥 Autor

**Jose** - AVA-QA Agent

---

**Última Atualização:** 28 de Novembro de 2025
