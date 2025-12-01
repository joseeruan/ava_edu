using FakeXrmEasy;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using NUnit.Framework;
using plugins_avaEdu.repository.occurrence.Impl;
using plugins_avaEdu.Tests.Helpers;
using plugins_avaEdu.utils;
using System;
using System.Linq;

namespace plugins_avaEdu.Tests.Repository
{
    [TestFixture]
    public class RepositoryTests : FakeXrmEasyTestBase
    {
        private plugins_avaEdu.repository.occurrence.Impl.Repository _repository;

        [SetUp]
        public override void Setup()
        {
            base.Setup();
            _repository = new plugins_avaEdu.repository.occurrence.Impl.Repository();
        }

        #region Create Tests

        [Test]
        public void Create_Should_CreateEntity_When_ValidEntityProvided()
        {
            // Arrange
            var entity = TestDataFactory.CreateOccurrence();

            // Act
            var result = _repository.Create(entity, Service);

            // Assert
            Assert.That(result, Is.Not.EqualTo(Guid.Empty));
            var created = Service.Retrieve(LogicalNames.ENTITYLOGICALNAME, result, new ColumnSet(true));
            Assert.That(created, Is.Not.Null);
            Assert.That(created.Id, Is.EqualTo(result));
        }

        [Test]
        public void Create_Should_ReturnGuid_When_EntityCreatedSuccessfully()
        {
            // Arrange
            var entity = TestDataFactory.CreateOccurrence(cpf: "98765432100");

            // Act
            var result = _repository.Create(entity, Service);

            // Assert
            Assert.That(result, Is.TypeOf<Guid>());
            Assert.That(result, Is.Not.EqualTo(Guid.Empty));
        }

        #endregion

        #region Retrieve Tests

        [Test]
        public void Retrieve_Should_ReturnEntity_When_EntityExists()
        {
            // Arrange
            var occurrenceId = Guid.NewGuid();
            var entity = TestDataFactory.CreateOccurrence(id: occurrenceId);
            InitializeContext(entity);

            // Act
            var result = _repository.Retrieve(occurrenceId, Service);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.EqualTo(occurrenceId));
            Assert.That(result.LogicalName, Is.EqualTo(LogicalNames.ENTITYLOGICALNAME));
        }

        [Test]
        public void Retrieve_Should_ReturnEntityWithSpecificColumns_When_ColumnSetProvided()
        {
            // Arrange
            var occurrenceId = Guid.NewGuid();
            var entity = TestDataFactory.CreateOccurrence(id: occurrenceId);
            InitializeContext(entity);
            var columnSet = new ColumnSet(LogicalNames.FIElDNOME, LogicalNames.FIElDCPF);

            // Act
            var result = _repository.Retrieve(occurrenceId, Service, columnSet);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Contains(LogicalNames.FIElDNOME), Is.True);
            Assert.That(result.Contains(LogicalNames.FIElDCPF), Is.True);
        }

        [Test]
        public void Retrieve_Should_ReturnAllColumns_When_NoColumnSetProvided()
        {
            // Arrange
            var occurrenceId = Guid.NewGuid();
            var entity = TestDataFactory.CreateOccurrence(id: occurrenceId);
            InitializeContext(entity);

            // Act
            var result = _repository.Retrieve(occurrenceId, Service);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Attributes.Count, Is.GreaterThan(0));
        }

        #endregion

        #region Update Tests

        [Test]
        public void Update_Should_UpdateEntity_When_ValidEntityProvided()
        {
            // Arrange
            var occurrenceId = Guid.NewGuid();
            var entity = TestDataFactory.CreateOccurrence(id: occurrenceId, nome: "Original Name");
            InitializeContext(entity);

            var updatedEntity = new Entity(LogicalNames.ENTITYLOGICALNAME)
            {
                Id = occurrenceId
            };
            updatedEntity[LogicalNames.FIElDNOME] = "Updated Name";

            // Act
            _repository.Update(updatedEntity, Service);

            // Assert
            var retrieved = Service.Retrieve(LogicalNames.ENTITYLOGICALNAME, occurrenceId, new ColumnSet(LogicalNames.FIElDNOME));
            Assert.That(retrieved.GetAttributeValue<string>(LogicalNames.FIElDNOME), Is.EqualTo("Updated Name"));
        }

        [Test]
        public void Update_Should_NotThrowException_When_EntityUpdatedSuccessfully()
        {
            // Arrange
            var entity = TestDataFactory.CreateOccurrence();
            InitializeContext(entity);

            entity[LogicalNames.FIElDEMAIL] = "newemail@example.com";

            // Act & Assert
            Assert.DoesNotThrow(() => _repository.Update(entity, Service));
        }

        #endregion

        #region ExistsOpenSameCpfTypeSubject Tests

        [Test]
        public void ExistsOpenSameCpfTypeSubject_Should_ReturnTrue_When_DuplicateExists()
        {
            // Arrange
            var cpf = "12345678901";
            var tipoId = Guid.NewGuid();
            var tipo = TestDataFactory.CreateOccurrenceTypeReference(tipoId);
            var assunto = TestDataFactory.CreateSubjectOptionSet(100000001);

            var existingOccurrence = TestDataFactory.CreateOpenOccurrence(
                cpf: cpf,
                tipo: tipo,
                assunto: assunto);

            InitializeContext(existingOccurrence);

            // Act
            var result = _repository.ExistsOpenSameCpfTypeSubject(cpf, tipo, assunto, Service);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void ExistsOpenSameCpfTypeSubject_Should_ReturnFalse_When_NoDuplicateExists()
        {
            // Arrange
            var cpf = "12345678901";
            var tipo = TestDataFactory.CreateOccurrenceTypeReference();
            var assunto = TestDataFactory.CreateSubjectOptionSet(100000001);

            InitializeContext(); // Empty context

            // Act
            var result = _repository.ExistsOpenSameCpfTypeSubject(cpf, tipo, assunto, Service);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void ExistsOpenSameCpfTypeSubject_Should_ReturnFalse_When_DuplicateIsIgnored()
        {
            // Arrange
            var cpf = "12345678901";
            var occurrenceId = Guid.NewGuid();
            var tipo = TestDataFactory.CreateOccurrenceTypeReference();
            var assunto = TestDataFactory.CreateSubjectOptionSet(100000001);

            var existingOccurrence = TestDataFactory.CreateOpenOccurrence(
                id: occurrenceId,
                cpf: cpf,
                tipo: tipo,
                assunto: assunto);

            InitializeContext(existingOccurrence);

            // Act - Ignoring the same occurrence ID
            var result = _repository.ExistsOpenSameCpfTypeSubject(cpf, tipo, assunto, Service, occurrenceId);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void ExistsOpenSameCpfTypeSubject_Should_ReturnFalse_When_ExistingOccurrenceIsClosed()
        {
            // Arrange
            var cpf = "12345678901";
            var tipo = TestDataFactory.CreateOccurrenceTypeReference();
            var assunto = TestDataFactory.CreateSubjectOptionSet(100000001);

            var closedOccurrence = TestDataFactory.CreateClosedOccurrence(
                cpf: cpf,
                tipo: tipo,
                assunto: assunto);

            InitializeContext(closedOccurrence);

            // Act
            var result = _repository.ExistsOpenSameCpfTypeSubject(cpf, tipo, assunto, Service);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void ExistsOpenSameCpfTypeSubject_Should_ReturnFalse_When_CpfIsNull()
        {
            // Arrange
            var tipo = TestDataFactory.CreateOccurrenceTypeReference();
            var assunto = TestDataFactory.CreateSubjectOptionSet();

            // Act
            var result = _repository.ExistsOpenSameCpfTypeSubject(null, tipo, assunto, Service);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void ExistsOpenSameCpfTypeSubject_Should_ReturnFalse_When_TypeIsNull()
        {
            // Arrange
            var cpf = "12345678901";
            var assunto = TestDataFactory.CreateSubjectOptionSet();

            // Act
            var result = _repository.ExistsOpenSameCpfTypeSubject(cpf, null, assunto, Service);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void ExistsOpenSameCpfTypeSubject_Should_ReturnFalse_When_SubjectIsNull()
        {
            // Arrange
            var cpf = "12345678901";
            var tipo = TestDataFactory.CreateOccurrenceTypeReference();

            // Act
            var result = _repository.ExistsOpenSameCpfTypeSubject(cpf, tipo, null, Service);

            // Assert
            Assert.That(result, Is.False);
        }

        #endregion

        #region IsClosed Tests

        [Test]
        public void IsClosed_Should_ReturnTrue_When_StatusIsFechado()
        {
            // Arrange
            var entity = TestDataFactory.CreateClosedOccurrence();

            // Act
            var result = _repository.IsClosed(entity);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void IsClosed_Should_ReturnFalse_When_StatusIsAberto()
        {
            // Arrange
            var entity = TestDataFactory.CreateOpenOccurrence();

            // Act
            var result = _repository.IsClosed(entity);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void IsClosed_Should_ReturnFalse_When_StatusIsNotPresent()
        {
            // Arrange
            var entity = TestDataFactory.CreateOccurrence(status: null);

            // Act
            var result = _repository.IsClosed(entity);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void IsClosed_Should_ReturnFalse_When_EntityIsNull()
        {
            // Act
            var result = _repository.IsClosed(null);

            // Assert
            Assert.That(result, Is.False);
        }

        #endregion

        #region RetrieveResponseDeadlineHours Tests

        [Test]
        public void RetrieveResponseDeadlineHours_Should_ReturnDeadlineHours_When_TypeExists()
        {
            // Arrange
            var tipoId = Guid.NewGuid();
            var expectedHours = 72;
            var tipoEntity = TestDataFactory.CreateOccurrenceType(id: tipoId, prazoHoras: expectedHours);
            InitializeContext(tipoEntity);

            var tipoRef = TestDataFactory.CreateOccurrenceTypeReference(tipoId);

            // Act
            var result = _repository.RetrieveResponseDeadlineHours(tipoRef, Service);

            // Assert
            Assert.That(result, Is.EqualTo(expectedHours));
        }

        [Test]
        public void RetrieveResponseDeadlineHours_Should_ReturnNull_When_TypeRefIsNull()
        {
            // Act
            var result = _repository.RetrieveResponseDeadlineHours(null, Service);

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public void RetrieveResponseDeadlineHours_Should_ReturnNull_When_DeadlineFieldNotPresent()
        {
            // Arrange
            var tipoId = Guid.NewGuid();
            var tipoEntity = new Entity(LogicalNames.FIELDTIPO)
            {
                Id = tipoId
            };
            // Not setting PRAZO field
            InitializeContext(tipoEntity);

            var tipoRef = TestDataFactory.CreateOccurrenceTypeReference(tipoId);

            // Act
            var result = _repository.RetrieveResponseDeadlineHours(tipoRef, Service);

            // Assert
            Assert.That(result, Is.Null);
        }

        #endregion
    }
}
