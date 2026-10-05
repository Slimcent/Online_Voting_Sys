using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;
using OnlineVoting.Models.Constants;
using OnlineVoting.Models.Dtos.Request;
using OnlineVoting.Models.Dtos.Response;
using OnlineVoting.Models.Entities;
using OnlineVoting.Models.Pagination;
using OnlineVoting.Models.Results;
using OnlineVoting.Services.Caching.Tags;
using OnlineVoting.Tests.Factories;
using OnlineVoting.Tests.TestData;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace OnlineVoting.Tests.Services
{
    public class PositionApplicationServiceTests
    {
        [Fact]
        public async Task CreatePositionApplication_ReturnsValidationError_WhenCurrentUserIsMissing()
        {
            using PositionApplicationServiceFactory factory = new PositionApplicationServiceFactory();

            factory.CurrentUserContext.Setup(x => x.UserId)
                .Returns((string?)null);

            CreatePositionApplicationRequest request = PositionApplicationTestData.CreateRequest();

            Result<CreatePositionApplicationResponse> result = await factory.Service.CreatePositionApplication(request);

            Assert.Equal(ResultStatus.ValidationError, result.Status);
            Assert.Equal("The current user could not be identified.", result.Error);
            Assert.Null(result.Value);
        }

        [Fact]
        public async Task CreatePositionApplication_ReturnsConflict_WhenIdempotencyRequestIsProcessing()
        {
            using PositionApplicationServiceFactory factory = new PositionApplicationServiceFactory();

            string userId = Guid.NewGuid().ToString();
            string electionPositionId = Guid.NewGuid().ToString();
            string idempotencyKey = Guid.NewGuid().ToString();
            string requestHash = CreateRequestHash(electionPositionId);

            factory.CurrentUserContext.Setup(x => x.UserId)
                .Returns(userId);

            IdempotencyRecord idempotencyRecord = PositionApplicationTestData.CreateIdempotencyRecord(userId, idempotencyKey, requestHash,
                ApplicationConstants.IdempotencyStatuses.Processing);

            factory.DbContextFactory.Context.IdempotencyRecords.Add(idempotencyRecord);
            await factory.DbContextFactory.Context.SaveChangesAsync();

            CreatePositionApplicationRequest request = PositionApplicationTestData.CreateRequest(electionPositionId, idempotencyKey);

            Result<CreatePositionApplicationResponse> result = await factory.Service.CreatePositionApplication(request);

            Assert.Equal(ResultStatus.Conflict, result.Status);
            Assert.Null(result.Value);
        }

        [Fact]
        public async Task CreatePositionApplication_ReturnsCreated_WhenCompletedIdempotencyRequestWasCreated()
        {
            using PositionApplicationServiceFactory factory = new PositionApplicationServiceFactory();

            string userId = Guid.NewGuid().ToString();
            string electionPositionId = Guid.NewGuid().ToString();
            string idempotencyKey = Guid.NewGuid().ToString();
            string requestHash = CreateRequestHash(electionPositionId);

            factory.CurrentUserContext.Setup(x => x.UserId)
                .Returns(userId);

            CreatePositionApplicationResponse storedResponse = PositionApplicationTestData.CreatePositionApplicationResponse();

            IdempotencyRecord idempotencyRecord = PositionApplicationTestData.CreateIdempotencyRecord(userId, idempotencyKey, requestHash,
                ApplicationConstants.IdempotencyStatuses.Completed);

            idempotencyRecord.ResourceId = storedResponse.PositionApplicationId;
            idempotencyRecord.StatusCode = StatusCodes.Status201Created;
            idempotencyRecord.Response = JsonSerializer.Serialize(storedResponse);

            factory.DbContextFactory.Context.IdempotencyRecords.Add(idempotencyRecord);
            await factory.DbContextFactory.Context.SaveChangesAsync();

            CreatePositionApplicationRequest request = PositionApplicationTestData.CreateRequest(electionPositionId, idempotencyKey);

            Result<CreatePositionApplicationResponse> result = await factory.Service.CreatePositionApplication(request);

            Assert.Equal(ResultStatus.Created, result.Status);
            Assert.NotNull(result.Value);
            Assert.Equal(storedResponse.PositionApplicationId, result.Value.PositionApplicationId);
            Assert.Equal(storedResponse.InvoiceId, result.Value.InvoiceId);
            Assert.Equal(storedResponse.InvoiceNumber, result.Value.InvoiceNumber);
            Assert.Equal(storedResponse.Amount, result.Value.Amount);
            Assert.Equal(storedResponse.Currency, result.Value.Currency);
            Assert.Equal(storedResponse.ApplicationStatus, result.Value.ApplicationStatus);
            Assert.Equal(storedResponse.InvoiceStatus, result.Value.InvoiceStatus);

            factory.PaymentService.Verify(x => x.CreateInvoice(It.IsAny<CreateInvoiceRequest>()), Times.Never);
        }

        [Fact]
        public async Task CreatePositionApplication_ReturnsSuccess_WhenCompletedIdempotencyRequestReturnedExistingApplication()
        {
            using PositionApplicationServiceFactory factory = new PositionApplicationServiceFactory();

            string userId = Guid.NewGuid().ToString();
            string electionPositionId = Guid.NewGuid().ToString();
            string idempotencyKey = Guid.NewGuid().ToString();
            string requestHash = CreateRequestHash(electionPositionId);

            factory.CurrentUserContext.Setup(x => x.UserId)
                .Returns(userId);

            CreatePositionApplicationResponse storedResponse = PositionApplicationTestData.CreatePositionApplicationResponse();

            IdempotencyRecord idempotencyRecord = PositionApplicationTestData.CreateIdempotencyRecord(userId, idempotencyKey, requestHash,
                ApplicationConstants.IdempotencyStatuses.Completed);

            idempotencyRecord.ResourceId = storedResponse.PositionApplicationId;
            idempotencyRecord.StatusCode = StatusCodes.Status200OK;
            idempotencyRecord.Response = JsonSerializer.Serialize(storedResponse);

            factory.DbContextFactory.Context.IdempotencyRecords.Add(idempotencyRecord);
            await factory.DbContextFactory.Context.SaveChangesAsync();

            CreatePositionApplicationRequest request = PositionApplicationTestData.CreateRequest(electionPositionId, idempotencyKey);

            Result<CreatePositionApplicationResponse> result = await factory.Service.CreatePositionApplication(request);

            Assert.Equal(ResultStatus.Success, result.Status);
            Assert.NotNull(result.Value);
            Assert.Equal(storedResponse.PositionApplicationId, result.Value.PositionApplicationId);
            Assert.Equal(storedResponse.InvoiceId, result.Value.InvoiceId);

            factory.PaymentService.Verify(x => x.CreateInvoice(It.IsAny<CreateInvoiceRequest>()), Times.Never);
        }

        [Fact]
        public async Task CreatePositionApplication_ReturnsConflict_WhenIdempotencyKeyIsReusedForDifferentRequest()
        {
            using PositionApplicationServiceFactory factory = new PositionApplicationServiceFactory();

            string userId = Guid.NewGuid().ToString();
            string originalElectionPositionId = Guid.NewGuid().ToString();
            string newElectionPositionId = Guid.NewGuid().ToString();
            string idempotencyKey = Guid.NewGuid().ToString();
            string requestHash = CreateRequestHash(originalElectionPositionId);

            factory.CurrentUserContext.Setup(x => x.UserId)
                .Returns(userId);

            IdempotencyRecord idempotencyRecord = PositionApplicationTestData.CreateIdempotencyRecord(userId, idempotencyKey, requestHash,
                ApplicationConstants.IdempotencyStatuses.Processing);

            factory.DbContextFactory.Context.IdempotencyRecords.Add(idempotencyRecord);
            await factory.DbContextFactory.Context.SaveChangesAsync();

            CreatePositionApplicationRequest request = PositionApplicationTestData.CreateRequest(newElectionPositionId, idempotencyKey);

            Result<CreatePositionApplicationResponse> result = await factory.Service.CreatePositionApplication(request);

            Assert.Equal(ResultStatus.Conflict, result.Status);
            Assert.Null(result.Value);
            Assert.Equal("The idempotency key has already been used for a different request.", result.Error);

            factory.PaymentService.Verify(x => x.CreateInvoice(It.IsAny<CreateInvoiceRequest>()), Times.Never);
        }

        [Fact]
        public async Task CreatePositionApplication_ReturnsNotFound_WhenStudentProfileIsNotFound()
        {
            using PositionApplicationServiceFactory factory = new PositionApplicationServiceFactory();

            string userId = Guid.NewGuid().ToString();

            factory.CurrentUserContext.Setup(x => x.UserId)
                .Returns(userId);

            CreatePositionApplicationRequest request = PositionApplicationTestData.CreateRequest();

            Result<CreatePositionApplicationResponse> result = await factory.Service.CreatePositionApplication(request);

            Assert.Equal(ResultStatus.NotFound, result.Status);
            Assert.Null(result.Value);
            Assert.Equal("Student profile was not found.", result.Error);

            factory.PaymentService.Verify(x => x.CreateInvoice(It.IsAny<CreateInvoiceRequest>()), Times.Never);
        }

        [Fact]
        public async Task CreatePositionApplication_ReturnsValidationError_WhenStudentHasNoLinkedUser()
        {
            using PositionApplicationServiceFactory factory = new PositionApplicationServiceFactory();

            string userId = Guid.NewGuid().ToString();

            Student student = new Student
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                RegNumber = "REG001",
                DepartmentId = 1,
                GenderId = 1,
                Active = true
            };

            factory.DbContextFactory.Context.Students.Add(student);
            await factory.DbContextFactory.Context.SaveChangesAsync();

            factory.CurrentUserContext.Setup(x => x.UserId)
                .Returns(userId);

            CreatePositionApplicationRequest request = PositionApplicationTestData.CreateRequest();

            Result<CreatePositionApplicationResponse> result = await factory.Service.CreatePositionApplication(request);

            Assert.Equal(ResultStatus.ValidationError, result.Status);
            Assert.Null(result.Value);
            Assert.Equal("The student profile does not have a valid user account.", result.Error);

            factory.PaymentService.Verify(x => x.CreateInvoice(It.IsAny<CreateInvoiceRequest>()), Times.Never);
        }

        [Fact]
        public async Task CreatePositionApplication_ReturnsValidationError_WhenStudentIsInactive()
        {
            using PositionApplicationServiceFactory factory = new PositionApplicationServiceFactory();

            User user = PositionApplicationTestData.CreateUser();
            Student student = PositionApplicationTestData.CreateStudent(user, false);

            factory.DbContextFactory.Context.Users.Add(user);
            factory.DbContextFactory.Context.Students.Add(student);
            await factory.DbContextFactory.Context.SaveChangesAsync();

            factory.CurrentUserContext.Setup(x => x.UserId)
                .Returns(user.Id);

            CreatePositionApplicationRequest request = PositionApplicationTestData.CreateRequest();

            Result<CreatePositionApplicationResponse> result = await factory.Service.CreatePositionApplication(request);

            Assert.Equal(ResultStatus.ValidationError, result.Status);
            Assert.Null(result.Value);
            Assert.Equal("The student profile is inactive.", result.Error);

            factory.PaymentService.Verify(x => x.CreateInvoice(It.IsAny<CreateInvoiceRequest>()), Times.Never);
        }

        [Fact]
        public async Task CreatePositionApplication_ReturnsValidationError_WhenUserIsInactive()
        {
            using PositionApplicationServiceFactory factory = new PositionApplicationServiceFactory();

            User user = PositionApplicationTestData.CreateUser(active: false);
            Student student = PositionApplicationTestData.CreateStudent(user);

            factory.DbContextFactory.Context.Users.Add(user);
            factory.DbContextFactory.Context.Students.Add(student);
            await factory.DbContextFactory.Context.SaveChangesAsync();

            factory.CurrentUserContext.Setup(x => x.UserId)
                .Returns(user.Id);

            CreatePositionApplicationRequest request = PositionApplicationTestData.CreateRequest();

            Result<CreatePositionApplicationResponse> result = await factory.Service.CreatePositionApplication(request);

            Assert.Equal(ResultStatus.ValidationError, result.Status);
            Assert.Null(result.Value);
            Assert.Equal("The user account is inactive.", result.Error);

            factory.PaymentService.Verify(x => x.CreateInvoice(It.IsAny<CreateInvoiceRequest>()), Times.Never);
        }

        [Fact]
        public async Task CreatePositionApplication_ReturnsNotFound_WhenElectionPositionIsNotFound()
        {
            using PositionApplicationServiceFactory factory = new PositionApplicationServiceFactory();

            User user = PositionApplicationTestData.CreateUser();
            Student student = PositionApplicationTestData.CreateStudent(user);

            factory.DbContextFactory.Context.Users.Add(user);
            factory.DbContextFactory.Context.Students.Add(student);
            await factory.DbContextFactory.Context.SaveChangesAsync();

            factory.CurrentUserContext.Setup(x => x.UserId)
                .Returns(user.Id);

            string electionPositionId = Guid.NewGuid().ToString();

            CreatePositionApplicationRequest request = PositionApplicationTestData.CreateRequest(electionPositionId);

            Result<CreatePositionApplicationResponse> result = await factory.Service.CreatePositionApplication(request);

            Assert.Equal(ResultStatus.NotFound, result.Status);
            Assert.Null(result.Value);
            Assert.Equal($"Election position with id {electionPositionId} was not found.", result.Error);

            factory.PaymentService.Verify(x => x.CreateInvoice(It.IsAny<CreateInvoiceRequest>()), Times.Never);
        }

        [Fact]
        public async Task CreatePositionApplication_ReturnsValidationError_WhenElectionPositionIsInactive()
        {
            using PositionApplicationServiceFactory factory = new PositionApplicationServiceFactory();

            User user = PositionApplicationTestData.CreateUser();
            Student student = PositionApplicationTestData.CreateStudent(user);
            Election election = PositionApplicationTestData.CreateElection();
            ElectionPosition electionPosition = PositionApplicationTestData.CreateElectionPosition(election, false);

            factory.DbContextFactory.Context.Users.Add(user);
            factory.DbContextFactory.Context.Students.Add(student);
            factory.DbContextFactory.Context.Elections.Add(election);
            factory.DbContextFactory.Context.ElectionPositions.Add(electionPosition);
            await factory.DbContextFactory.Context.SaveChangesAsync();

            factory.CurrentUserContext.Setup(x => x.UserId)
                .Returns(user.Id);

            CreatePositionApplicationRequest request = PositionApplicationTestData.CreateRequest(electionPosition.Id);

            Result<CreatePositionApplicationResponse> result = await factory.Service.CreatePositionApplication(request);

            Assert.Equal(ResultStatus.ValidationError, result.Status);
            Assert.Null(result.Value);
            Assert.Equal("The election position is inactive.", result.Error);

            factory.PaymentService.Verify(x => x.CreateInvoice(It.IsAny<CreateInvoiceRequest>()), Times.Never);
        }

        [Fact]
        public async Task CreatePositionApplication_ReturnsValidationError_WhenElectionIsInactive()
        {
            using PositionApplicationServiceFactory factory = new PositionApplicationServiceFactory();

            User user = PositionApplicationTestData.CreateUser();
            Student student = PositionApplicationTestData.CreateStudent(user);
            Election election = PositionApplicationTestData.CreateElection(false);
            ElectionPosition electionPosition = PositionApplicationTestData.CreateElectionPosition(election);

            factory.DbContextFactory.Context.Users.Add(user);
            factory.DbContextFactory.Context.Students.Add(student);
            factory.DbContextFactory.Context.Elections.Add(election);
            factory.DbContextFactory.Context.ElectionPositions.Add(electionPosition);
            await factory.DbContextFactory.Context.SaveChangesAsync();

            factory.CurrentUserContext.Setup(x => x.UserId)
                .Returns(user.Id);

            CreatePositionApplicationRequest request = PositionApplicationTestData.CreateRequest(electionPosition.Id);

            Result<CreatePositionApplicationResponse> result = await factory.Service.CreatePositionApplication(request);

            Assert.Equal(ResultStatus.ValidationError, result.Status);
            Assert.Null(result.Value);
            Assert.Equal("The election is inactive.", result.Error);

            factory.PaymentService.Verify(x => x.CreateInvoice(It.IsAny<CreateInvoiceRequest>()), Times.Never);
        }

        [Fact]
        public async Task CreatePositionApplication_ReturnsValidationError_WhenApplicationPeriodIsNotConfigured()
        {
            using PositionApplicationServiceFactory factory = new PositionApplicationServiceFactory();

            User user = PositionApplicationTestData.CreateUser();
            Student student = PositionApplicationTestData.CreateStudent(user);
            Election election = PositionApplicationTestData.CreateElection();
            election.ApplicationStartAt = null;
            election.ApplicationEndAt = null;

            ElectionPosition electionPosition = PositionApplicationTestData.CreateElectionPosition(election);

            factory.DbContextFactory.Context.Users.Add(user);
            factory.DbContextFactory.Context.Students.Add(student);
            factory.DbContextFactory.Context.Elections.Add(election);
            factory.DbContextFactory.Context.ElectionPositions.Add(electionPosition);
            await factory.DbContextFactory.Context.SaveChangesAsync();

            factory.CurrentUserContext.Setup(x => x.UserId)
                .Returns(user.Id);

            CreatePositionApplicationRequest request = PositionApplicationTestData.CreateRequest(electionPosition.Id);

            Result<CreatePositionApplicationResponse> result = await factory.Service.CreatePositionApplication(request);

            Assert.Equal(ResultStatus.ValidationError, result.Status);
            Assert.Null(result.Value);
            Assert.Equal("The application period has not been configured.", result.Error);

            factory.PaymentService.Verify(x => x.CreateInvoice(It.IsAny<CreateInvoiceRequest>()), Times.Never);
        }

        [Fact]
        public async Task CreatePositionApplication_ReturnsValidationError_WhenApplicationPeriodHasNotStarted()
        {
            using PositionApplicationServiceFactory factory = new PositionApplicationServiceFactory();

            User user = PositionApplicationTestData.CreateUser();
            Student student = PositionApplicationTestData.CreateStudent(user);
            Election election = PositionApplicationTestData.CreateElection(applicationStartAt: DateTime.UtcNow.AddDays(1), applicationEndAt: DateTime.UtcNow.AddDays(2));
            ElectionPosition electionPosition = PositionApplicationTestData.CreateElectionPosition(election);

            factory.DbContextFactory.Context.Users.Add(user);
            factory.DbContextFactory.Context.Students.Add(student);
            factory.DbContextFactory.Context.Elections.Add(election);
            factory.DbContextFactory.Context.ElectionPositions.Add(electionPosition);
            await factory.DbContextFactory.Context.SaveChangesAsync();

            factory.CurrentUserContext.Setup(x => x.UserId)
                .Returns(user.Id);

            CreatePositionApplicationRequest request = PositionApplicationTestData.CreateRequest(electionPosition.Id);

            Result<CreatePositionApplicationResponse> result = await factory.Service.CreatePositionApplication(request);

            Assert.Equal(ResultStatus.ValidationError, result.Status);
            Assert.Null(result.Value);
            Assert.Equal("The application period has not started.", result.Error);

            factory.PaymentService.Verify(x => x.CreateInvoice(It.IsAny<CreateInvoiceRequest>()), Times.Never);
        }

        [Fact]
        public async Task CreatePositionApplication_ReturnsValidationError_WhenApplicationPeriodHasEnded()
        {
            using PositionApplicationServiceFactory factory = new PositionApplicationServiceFactory();

            User user = PositionApplicationTestData.CreateUser();
            Student student = PositionApplicationTestData.CreateStudent(user);
            Election election = PositionApplicationTestData.CreateElection(applicationStartAt: DateTime.UtcNow.AddDays(-2), applicationEndAt: DateTime.UtcNow.AddDays(-1));
            ElectionPosition electionPosition = PositionApplicationTestData.CreateElectionPosition(election);

            factory.DbContextFactory.Context.Users.Add(user);
            factory.DbContextFactory.Context.Students.Add(student);
            factory.DbContextFactory.Context.Elections.Add(election);
            factory.DbContextFactory.Context.ElectionPositions.Add(electionPosition);
            await factory.DbContextFactory.Context.SaveChangesAsync();

            factory.CurrentUserContext.Setup(x => x.UserId)
                .Returns(user.Id);

            CreatePositionApplicationRequest request = PositionApplicationTestData.CreateRequest(electionPosition.Id);

            Result<CreatePositionApplicationResponse> result = await factory.Service.CreatePositionApplication(request);

            Assert.Equal(ResultStatus.ValidationError, result.Status);
            Assert.Null(result.Value);
            Assert.Equal("The application period has ended.", result.Error);

            factory.PaymentService.Verify(x => x.CreateInvoice(It.IsAny<CreateInvoiceRequest>()), Times.Never);
        }

        [Fact]
        public async Task CreatePositionApplication_ReturnsNotFound_WhenPendingPaymentStatusIsNotFound()
        {
            using PositionApplicationServiceFactory factory = new PositionApplicationServiceFactory();

            User user = PositionApplicationTestData.CreateUser();
            Student student = PositionApplicationTestData.CreateStudent(user);
            Election election = PositionApplicationTestData.CreateElection();
            ElectionPosition electionPosition = PositionApplicationTestData.CreateElectionPosition(election);

            factory.DbContextFactory.Context.Users.Add(user);
            factory.DbContextFactory.Context.Students.Add(student);
            factory.DbContextFactory.Context.Elections.Add(election);
            factory.DbContextFactory.Context.ElectionPositions.Add(electionPosition);

            PositionApplicationStatus? pendingPaymentStatus = factory.DbContextFactory.Context.PositionApplicationStatuses
                .FirstOrDefault(x => x.Code == ApplicationConstants.PositionApplicationStatuses.PendingPayment);

            if (pendingPaymentStatus is not null)
            {
                factory.DbContextFactory.Context.PositionApplicationStatuses.Remove(pendingPaymentStatus);
            }

            await factory.DbContextFactory.Context.SaveChangesAsync();

            factory.CurrentUserContext.Setup(x => x.UserId)
                .Returns(user.Id);

            factory.Mapper.Setup(x => x.Map<CreateInvoiceRequest>(It.IsAny<Student>()))
                .Returns(new CreateInvoiceRequest
                {
                    StudentId = student.Id,
                    UserId = user.Id,
                    PayerFirstName = user.FirstName ?? string.Empty,
                    PayerLastName = user.LastName ?? string.Empty,
                    PayerEmail = user.Email,
                    RegistrationNumber = student.RegNumber,
                    Amount = electionPosition.ApplicationFee,
                    Currency = electionPosition.Currency
                });

            CreatePositionApplicationRequest request = PositionApplicationTestData.CreateRequest(electionPosition.Id);

            Result<CreatePositionApplicationResponse> result = await factory.Service.CreatePositionApplication(request);

            Assert.Equal(ResultStatus.NotFound, result.Status);
            Assert.Null(result.Value);
            Assert.Equal("Pending Payment position application status was not found.", result.Error);

            factory.PaymentService.Verify(x => x.CreateInvoice(It.IsAny<CreateInvoiceRequest>()), Times.Never);
        }

        [Fact]
        public async Task CreatePositionApplication_ReturnsCreated_WhenNewApplicationIsCreatedSuccessfully()
        {
            using PositionApplicationServiceFactory factory = new PositionApplicationServiceFactory();

            (User user, Student student, Election election, ElectionPosition electionPosition, PositionApplicationStatus pendingPaymentStatus) =
                await CreateValidApplicationData(factory);

            factory.CurrentUserContext.Setup(x => x.UserId)
                .Returns(user.Id);

            SetupInvoiceRequestMapping(factory, user, student, electionPosition);
            SetupPositionApplicationMapping(factory, pendingPaymentStatus);

            InvoiceResponse invoiceResponse = PositionApplicationTestData.CreateInvoiceResponse(electionPosition.ApplicationFee, electionPosition.Currency);

            factory.PaymentService.Setup(x => x.CreateInvoice(It.IsAny<CreateInvoiceRequest>()))
                .ReturnsAsync(Result<InvoiceResponse>.Created(invoiceResponse));

            CreatePositionApplicationRequest request = PositionApplicationTestData.CreateRequest(electionPosition.Id);

            Result<CreatePositionApplicationResponse> result = await factory.Service.CreatePositionApplication(request);

            Assert.Equal(ResultStatus.Created, result.Status);
            Assert.NotNull(result.Value);
            Assert.Equal(invoiceResponse.Id, result.Value.InvoiceId);
            Assert.Equal(invoiceResponse.InvoiceNumber, result.Value.InvoiceNumber);
            Assert.Equal(invoiceResponse.Amount, result.Value.Amount);
            Assert.Equal(invoiceResponse.Currency, result.Value.Currency);
            Assert.Equal(invoiceResponse.Status, result.Value.InvoiceStatus);
            Assert.Equal("Pending Payment", result.Value.ApplicationStatus);

            PositionApplication? application = factory.DbContextFactory.Context.PositionApplications
                .FirstOrDefault(x => x.StudentId == student.Id && x.ElectionPositionId == electionPosition.Id);

            Assert.NotNull(application);

            IdempotencyRecord? idempotencyRecord = factory.DbContextFactory.Context.IdempotencyRecords
                .FirstOrDefault(x => x.Key == request.IdempotencyKey);

            Assert.NotNull(idempotencyRecord);
            Assert.Equal(ApplicationConstants.IdempotencyStatuses.Completed, idempotencyRecord.Status);
            Assert.Equal(application.Id, idempotencyRecord.ResourceId);
            Assert.Equal(StatusCodes.Status201Created, idempotencyRecord.StatusCode);
            Assert.NotNull(idempotencyRecord.Response);

            factory.PaymentService.Verify(x => x.CreateInvoice(It.Is<CreateInvoiceRequest>(x =>
                x.PositionApplicationId == application.Id)), Times.Once);

            factory.UnitOfWork.Verify(x => x.BeginTransactionAsync(), Times.Once);
            factory.UnitOfWork.Verify(x => x.CommitTransactionAsync(), Times.Once);
            factory.UnitOfWork.Verify(x => x.RollbackTransactionAsync(), Times.Never);

            factory.CacheService.Verify(x => x.RemoveByTag(CacheTags.PositionApplication, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task CreatePositionApplication_ReturnsSuccess_WhenApplicationAlreadyExists()
        {
            using PositionApplicationServiceFactory factory = new PositionApplicationServiceFactory();

            (User user, Student student, Election election, ElectionPosition electionPosition, PositionApplicationStatus pendingPaymentStatus) =
                await CreateValidApplicationData(factory);

            PositionApplication existingApplication = PositionApplicationTestData.CreatePositionApplication(student, electionPosition, pendingPaymentStatus);

            factory.DbContextFactory.Context.PositionApplications.Add(existingApplication);
            await factory.DbContextFactory.Context.SaveChangesAsync();

            factory.CurrentUserContext.Setup(x => x.UserId)
                .Returns(user.Id);

            SetupInvoiceRequestMapping(factory, user, student, electionPosition);

            factory.Mapper.Setup(x => x.Map<CreatePositionApplicationResponse>(It.IsAny<PositionApplication>()))
                .Returns((PositionApplication application) => new CreatePositionApplicationResponse
                {
                    PositionApplicationId = application.Id,
                    ApplicationStatus = application.PositionApplicationStatus.Name
                });

            InvoiceResponse invoiceResponse = PositionApplicationTestData.CreateInvoiceResponse(electionPosition.ApplicationFee, electionPosition.Currency);

            factory.PaymentService.Setup(x => x.CreateInvoice(It.IsAny<CreateInvoiceRequest>()))
                .ReturnsAsync(Result<InvoiceResponse>.Success(invoiceResponse));

            CreatePositionApplicationRequest request = PositionApplicationTestData.CreateRequest(electionPosition.Id);

            Result<CreatePositionApplicationResponse> result = await factory.Service.CreatePositionApplication(request);

            Assert.Equal(ResultStatus.Success, result.Status);
            Assert.NotNull(result.Value);
            Assert.Equal(existingApplication.Id, result.Value.PositionApplicationId);
            Assert.Equal(invoiceResponse.Id, result.Value.InvoiceId);

            IdempotencyRecord? idempotencyRecord = factory.DbContextFactory.Context.IdempotencyRecords
                .FirstOrDefault(x => x.Key == request.IdempotencyKey);

            Assert.NotNull(idempotencyRecord);
            Assert.Equal(ApplicationConstants.IdempotencyStatuses.Completed, idempotencyRecord.Status);
            Assert.Equal(existingApplication.Id, idempotencyRecord.ResourceId);
            Assert.Equal(StatusCodes.Status200OK, idempotencyRecord.StatusCode);

            int applicationCount = factory.DbContextFactory.Context.PositionApplications
                .Count(x => x.StudentId == student.Id && x.ElectionPositionId == electionPosition.Id);

            Assert.Equal(1, applicationCount);

            factory.PaymentService.Verify(x => x.CreateInvoice(It.Is<CreateInvoiceRequest>(x =>
                x.PositionApplicationId == existingApplication.Id)), Times.Once);

            factory.UnitOfWork.Verify(x => x.BeginTransactionAsync(), Times.Once);
            factory.UnitOfWork.Verify(x => x.CommitTransactionAsync(), Times.Once);
            factory.CacheService.Verify(x => x.RemoveByTag(CacheTags.PositionApplication, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task CreatePositionApplication_RetriesFailedIdempotencyRequestSuccessfully()
        {
            using PositionApplicationServiceFactory factory = new PositionApplicationServiceFactory();

            (User user, Student student, Election election, ElectionPosition electionPosition, PositionApplicationStatus pendingPaymentStatus) =
                await CreateValidApplicationData(factory);

            string idempotencyKey = Guid.NewGuid().ToString();
            string requestHash = CreateRequestHash(electionPosition.Id);

            IdempotencyRecord idempotencyRecord = PositionApplicationTestData.CreateIdempotencyRecord(user.Id, idempotencyKey, requestHash,
                ApplicationConstants.IdempotencyStatuses.Failed);

            idempotencyRecord.ResourceId = Guid.NewGuid().ToString();
            idempotencyRecord.StatusCode = StatusCodes.Status500InternalServerError;
            idempotencyRecord.Response = "Previous failure.";

            factory.DbContextFactory.Context.IdempotencyRecords.Add(idempotencyRecord);
            await factory.DbContextFactory.Context.SaveChangesAsync();

            factory.CurrentUserContext.Setup(x => x.UserId)
                .Returns(user.Id);

            SetupInvoiceRequestMapping(factory, user, student, electionPosition);
            SetupPositionApplicationMapping(factory, pendingPaymentStatus);

            InvoiceResponse invoiceResponse = PositionApplicationTestData.CreateInvoiceResponse(electionPosition.ApplicationFee, electionPosition.Currency);

            factory.PaymentService.Setup(x => x.CreateInvoice(It.IsAny<CreateInvoiceRequest>()))
                .ReturnsAsync(Result<InvoiceResponse>.Created(invoiceResponse));

            CreatePositionApplicationRequest request = PositionApplicationTestData.CreateRequest(electionPosition.Id, idempotencyKey);

            Result<CreatePositionApplicationResponse> result = await factory.Service.CreatePositionApplication(request);

            Assert.Equal(ResultStatus.Created, result.Status);

            IdempotencyRecord? updatedRecord = factory.DbContextFactory.Context.IdempotencyRecords
                .FirstOrDefault(x => x.Id == idempotencyRecord.Id);

            Assert.NotNull(updatedRecord);
            Assert.Equal(ApplicationConstants.IdempotencyStatuses.Completed, updatedRecord.Status);
            Assert.Equal(StatusCodes.Status201Created, updatedRecord.StatusCode);
            Assert.NotNull(updatedRecord.ResourceId);
            Assert.NotNull(updatedRecord.Response);

            factory.PaymentService.Verify(x => x.CreateInvoice(It.IsAny<CreateInvoiceRequest>()), Times.Once);
        }

        [Fact]
        public async Task CreatePositionApplication_RollsBackAndMarksIdempotencyFailed_WhenInvoiceCreationFails()
        {
            using PositionApplicationServiceFactory factory = new PositionApplicationServiceFactory();

            (User user, Student student, Election election, ElectionPosition electionPosition, PositionApplicationStatus pendingPaymentStatus) =
                await CreateValidApplicationData(factory);

            factory.CurrentUserContext.Setup(x => x.UserId)
                .Returns(user.Id);

            SetupInvoiceRequestMapping(factory, user, student, electionPosition);
            SetupPositionApplicationMapping(factory, pendingPaymentStatus);

            factory.PaymentService.Setup(x => x.CreateInvoice(It.IsAny<CreateInvoiceRequest>()))
                .ReturnsAsync(Result<InvoiceResponse>.ValidationError("Invoice creation failed."));

            CreatePositionApplicationRequest request = PositionApplicationTestData.CreateRequest(electionPosition.Id);

            Result<CreatePositionApplicationResponse> result = await factory.Service.CreatePositionApplication(request);

            Assert.Equal(ResultStatus.ValidationError, result.Status);
            Assert.Null(result.Value);
            Assert.Equal("Invoice creation failed.", result.Error);

            PositionApplication? application = factory.DbContextFactory.Context.PositionApplications
                .FirstOrDefault(x => x.StudentId == student.Id && x.ElectionPositionId == electionPosition.Id);

            Assert.Null(application);

            IdempotencyRecord? idempotencyRecord = factory.DbContextFactory.Context.IdempotencyRecords
                .FirstOrDefault(x => x.Key == request.IdempotencyKey);

            Assert.NotNull(idempotencyRecord);
            Assert.Equal(ApplicationConstants.IdempotencyStatuses.Failed, idempotencyRecord.Status);
            Assert.Null(idempotencyRecord.ResourceId);
            Assert.Equal(StatusCodes.Status400BadRequest, idempotencyRecord.StatusCode);
            Assert.Equal("Invoice creation failed.", idempotencyRecord.Response);

            factory.UnitOfWork.Verify(x => x.BeginTransactionAsync(), Times.Once);
            factory.UnitOfWork.Verify(x => x.RollbackTransactionAsync(), Times.Once);
            factory.UnitOfWork.Verify(x => x.CommitTransactionAsync(), Times.Never);
        }

        [Fact]
        public async Task CreatePositionApplication_MarksIdempotencyFailed_WhenInvoiceCreationFailsForExistingApplication()
        {
            using PositionApplicationServiceFactory factory = new PositionApplicationServiceFactory();

            (User user, Student student, Election election, ElectionPosition electionPosition, PositionApplicationStatus pendingPaymentStatus) =
                await CreateValidApplicationData(factory);

            PositionApplication existingApplication = PositionApplicationTestData.CreatePositionApplication(student, electionPosition, pendingPaymentStatus);

            factory.DbContextFactory.Context.PositionApplications.Add(existingApplication);
            await factory.DbContextFactory.Context.SaveChangesAsync();

            factory.CurrentUserContext.Setup(x => x.UserId)
                .Returns(user.Id);

            SetupInvoiceRequestMapping(factory, user, student, electionPosition);

            factory.PaymentService.Setup(x => x.CreateInvoice(It.IsAny<CreateInvoiceRequest>()))
                .ReturnsAsync(Result<InvoiceResponse>.NotFound("Invoice could not be created."));

            CreatePositionApplicationRequest request = PositionApplicationTestData.CreateRequest(electionPosition.Id);

            Result<CreatePositionApplicationResponse> result = await factory.Service.CreatePositionApplication(request);

            Assert.Equal(ResultStatus.NotFound, result.Status);
            Assert.Null(result.Value);
            Assert.Equal("Invoice could not be created.", result.Error);

            IdempotencyRecord? idempotencyRecord = factory.DbContextFactory.Context.IdempotencyRecords
                .FirstOrDefault(x => x.Key == request.IdempotencyKey);

            Assert.NotNull(idempotencyRecord);
            Assert.Equal(ApplicationConstants.IdempotencyStatuses.Failed, idempotencyRecord.Status);
            Assert.Equal(existingApplication.Id, idempotencyRecord.ResourceId);
            Assert.Equal(StatusCodes.Status404NotFound, idempotencyRecord.StatusCode);
            Assert.Equal("Invoice could not be created.", idempotencyRecord.Response);

            PositionApplication? application = factory.DbContextFactory.Context.PositionApplications
                .FirstOrDefault(x => x.Id == existingApplication.Id);

            Assert.NotNull(application);

            factory.UnitOfWork.Verify(x => x.RollbackTransactionAsync(), Times.Once);
            factory.UnitOfWork.Verify(x => x.CommitTransactionAsync(), Times.Never);
        }

        [Fact]
        public async Task CreatePositionApplication_MarksIdempotencyFailedAndRethrows_WhenApplicationCreationThrows()
        {
            using PositionApplicationServiceFactory factory = new PositionApplicationServiceFactory();

            (User user, Student student, Election election, ElectionPosition electionPosition, PositionApplicationStatus pendingPaymentStatus) =
                await CreateValidApplicationData(factory);

            factory.CurrentUserContext.Setup(x => x.UserId)
                .Returns(user.Id);

            SetupInvoiceRequestMapping(factory, user, student, electionPosition);
            SetupPositionApplicationMapping(factory, pendingPaymentStatus);

            factory.PositionApplicationRepository.Setup(x => x.AddAsync(It.IsAny<PositionApplication>(), It.IsAny<bool>()))
                .ThrowsAsync(new InvalidOperationException("Database insert failed."));

            CreatePositionApplicationRequest request = PositionApplicationTestData.CreateRequest(electionPosition.Id);

            InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                factory.Service.CreatePositionApplication(request));

            Assert.Equal("Database insert failed.", exception.Message);

            IdempotencyRecord? idempotencyRecord = factory.DbContextFactory.Context.IdempotencyRecords
                .FirstOrDefault(x => x.Key == request.IdempotencyKey);

            Assert.NotNull(idempotencyRecord);
            Assert.Equal(ApplicationConstants.IdempotencyStatuses.Failed, idempotencyRecord.Status);
            Assert.Null(idempotencyRecord.ResourceId);
            Assert.Equal(StatusCodes.Status500InternalServerError, idempotencyRecord.StatusCode);
            Assert.Equal("Position application creation failed.", idempotencyRecord.Response);

            factory.PaymentService.Verify(x => x.CreateInvoice(It.IsAny<CreateInvoiceRequest>()), Times.Never);

            factory.UnitOfWork.Verify(x => x.RollbackTransactionAsync(), Times.Once);
            factory.UnitOfWork.Verify(x => x.CommitTransactionAsync(), Times.Never);
        }

        [Fact]
        public async Task CreatePositionApplication_ReturnsConflictAndStores409_WhenInvoiceReturnsConflict()
        {
            using PositionApplicationServiceFactory factory = new PositionApplicationServiceFactory();

            (User user, Student student, Election election, ElectionPosition electionPosition, PositionApplicationStatus pendingPaymentStatus) =
                await CreateValidApplicationData(factory);

            factory.CurrentUserContext.Setup(x => x.UserId)
                .Returns(user.Id);

            SetupInvoiceRequestMapping(factory, user, student, electionPosition);
            SetupPositionApplicationMapping(factory, pendingPaymentStatus);

            factory.PaymentService.Setup(x => x.CreateInvoice(It.IsAny<CreateInvoiceRequest>()))
                .ReturnsAsync(Result<InvoiceResponse>.Conflict("Invoice already exists for another resource."));

            CreatePositionApplicationRequest request = PositionApplicationTestData.CreateRequest(electionPosition.Id);

            Result<CreatePositionApplicationResponse> result = await factory.Service.CreatePositionApplication(request);

            Assert.Equal(ResultStatus.Conflict, result.Status);
            Assert.Equal("Invoice already exists for another resource.", result.Error);

            IdempotencyRecord? idempotencyRecord = factory.DbContextFactory.Context.IdempotencyRecords
                .FirstOrDefault(x => x.Key == request.IdempotencyKey);

            Assert.NotNull(idempotencyRecord);
            Assert.Equal(ApplicationConstants.IdempotencyStatuses.Failed, idempotencyRecord.Status);
            Assert.Equal(StatusCodes.Status409Conflict, idempotencyRecord.StatusCode);
        }

        [Fact]
        public async Task CreatePositionApplication_ReturnsForbiddenAndStores403_WhenInvoiceReturnsForbidden()
        {
            using PositionApplicationServiceFactory factory = new PositionApplicationServiceFactory();

            (User user, Student student, Election election, ElectionPosition electionPosition, PositionApplicationStatus pendingPaymentStatus) =
                await CreateValidApplicationData(factory);

            factory.CurrentUserContext.Setup(x => x.UserId)
                .Returns(user.Id);

            SetupInvoiceRequestMapping(factory, user, student, electionPosition);
            SetupPositionApplicationMapping(factory, pendingPaymentStatus);

            factory.PaymentService.Setup(x => x.CreateInvoice(It.IsAny<CreateInvoiceRequest>()))
                .ReturnsAsync(Result<InvoiceResponse>.Forbidden("Invoice creation is not allowed."));

            CreatePositionApplicationRequest request = PositionApplicationTestData.CreateRequest(electionPosition.Id);

            Result<CreatePositionApplicationResponse> result = await factory.Service.CreatePositionApplication(request);

            Assert.Equal(ResultStatus.Forbidden, result.Status);
            Assert.Equal("Invoice creation is not allowed.", result.Error);

            IdempotencyRecord? idempotencyRecord = factory.DbContextFactory.Context.IdempotencyRecords
                .FirstOrDefault(x => x.Key == request.IdempotencyKey);

            Assert.NotNull(idempotencyRecord);
            Assert.Equal(ApplicationConstants.IdempotencyStatuses.Failed, idempotencyRecord.Status);
            Assert.Equal(StatusCodes.Status403Forbidden, idempotencyRecord.StatusCode);
        }

        [Fact]
        public async Task CancelPositionApplication_ReturnsValidationError_WhenCurrentUserIsMissing()
        {
            using PositionApplicationServiceFactory factory = new PositionApplicationServiceFactory();

            factory.CurrentUserContext.Setup(x => x.UserId)
                .Returns((string?)null);

            Result<string> result = await factory.Service.CancelPositionApplication(Guid.NewGuid().ToString());

            Assert.Equal(ResultStatus.ValidationError, result.Status);
            Assert.Equal("The current user could not be identified.", result.Error);
            Assert.Null(result.Value);

            factory.CacheService.Verify(x => x.RemoveByTag(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task CancelPositionApplication_ReturnsNotFound_WhenPositionApplicationDoesNotExist()
        {
            using PositionApplicationServiceFactory factory = new PositionApplicationServiceFactory();

            string userId = Guid.NewGuid().ToString();
            string positionApplicationId = Guid.NewGuid().ToString();

            factory.CurrentUserContext.Setup(x => x.UserId)
                .Returns(userId);

            Result<string> result = await factory.Service.CancelPositionApplication(positionApplicationId);

            Assert.Equal(ResultStatus.NotFound, result.Status);
            Assert.Equal($"Position application with id {positionApplicationId} was not found.", result.Error);
            Assert.Null(result.Value);
        }

        [Fact]
        public async Task CancelPositionApplication_ReturnsForbidden_WhenPositionApplicationBelongsToAnotherStudent()
        {
            using PositionApplicationServiceFactory factory = new PositionApplicationServiceFactory();

            string currentUserId = Guid.NewGuid().ToString();
            string applicationOwnerUserId = Guid.NewGuid().ToString();

            Student student = new Student
            {
                Id = Guid.NewGuid(),
                UserId = applicationOwnerUserId,
                Active = true
            };

            PositionApplicationStatus pendingPaymentStatus = new PositionApplicationStatus
            {
                Id = 1,
                Code = ApplicationConstants.PositionApplicationStatuses.PendingPayment,
                Name = "Pending Payment"
            };

            PositionApplication positionApplication = new PositionApplication
            {
                Id = Guid.NewGuid().ToString(),
                StudentId = student.Id,
                ElectionPositionId = Guid.NewGuid().ToString(),
                PositionApplicationStatusId = pendingPaymentStatus.Id,
                Student = student,
                PositionApplicationStatus = pendingPaymentStatus
            };

            factory.DbContextFactory.Context.Students.Add(student);
            factory.DbContextFactory.Context.PositionApplicationStatuses.Add(pendingPaymentStatus);
            factory.DbContextFactory.Context.PositionApplications.Add(positionApplication);

            await factory.DbContextFactory.Context.SaveChangesAsync();

            factory.CurrentUserContext.Setup(x => x.UserId)
                .Returns(currentUserId);

            Result<string> result = await factory.Service.CancelPositionApplication(positionApplication.Id);

            Assert.Equal(ResultStatus.Forbidden, result.Status);
            Assert.Equal("You are not allowed to cancel this position application.", result.Error);
            Assert.Null(result.Value);
        }

        [Fact]
        public async Task CancelPositionApplication_ReturnsSuccess_WhenPositionApplicationWasAlreadyWithdrawn()
        {
            using PositionApplicationServiceFactory factory = new PositionApplicationServiceFactory();

            string userId = Guid.NewGuid().ToString();

            Student student = new Student
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Active = true
            };

            PositionApplicationStatus withdrawnStatus = new PositionApplicationStatus
            {
                Id = 5,
                Code = ApplicationConstants.PositionApplicationStatuses.Withdrawn,
                Name = "Withdrawn"
            };

            PositionApplication positionApplication = new PositionApplication
            {
                Id = Guid.NewGuid().ToString(),
                StudentId = student.Id,
                ElectionPositionId = Guid.NewGuid().ToString(),
                PositionApplicationStatusId = withdrawnStatus.Id,
                Student = student,
                PositionApplicationStatus = withdrawnStatus
            };

            factory.DbContextFactory.Context.Students.Add(student);
            factory.DbContextFactory.Context.PositionApplicationStatuses.Add(withdrawnStatus);
            factory.DbContextFactory.Context.PositionApplications.Add(positionApplication);

            await factory.DbContextFactory.Context.SaveChangesAsync();

            factory.CurrentUserContext.Setup(x => x.UserId)
                .Returns(userId);

            Result<string> result = await factory.Service.CancelPositionApplication(positionApplication.Id);

            Assert.Equal(ResultStatus.Success, result.Status);
            Assert.Equal("Position application has already been cancelled.", result.Value);
        }

        [Fact]
        public async Task CancelPositionApplication_ReturnsConflict_WhenPositionApplicationIsNotPendingPayment()
        {
            using PositionApplicationServiceFactory factory = new PositionApplicationServiceFactory();

            string userId = Guid.NewGuid().ToString();

            Student student = new Student
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Active = true
            };

            PositionApplicationStatus pendingReviewStatus = new PositionApplicationStatus
            {
                Id = 2,
                Code = ApplicationConstants.PositionApplicationStatuses.PendingReview,
                Name = "Pending Review"
            };

            PositionApplication positionApplication = new PositionApplication
            {
                Id = Guid.NewGuid().ToString(),
                StudentId = student.Id,
                ElectionPositionId = Guid.NewGuid().ToString(),
                PositionApplicationStatusId = pendingReviewStatus.Id,
                Student = student,
                PositionApplicationStatus = pendingReviewStatus
            };

            factory.DbContextFactory.Context.Students.Add(student);
            factory.DbContextFactory.Context.PositionApplicationStatuses.Add(pendingReviewStatus);
            factory.DbContextFactory.Context.PositionApplications.Add(positionApplication);

            await factory.DbContextFactory.Context.SaveChangesAsync();

            factory.CurrentUserContext.Setup(x => x.UserId)
                .Returns(userId);

            Result<string> result = await factory.Service.CancelPositionApplication(positionApplication.Id);

            Assert.Equal(ResultStatus.Conflict, result.Status);
            Assert.Equal("Only a position application awaiting payment can be cancelled.", result.Error);
            Assert.Null(result.Value);
        }

        [Fact]
        public async Task CancelPositionApplication_ReturnsConflict_WhenInvoiceHasAlreadyBeenPaid()
        {
            using PositionApplicationServiceFactory factory = new PositionApplicationServiceFactory();

            string userId = Guid.NewGuid().ToString();

            Student student = new Student
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Active = true
            };

            PositionApplicationStatus pendingPaymentStatus = new PositionApplicationStatus
            {
                Id = 1,
                Code = ApplicationConstants.PositionApplicationStatuses.PendingPayment,
                Name = "Pending Payment"
            };

            InvoiceStatus paidInvoiceStatus = new InvoiceStatus
            {
                Id = 2,
                Code = ApplicationConstants.InvoiceStatuses.Paid,
                Name = "Paid"
            };

            PositionApplication positionApplication = new PositionApplication
            {
                Id = Guid.NewGuid().ToString(),
                StudentId = student.Id,
                ElectionPositionId = Guid.NewGuid().ToString(),
                PositionApplicationStatusId = pendingPaymentStatus.Id,
                Student = student,
                PositionApplicationStatus = pendingPaymentStatus
            };

            Invoice invoice = new Invoice
            {
                Id = Guid.NewGuid().ToString(),
                PositionApplicationId = positionApplication.Id,
                InvoiceNumber = $"INV-{Guid.NewGuid():N}",
                StudentId = student.Id,
                UserId = userId,
                PayerFirstName = "Test",
                PayerLastName = "Student",
                Amount = 5000m,
                Currency = "NGN",
                InvoiceStatusId = paidInvoiceStatus.Id,
                InvoiceStatus = paidInvoiceStatus,
                PositionApplication = positionApplication
            };

            positionApplication.Invoice = invoice;

            factory.DbContextFactory.Context.Students.Add(student);
            factory.DbContextFactory.Context.PositionApplicationStatuses.Add(pendingPaymentStatus);
            factory.DbContextFactory.Context.InvoiceStatuses.Add(paidInvoiceStatus);
            factory.DbContextFactory.Context.PositionApplications.Add(positionApplication);
            factory.DbContextFactory.Context.Invoices.Add(invoice);

            await factory.DbContextFactory.Context.SaveChangesAsync();

            factory.CurrentUserContext.Setup(x => x.UserId)
                .Returns(userId);

            Result<string> result = await factory.Service.CancelPositionApplication(positionApplication.Id);

            Assert.Equal(ResultStatus.Conflict, result.Status);
            Assert.Equal("A paid position application cannot be cancelled.", result.Error);
            Assert.Null(result.Value);
        }

        [Fact]
        public async Task CancelPositionApplication_WithValidApplication_ShouldWithdrawApplicationAndCancelInvoice()
        {
            using PositionApplicationServiceFactory factory = new PositionApplicationServiceFactory();

            string userId = Guid.NewGuid().ToString();

            Student student = new Student
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Active = true
            };

            PositionApplicationStatus pendingPaymentStatus = new PositionApplicationStatus
            {
                Id = 1,
                Code = ApplicationConstants.PositionApplicationStatuses.PendingPayment,
                Name = "Pending Payment"
            };

            PositionApplicationStatus withdrawnStatus = new PositionApplicationStatus
            {
                Id = 5,
                Code = ApplicationConstants.PositionApplicationStatuses.Withdrawn,
                Name = "Withdrawn"
            };

            InvoiceStatus unpaidInvoiceStatus = new InvoiceStatus
            {
                Id = 1,
                Code = ApplicationConstants.InvoiceStatuses.Unpaid,
                Name = "Unpaid"
            };

            InvoiceStatus cancelledInvoiceStatus = new InvoiceStatus
            {
                Id = 3,
                Code = ApplicationConstants.InvoiceStatuses.Cancelled,
                Name = "Cancelled"
            };

            PositionApplication positionApplication = new PositionApplication
            {
                Id = Guid.NewGuid().ToString(),
                StudentId = student.Id,
                ElectionPositionId = Guid.NewGuid().ToString(),
                PositionApplicationStatusId = pendingPaymentStatus.Id,
                Student = student,
                PositionApplicationStatus = pendingPaymentStatus
            };

            Invoice invoice = new Invoice
            {
                Id = Guid.NewGuid().ToString(),
                PositionApplicationId = positionApplication.Id,
                InvoiceNumber = $"INV-{Guid.NewGuid():N}",
                StudentId = student.Id,
                UserId = userId,
                PayerFirstName = "Test",
                PayerLastName = "Student",
                Amount = 5000m,
                Currency = "NGN",
                InvoiceStatusId = unpaidInvoiceStatus.Id,
                InvoiceStatus = unpaidInvoiceStatus,
                PositionApplication = positionApplication
            };

            positionApplication.Invoice = invoice;

            factory.DbContextFactory.Context.Students.Add(student);
            factory.DbContextFactory.Context.PositionApplicationStatuses.AddRange(
                pendingPaymentStatus,
                withdrawnStatus);

            factory.DbContextFactory.Context.InvoiceStatuses.AddRange(
                unpaidInvoiceStatus,
                cancelledInvoiceStatus);

            factory.DbContextFactory.Context.PositionApplications.Add(positionApplication);
            factory.DbContextFactory.Context.Invoices.Add(invoice);

            await factory.DbContextFactory.Context.SaveChangesAsync();

            factory.CurrentUserContext.Setup(x => x.UserId)
                .Returns(userId);

            Result<string> result = await factory.Service.CancelPositionApplication(positionApplication.Id);
                        
            Assert.Equal(ResultStatus.Success, result.Status);
            Assert.Equal("Position application cancelled successfully.", result.Value);

            factory.DbContextFactory.Context.ChangeTracker.Clear();

            PositionApplication? updatedPositionApplication = factory.DbContextFactory.Context.PositionApplications
                .SingleOrDefault(x => x.Id == positionApplication.Id);

            Invoice? updatedInvoice = factory.DbContextFactory.Context.Invoices
                .SingleOrDefault(x => x.Id == invoice.Id);

            Assert.NotNull(updatedPositionApplication);
            Assert.NotNull(updatedInvoice);

            Assert.Equal(withdrawnStatus.Id, updatedPositionApplication.PositionApplicationStatusId);
            Assert.Equal(cancelledInvoiceStatus.Id, updatedInvoice.InvoiceStatusId);

            factory.PositionApplicationRepository.Verify(x => x.Update(It.Is<PositionApplication>(application =>
                application.Id == positionApplication.Id)), Times.Once);

            factory.InvoiceRepository.Verify(x => x.Update(It.Is<Invoice>(storedInvoice =>
                storedInvoice.Id == invoice.Id)), Times.Once);

            factory.UnitOfWork.Verify(x => x.SaveChangesAsync(), Times.Once);

            factory.CacheService.Verify(x => x.RemoveByTag(CacheTags.PositionApplication, It.IsAny<CancellationToken>()), Times.Once);

            factory.DbContextFactory.Context.ChangeTracker.Clear();
        }

        [Fact]
        public async Task GetMyPositionApplications_ReturnsValidationError_WhenCurrentUserIsMissing()
        {
            using PositionApplicationServiceFactory factory = new PositionApplicationServiceFactory();

            factory.CurrentUserContext.Setup(x => x.UserId)
                .Returns((string?)null);

            PositionApplicationRequest request = new()
            {
                PageNumber = 1,
                PageSize = 10
            };

            Result<PagedResponse<PositionApplicationResponse>> result = await factory.Service.GetMyPositionApplications(request);

            Assert.Equal(ResultStatus.ValidationError, result.Status);
            Assert.Equal("The current user could not be identified.", result.Error);
            Assert.Null(result.Value);
        }

        [Fact]
        public async Task GetMyPositionApplications_ReturnsOnlyApplicationsBelongingToCurrentUser()
        {
            using PositionApplicationServiceFactory factory = new PositionApplicationServiceFactory();

            string currentUserId = Guid.NewGuid().ToString();
            string otherUserId = Guid.NewGuid().ToString();

            Student currentStudent = new()
            {
                Id = Guid.NewGuid(),
                UserId = currentUserId,
                Active = true
            };

            Student otherStudent = new()
            {
                Id = Guid.NewGuid(),
                UserId = otherUserId,
                Active = true
            };

            PositionApplicationStatus pendingPaymentStatus = new()
            {
                Id = 1,
                Code = ApplicationConstants.PositionApplicationStatuses.PendingPayment,
                Name = "Pending Payment",
                Active = true
            };

            Election currentElection = new()
            {
                Id = Guid.NewGuid().ToString(),
                Name = "2026 Engineering Election",
                ElectionTypeId = 1,
                YearId = 1
            };

            Election otherElection = new()
            {
                Id = Guid.NewGuid().ToString(),
                Name = "2026 Science Election",
                ElectionTypeId = 2,
                YearId = 1
            };

            Position currentPosition = new()
            {
                Id = Guid.NewGuid().ToString(),
                Name = "President",
                Active = true
            };

            Position otherPosition = new()
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Treasurer",
                Active = true
            };

            ElectionPosition currentElectionPosition = new()
            {
                Id = Guid.NewGuid().ToString(),
                ElectionId = currentElection.Id,
                PositionId = currentPosition.Id,
                ApplicationFee = 5000m,
                Currency = "NGN",
                Election = currentElection,
                Position = currentPosition
            };

            ElectionPosition otherElectionPosition = new()
            {
                Id = Guid.NewGuid().ToString(),
                ElectionId = otherElection.Id,
                PositionId = otherPosition.Id,
                ApplicationFee = 5000m,
                Currency = "NGN",
                Election = otherElection,
                Position = otherPosition
            };

            PositionApplication currentApplication = new()
            {
                Id = Guid.NewGuid().ToString(),
                StudentId = currentStudent.Id,
                ElectionPositionId = currentElectionPosition.Id,
                PositionApplicationStatusId = pendingPaymentStatus.Id,
                Student = currentStudent,
                ElectionPosition = currentElectionPosition,
                PositionApplicationStatus = pendingPaymentStatus,
                CreatedAt = DateTime.UtcNow
            };

            PositionApplication otherApplication = new()
            {
                Id = Guid.NewGuid().ToString(),
                StudentId = otherStudent.Id,
                ElectionPositionId = otherElectionPosition.Id,
                PositionApplicationStatusId = pendingPaymentStatus.Id,
                Student = otherStudent,
                ElectionPosition = otherElectionPosition,
                PositionApplicationStatus = pendingPaymentStatus,
                CreatedAt = DateTime.UtcNow.AddMinutes(-1)
            };

            factory.DbContextFactory.Context.Students.AddRange(currentStudent, otherStudent);
            factory.DbContextFactory.Context.PositionApplicationStatuses.Add(pendingPaymentStatus);
            factory.DbContextFactory.Context.Elections.AddRange(currentElection, otherElection);
            factory.DbContextFactory.Context.Positions.AddRange(currentPosition, otherPosition);
            factory.DbContextFactory.Context.ElectionPositions.AddRange(currentElectionPosition, otherElectionPosition);
            factory.DbContextFactory.Context.PositionApplications.AddRange(currentApplication, otherApplication);

            await factory.DbContextFactory.Context.SaveChangesAsync();

            factory.CurrentUserContext.Setup(x => x.UserId)
                .Returns(currentUserId);

            factory.Mapper.Setup(x => x.Map<PagedResponse<PositionApplicationResponse>>(It.IsAny<PagedList<PositionApplication>>()))
                .Returns((PagedList<PositionApplication> positionApplications) => new PagedResponse<PositionApplicationResponse>
                {
                    Items = positionApplications.Select(positionApplication => new PositionApplicationResponse
                    {
                        PositionApplicationId = positionApplication.Id,
                        ElectionPositionId = positionApplication.ElectionPositionId,
                        ElectionName = positionApplication.ElectionPosition.Election.Name,
                        PositionName = positionApplication.ElectionPosition.Position.Name,
                        ApplicationStatus = positionApplication.PositionApplicationStatus.Name,
                        CreatedAt = positionApplication.CreatedAt
                    }).ToList(),
                    MetaData = positionApplications.MetaData
                });

            PositionApplicationRequest request = new()
            {
                PageNumber = 1,
                PageSize = 10
            };

            Result<PagedResponse<PositionApplicationResponse>> result = await factory.Service.GetMyPositionApplications(request);

            Assert.Equal(ResultStatus.Success, result.Status);
            Assert.NotNull(result.Value);
            Assert.NotNull(result.Value.Items);
            Assert.Single(result.Value.Items);
            Assert.Equal(currentApplication.Id, result.Value.Items.Single().PositionApplicationId);
            Assert.Equal(1, result.Value.MetaData.TotalCount);
        }

        [Fact]
        public async Task GetMyPositionApplications_WithPositionApplicationStatusId_ShouldReturnFilteredApplications()
        {
            using PositionApplicationServiceFactory factory = new PositionApplicationServiceFactory();

            string userId = Guid.NewGuid().ToString();

            Student student = new()
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Active = true
            };

            PositionApplicationStatus pendingPaymentStatus = new()
            {
                Id = 1,
                Code = ApplicationConstants.PositionApplicationStatuses.PendingPayment,
                Name = "Pending Payment",
                Active = true
            };

            PositionApplicationStatus withdrawnStatus = new()
            {
                Id = 5,
                Code = ApplicationConstants.PositionApplicationStatuses.Withdrawn,
                Name = "Withdrawn",
                Active = true
            };

            Election election = new()
            {
                Id = Guid.NewGuid().ToString(),
                Name = "2026 Engineering Election"
            };

            Position firstPosition = new()
            {
                Id = Guid.NewGuid().ToString(),
                Name = "President",
                Active = true
            };

            Position secondPosition = new()
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Secretary",
                Active = true
            };

            ElectionPosition firstElectionPosition = new()
            {
                Id = Guid.NewGuid().ToString(),
                ElectionId = election.Id,
                PositionId = firstPosition.Id,
                ApplicationFee = 5000m,
                Currency = "NGN",
                Election = election,
                Position = firstPosition
            };

            ElectionPosition secondElectionPosition = new()
            {
                Id = Guid.NewGuid().ToString(),
                ElectionId = election.Id,
                PositionId = secondPosition.Id,
                ApplicationFee = 3000m,
                Currency = "NGN",
                Election = election,
                Position = secondPosition
            };

            PositionApplication pendingApplication = new()
            {
                Id = Guid.NewGuid().ToString(),
                StudentId = student.Id,
                ElectionPositionId = firstElectionPosition.Id,
                PositionApplicationStatusId = pendingPaymentStatus.Id,
                Student = student,
                ElectionPosition = firstElectionPosition,
                PositionApplicationStatus = pendingPaymentStatus,
                CreatedAt = DateTime.UtcNow
            };

            PositionApplication withdrawnApplication = new()
            {
                Id = Guid.NewGuid().ToString(),
                StudentId = student.Id,
                ElectionPositionId = secondElectionPosition.Id,
                PositionApplicationStatusId = withdrawnStatus.Id,
                Student = student,
                ElectionPosition = secondElectionPosition,
                PositionApplicationStatus = withdrawnStatus,
                CreatedAt = DateTime.UtcNow.AddMinutes(-1)
            };

            factory.DbContextFactory.Context.Students.Add(student);
            factory.DbContextFactory.Context.PositionApplicationStatuses.AddRange(pendingPaymentStatus, withdrawnStatus);
            factory.DbContextFactory.Context.Elections.Add(election);
            factory.DbContextFactory.Context.Positions.AddRange(firstPosition, secondPosition);
            factory.DbContextFactory.Context.ElectionPositions.AddRange(firstElectionPosition, secondElectionPosition);
            factory.DbContextFactory.Context.PositionApplications.AddRange(pendingApplication, withdrawnApplication);

            await factory.DbContextFactory.Context.SaveChangesAsync();

            factory.CurrentUserContext.Setup(x => x.UserId)
                .Returns(userId);

            factory.Mapper.Setup(x => x.Map<PagedResponse<PositionApplicationResponse>>(It.IsAny<PagedList<PositionApplication>>()))
                .Returns((PagedList<PositionApplication> positionApplications) => new PagedResponse<PositionApplicationResponse>
                {
                    Items = positionApplications.Select(positionApplication => new PositionApplicationResponse
                    {
                        PositionApplicationId = positionApplication.Id,
                        ElectionPositionId = positionApplication.ElectionPositionId,
                        ElectionName = positionApplication.ElectionPosition.Election.Name,
                        PositionName = positionApplication.ElectionPosition.Position.Name,
                        ApplicationStatus = positionApplication.PositionApplicationStatus.Name,
                        CreatedAt = positionApplication.CreatedAt
                    }).ToList(),
                    MetaData = positionApplications.MetaData
                });

            PositionApplicationRequest request = new()
            {
                PageNumber = 1,
                PageSize = 10,
                PositionApplicationStatusId = withdrawnStatus.Id
            };

            Result<PagedResponse<PositionApplicationResponse>> result = await factory.Service.GetMyPositionApplications(request);

            Assert.Equal(ResultStatus.Success, result.Status);
            Assert.NotNull(result.Value);
            Assert.NotNull(result.Value.Items);
            Assert.Single(result.Value.Items);
            Assert.Equal(withdrawnApplication.Id, result.Value.Items.Single().PositionApplicationId);
            Assert.Equal("Withdrawn", result.Value.Items.Single().ApplicationStatus);
            Assert.Equal(1, result.Value.MetaData.TotalCount);
        }

        [Fact]
        public async Task GetMyPositionApplications_WithSearchTerm_ShouldReturnMatchingApplications()
        {
            using PositionApplicationServiceFactory factory = new PositionApplicationServiceFactory();

            string userId = Guid.NewGuid().ToString();

            Student student = new()
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Active = true
            };

            PositionApplicationStatus pendingPaymentStatus = new()
            {
                Id = 1,
                Code = ApplicationConstants.PositionApplicationStatuses.PendingPayment,
                Name = "Pending Payment",
                Active = true
            };

            InvoiceStatus unpaidInvoiceStatus = new()
            {
                Id = 1,
                Code = ApplicationConstants.InvoiceStatuses.Unpaid,
                Name = "Unpaid",
                Active = true
            };

            Election engineeringElection = new()
            {
                Id = Guid.NewGuid().ToString(),
                Name = "2026 Engineering Election",
                ElectionTypeId = 1,
                YearId = 1
            };

            Election scienceElection = new()
            {
                Id = Guid.NewGuid().ToString(),
                Name = "2026 Science Election",
                ElectionTypeId = 2,
                YearId = 1
            };

            Position presidentPosition = new()
            {
                Id = Guid.NewGuid().ToString(),
                Name = "President",
                Active = true
            };

            Position treasurerPosition = new()
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Treasurer",
                Active = true
            };

            ElectionPosition engineeringElectionPosition = new()
            {
                Id = Guid.NewGuid().ToString(),
                ElectionId = engineeringElection.Id,
                PositionId = presidentPosition.Id,
                ApplicationFee = 5000m,
                Currency = "NGN",
                Election = engineeringElection,
                Position = presidentPosition
            };

            ElectionPosition scienceElectionPosition = new()
            {
                Id = Guid.NewGuid().ToString(),
                ElectionId = scienceElection.Id,
                PositionId = treasurerPosition.Id,
                ApplicationFee = 3000m,
                Currency = "NGN",
                Election = scienceElection,
                Position = treasurerPosition
            };

            PositionApplication matchingApplication = new()
            {
                Id = Guid.NewGuid().ToString(),
                StudentId = student.Id,
                ElectionPositionId = engineeringElectionPosition.Id,
                PositionApplicationStatusId = pendingPaymentStatus.Id,
                Student = student,
                ElectionPosition = engineeringElectionPosition,
                PositionApplicationStatus = pendingPaymentStatus,
                CreatedAt = DateTime.UtcNow
            };

            PositionApplication nonMatchingApplication = new()
            {
                Id = Guid.NewGuid().ToString(),
                StudentId = student.Id,
                ElectionPositionId = scienceElectionPosition.Id,
                PositionApplicationStatusId = pendingPaymentStatus.Id,
                Student = student,
                ElectionPosition = scienceElectionPosition,
                PositionApplicationStatus = pendingPaymentStatus,
                CreatedAt = DateTime.UtcNow.AddMinutes(-1)
            };

            Invoice matchingInvoice = new()
            {
                Id = Guid.NewGuid().ToString(),
                PositionApplicationId = matchingApplication.Id,
                InvoiceNumber = "INV-ENGINEERING-001",
                StudentId = student.Id,
                UserId = userId,
                PayerFirstName = "Test",
                PayerLastName = "Student",
                Amount = 5000m,
                Currency = "NGN",
                InvoiceStatusId = unpaidInvoiceStatus.Id,
                InvoiceStatus = unpaidInvoiceStatus,
                PositionApplication = matchingApplication
            };

            matchingApplication.Invoice = matchingInvoice;

            factory.DbContextFactory.Context.Students.Add(student);
            factory.DbContextFactory.Context.PositionApplicationStatuses.Add(pendingPaymentStatus);
            factory.DbContextFactory.Context.InvoiceStatuses.Add(unpaidInvoiceStatus);
            factory.DbContextFactory.Context.Elections.AddRange(engineeringElection, scienceElection);
            factory.DbContextFactory.Context.Positions.AddRange(presidentPosition, treasurerPosition);
            factory.DbContextFactory.Context.ElectionPositions.AddRange(engineeringElectionPosition, scienceElectionPosition);
            factory.DbContextFactory.Context.PositionApplications.AddRange(matchingApplication, nonMatchingApplication);
            factory.DbContextFactory.Context.Invoices.Add(matchingInvoice);

            await factory.DbContextFactory.Context.SaveChangesAsync();

            factory.CurrentUserContext.Setup(x => x.UserId)
                .Returns(userId);

            factory.Mapper.Setup(x => x.Map<PagedResponse<PositionApplicationResponse>>(It.IsAny<PagedList<PositionApplication>>()))
                .Returns((PagedList<PositionApplication> positionApplications) => new PagedResponse<PositionApplicationResponse>
                {
                    Items = positionApplications.Select(positionApplication => new PositionApplicationResponse
                    {
                        PositionApplicationId = positionApplication.Id,
                        ElectionPositionId = positionApplication.ElectionPositionId,
                        ElectionName = positionApplication.ElectionPosition.Election.Name,
                        PositionName = positionApplication.ElectionPosition.Position.Name,
                        ApplicationStatus = positionApplication.PositionApplicationStatus.Name,
                        InvoiceId = positionApplication.Invoice?.Id,
                        InvoiceNumber = positionApplication.Invoice?.InvoiceNumber,
                        Amount = positionApplication.Invoice?.Amount,
                        Currency = positionApplication.Invoice?.Currency,
                        InvoiceStatus = positionApplication.Invoice?.InvoiceStatus.Name,
                        CreatedAt = positionApplication.CreatedAt
                    }).ToList(),
                    MetaData = positionApplications.MetaData
                });

            PositionApplicationRequest request = new()
            {
                PageNumber = 1,
                PageSize = 10,
                SearchTerm = "Engineering"
            };

            Result<PagedResponse<PositionApplicationResponse>> result = await factory.Service.GetMyPositionApplications(request);

            Assert.Equal(ResultStatus.Success, result.Status);
            Assert.NotNull(result.Value);
            Assert.NotNull(result.Value.Items);
            Assert.Single(result.Value.Items);
            Assert.Equal(matchingApplication.Id, result.Value.Items.Single().PositionApplicationId);
            Assert.Equal("2026 Engineering Election", result.Value.Items.Single().ElectionName);
            Assert.Equal(1, result.Value.MetaData.TotalCount);
        }

        [Fact]
        public async Task GetPositionApplication_ReturnsForbidden_WhenActiveRoleIsMissing()
        {
            using PositionApplicationServiceFactory factory = new PositionApplicationServiceFactory();

            User user = PositionApplicationTestData.CreateUser();

            factory.CurrentUserContext.Setup(x => x.UserId).Returns(user.Id);

            factory.DbContextFactory.Context.Users.Add(user);
            await factory.DbContextFactory.Context.SaveChangesAsync();

            Result<PositionApplicationResponse> result =
                await factory.Service.GetPositionApplication(Guid.NewGuid().ToString());

            Assert.Equal(ResultStatus.Forbidden, result.Status);
        }

        [Fact]
        public async Task GetPositionApplication_ReturnsValidationError_WhenCurrentUserIsMissing()
        {
            using PositionApplicationServiceFactory factory = new PositionApplicationServiceFactory();

            factory.CurrentUserContext.Setup(x => x.UserId)
                .Returns((string?)null);

            Result<PositionApplicationResponse> result = await factory.Service.GetPositionApplication(Guid.NewGuid().ToString());

            Assert.Equal(ResultStatus.ValidationError, result.Status);
            Assert.Equal("The current user could not be identified.", result.Error);
            Assert.Null(result.Value);
        }

        [Fact]
        public async Task GetPositionApplication_ReturnsNotFound_WhenPositionApplicationBelongsToAnotherStudent()
        {
            using PositionApplicationServiceFactory factory = new PositionApplicationServiceFactory();

            User currentUser = PositionApplicationTestData.CreateUser();
            User otherUser = PositionApplicationTestData.CreateUser();

            Student currentStudent = PositionApplicationTestData.CreateStudent(currentUser);
            Student otherStudent = PositionApplicationTestData.CreateStudent(otherUser);

            Role studentRole = PositionApplicationTestData.CreateRole(ApplicationConstants.RoleNames.Student);
            ApplicationUserRole currentUserRole =
                PositionApplicationTestData.CreateApplicationUserRole(currentUser, studentRole);

            PositionApplicationStatus pendingPaymentStatus = new()
            {
                Id = 1,
                Code = ApplicationConstants.PositionApplicationStatuses.PendingPayment,
                Name = "Pending Payment",
                Active = true
            };

            InvoiceStatus unpaidInvoiceStatus = new()
            {
                Id = 1,
                Code = ApplicationConstants.InvoiceStatuses.Unpaid,
                Name = "Unpaid",
                Active = true
            };

            Election election = new()
            {
                Id = Guid.NewGuid().ToString(),
                Name = "2026 Engineering Election",
                ElectionTypeId = 1,
                YearId = 1
            };

            Position position = new()
            {
                Id = Guid.NewGuid().ToString(),
                Name = "President",
                Active = true
            };

            ElectionPosition electionPosition = new()
            {
                Id = Guid.NewGuid().ToString(),
                ElectionId = election.Id,
                PositionId = position.Id,
                ApplicationFee = 5000m,
                Currency = "NGN",
                Election = election,
                Position = position
            };

            PositionApplication positionApplication = new()
            {
                Id = Guid.NewGuid().ToString(),
                StudentId = otherStudent.Id,
                ElectionPositionId = electionPosition.Id,
                PositionApplicationStatusId = pendingPaymentStatus.Id,
                Student = otherStudent,
                ElectionPosition = electionPosition,
                PositionApplicationStatus = pendingPaymentStatus,
                CreatedAt = DateTime.UtcNow
            };

            Invoice invoice = new()
            {
                Id = Guid.NewGuid().ToString(),
                PositionApplicationId = positionApplication.Id,
                InvoiceNumber = $"INV-{Guid.NewGuid():N}",
                StudentId = otherStudent.Id,
                UserId = otherUser.Id,
                PayerFirstName = "Other",
                PayerLastName = "Student",
                Amount = 5000m,
                Currency = "NGN",
                InvoiceStatusId = unpaidInvoiceStatus.Id,
                InvoiceStatus = unpaidInvoiceStatus,
                PositionApplication = positionApplication
            };

            positionApplication.Invoice = invoice;

            factory.DbContextFactory.Context.Users.AddRange(currentUser, otherUser);
            factory.DbContextFactory.Context.Students.AddRange(currentStudent, otherStudent);

            factory.DbContextFactory.Context.Roles.Add(studentRole);
            factory.DbContextFactory.Context.UserRoles.Add(currentUserRole);

            factory.DbContextFactory.Context.PositionApplicationStatuses.Add(pendingPaymentStatus);
            factory.DbContextFactory.Context.InvoiceStatuses.Add(unpaidInvoiceStatus);
            factory.DbContextFactory.Context.Elections.Add(election);
            factory.DbContextFactory.Context.Positions.Add(position);
            factory.DbContextFactory.Context.ElectionPositions.Add(electionPosition);
            factory.DbContextFactory.Context.PositionApplications.Add(positionApplication);
            factory.DbContextFactory.Context.Invoices.Add(invoice);

            await factory.DbContextFactory.Context.SaveChangesAsync();

            factory.CurrentUserContext.Setup(x => x.UserId)
                .Returns(currentUser.Id);

            Result<PositionApplicationResponse> result =
                await factory.Service.GetPositionApplication(positionApplication.Id);

            Assert.Equal(ResultStatus.NotFound, result.Status);
        }

        [Fact]
        public async Task GetPositionApplication_ReturnsSuccess_WhenAdminRequestsAnotherStudentsApplication()
        {
            using PositionApplicationServiceFactory factory = new PositionApplicationServiceFactory();

            User adminUser = PositionApplicationTestData.CreateUser();
            User studentUser = PositionApplicationTestData.CreateUser();

            Student student = PositionApplicationTestData.CreateStudent(studentUser);

            Role adminRole = PositionApplicationTestData.CreateRole(ApplicationConstants.RoleNames.Admin);
            ApplicationUserRole adminUserRole =
                PositionApplicationTestData.CreateApplicationUserRole(adminUser, adminRole);

            PositionApplicationStatus pendingPaymentStatus = new()
            {
                Id = 1,
                Code = ApplicationConstants.PositionApplicationStatuses.PendingPayment,
                Name = "Pending Payment",
                Active = true
            };

            InvoiceStatus unpaidInvoiceStatus = new()
            {
                Id = 1,
                Code = ApplicationConstants.InvoiceStatuses.Unpaid,
                Name = "Unpaid",
                Active = true
            };

            Election election = new()
            {
                Id = Guid.NewGuid().ToString(),
                Name = "2026 Engineering Election",
                ElectionTypeId = 1,
                YearId = 1
            };

            Position position = new()
            {
                Id = Guid.NewGuid().ToString(),
                Name = "President",
                Active = true
            };

            ElectionPosition electionPosition = new()
            {
                Id = Guid.NewGuid().ToString(),
                ElectionId = election.Id,
                PositionId = position.Id,
                ApplicationFee = 5000m,
                Currency = "NGN",
                Election = election,
                Position = position
            };

            PositionApplication positionApplication = new()
            {
                Id = Guid.NewGuid().ToString(),
                StudentId = student.Id,
                ElectionPositionId = electionPosition.Id,
                PositionApplicationStatusId = pendingPaymentStatus.Id,
                Student = student,
                ElectionPosition = electionPosition,
                PositionApplicationStatus = pendingPaymentStatus,
                CreatedAt = DateTime.UtcNow
            };

            Invoice invoice = new()
            {
                Id = Guid.NewGuid().ToString(),
                PositionApplicationId = positionApplication.Id,
                InvoiceNumber = $"INV-{Guid.NewGuid():N}",
                StudentId = student.Id,
                UserId = studentUser.Id,
                PayerFirstName = "Test",
                PayerLastName = "Student",
                Amount = 5000m,
                Currency = "NGN",
                InvoiceStatusId = unpaidInvoiceStatus.Id,
                InvoiceStatus = unpaidInvoiceStatus,
                PositionApplication = positionApplication
            };

            positionApplication.Invoice = invoice;

            factory.DbContextFactory.Context.Users.AddRange(adminUser, studentUser);
            factory.DbContextFactory.Context.Students.Add(student);
            factory.DbContextFactory.Context.Roles.Add(adminRole);
            factory.DbContextFactory.Context.UserRoles.Add(adminUserRole);
            factory.DbContextFactory.Context.PositionApplicationStatuses.Add(pendingPaymentStatus);
            factory.DbContextFactory.Context.InvoiceStatuses.Add(unpaidInvoiceStatus);
            factory.DbContextFactory.Context.Elections.Add(election);
            factory.DbContextFactory.Context.Positions.Add(position);
            factory.DbContextFactory.Context.ElectionPositions.Add(electionPosition);
            factory.DbContextFactory.Context.PositionApplications.Add(positionApplication);
            factory.DbContextFactory.Context.Invoices.Add(invoice);

            await factory.DbContextFactory.Context.SaveChangesAsync();

            factory.CurrentUserContext.Setup(x => x.UserId)
                .Returns(adminUser.Id);

            factory.Mapper.Setup(x => x.Map<PositionApplicationResponse>(It.IsAny<PositionApplication>()))
                .Returns((PositionApplication application) => new PositionApplicationResponse
                {
                    PositionApplicationId = application.Id,
                    StudentId = application.StudentId,
                    RegistrationNumber = application.Student.RegNumber,
                    StudentName = application.Student.User is not null
                        ? $"{application.Student.User.FirstName} {application.Student.User.LastName}".Trim()
                        : string.Empty,
                    StudentEmail = application.Student.User?.Email,
                    ElectionPositionId = application.ElectionPositionId,
                    ElectionName = application.ElectionPosition.Election.Name,
                    PositionName = application.ElectionPosition.Position.Name,
                    ApplicationStatus = application.PositionApplicationStatus.Name,
                    InvoiceId = application.Invoice?.Id,
                    InvoiceNumber = application.Invoice?.InvoiceNumber,
                    Amount = application.Invoice?.Amount,
                    Currency = application.Invoice?.Currency,
                    InvoiceStatus = application.Invoice?.InvoiceStatus.Name,
                    CreatedAt = application.CreatedAt
                });

            Result<PositionApplicationResponse> result =
                await factory.Service.GetPositionApplication(positionApplication.Id);

            Assert.Equal(ResultStatus.Success, result.Status);
            Assert.NotNull(result.Value);
            Assert.Equal(positionApplication.Id, result.Value.PositionApplicationId);
            Assert.Equal(student.Id, result.Value.StudentId);
            Assert.Equal(student.RegNumber, result.Value.RegistrationNumber);
            Assert.Equal(studentUser.Email, result.Value.StudentEmail);
            Assert.Equal(electionPosition.Id, result.Value.ElectionPositionId);
            Assert.Equal(election.Name, result.Value.ElectionName);
            Assert.Equal(position.Name, result.Value.PositionName);
            Assert.Equal("Pending Payment", result.Value.ApplicationStatus);
            Assert.Equal(invoice.Id, result.Value.InvoiceId);
            Assert.Equal("Unpaid", result.Value.InvoiceStatus);
        }

        [Fact]
        public async Task GetPositionApplication_ReturnsSuccess_WhenSuperAdminRequestsAnotherStudentsApplication()
        {
            using PositionApplicationServiceFactory factory = new PositionApplicationServiceFactory();

            User superAdminUser = PositionApplicationTestData.CreateUser();
            User studentUser = PositionApplicationTestData.CreateUser();

            Student student = PositionApplicationTestData.CreateStudent(studentUser);

            Role superAdminRole = PositionApplicationTestData.CreateRole(ApplicationConstants.RoleNames.SuperAdmin);
            ApplicationUserRole superAdminUserRole =
                PositionApplicationTestData.CreateApplicationUserRole(superAdminUser, superAdminRole);

            PositionApplicationStatus pendingPaymentStatus = new()
            {
                Id = 1,
                Code = ApplicationConstants.PositionApplicationStatuses.PendingPayment,
                Name = "Pending Payment",
                Active = true
            };

            InvoiceStatus unpaidInvoiceStatus = new()
            {
                Id = 1,
                Code = ApplicationConstants.InvoiceStatuses.Unpaid,
                Name = "Unpaid",
                Active = true
            };

            Election election = new()
            {
                Id = Guid.NewGuid().ToString(),
                Name = "2026 Engineering Election",
                ElectionTypeId = 1,
                YearId = 1
            };

            Position position = new()
            {
                Id = Guid.NewGuid().ToString(),
                Name = "President",
                Active = true
            };

            ElectionPosition electionPosition = new()
            {
                Id = Guid.NewGuid().ToString(),
                ElectionId = election.Id,
                PositionId = position.Id,
                ApplicationFee = 5000m,
                Currency = "NGN",
                Election = election,
                Position = position
            };

            PositionApplication positionApplication = new()
            {
                Id = Guid.NewGuid().ToString(),
                StudentId = student.Id,
                ElectionPositionId = electionPosition.Id,
                PositionApplicationStatusId = pendingPaymentStatus.Id,
                Student = student,
                ElectionPosition = electionPosition,
                PositionApplicationStatus = pendingPaymentStatus,
                CreatedAt = DateTime.UtcNow
            };

            Invoice invoice = new()
            {
                Id = Guid.NewGuid().ToString(),
                PositionApplicationId = positionApplication.Id,
                InvoiceNumber = $"INV-{Guid.NewGuid():N}",
                StudentId = student.Id,
                UserId = studentUser.Id,
                PayerFirstName = "Test",
                PayerLastName = "Student",
                Amount = 5000m,
                Currency = "NGN",
                InvoiceStatusId = unpaidInvoiceStatus.Id,
                InvoiceStatus = unpaidInvoiceStatus,
                PositionApplication = positionApplication
            };

            positionApplication.Invoice = invoice;

            factory.DbContextFactory.Context.Users.AddRange(superAdminUser, studentUser);
            factory.DbContextFactory.Context.Students.Add(student);
            factory.DbContextFactory.Context.Roles.Add(superAdminRole);
            factory.DbContextFactory.Context.UserRoles.Add(superAdminUserRole);
            factory.DbContextFactory.Context.PositionApplicationStatuses.Add(pendingPaymentStatus);
            factory.DbContextFactory.Context.InvoiceStatuses.Add(unpaidInvoiceStatus);
            factory.DbContextFactory.Context.Elections.Add(election);
            factory.DbContextFactory.Context.Positions.Add(position);
            factory.DbContextFactory.Context.ElectionPositions.Add(electionPosition);
            factory.DbContextFactory.Context.PositionApplications.Add(positionApplication);
            factory.DbContextFactory.Context.Invoices.Add(invoice);

            await factory.DbContextFactory.Context.SaveChangesAsync();

            factory.CurrentUserContext.Setup(x => x.UserId)
                .Returns(superAdminUser.Id);

            factory.Mapper.Setup(x => x.Map<PositionApplicationResponse>(It.IsAny<PositionApplication>()))
                .Returns((PositionApplication application) => new PositionApplicationResponse
                {
                    PositionApplicationId = application.Id,
                    StudentId = application.StudentId,
                    RegistrationNumber = application.Student.RegNumber,
                    StudentName = application.Student.User is not null
                        ? $"{application.Student.User.FirstName} {application.Student.User.LastName}".Trim()
                        : string.Empty,
                    StudentEmail = application.Student.User?.Email,
                    ElectionPositionId = application.ElectionPositionId,
                    ElectionName = application.ElectionPosition.Election.Name,
                    PositionName = application.ElectionPosition.Position.Name,
                    ApplicationStatus = application.PositionApplicationStatus.Name,
                    InvoiceId = application.Invoice?.Id,
                    InvoiceNumber = application.Invoice?.InvoiceNumber,
                    Amount = application.Invoice?.Amount,
                    Currency = application.Invoice?.Currency,
                    InvoiceStatus = application.Invoice?.InvoiceStatus.Name,
                    CreatedAt = application.CreatedAt
                });

            Result<PositionApplicationResponse> result =
                await factory.Service.GetPositionApplication(positionApplication.Id);

            Assert.Equal(ResultStatus.Success, result.Status);
            Assert.NotNull(result.Value);
            Assert.Equal(positionApplication.Id, result.Value.PositionApplicationId);
            Assert.Equal(student.Id, result.Value.StudentId);
            Assert.Equal(student.RegNumber, result.Value.RegistrationNumber);
            Assert.Equal(studentUser.Email, result.Value.StudentEmail);
            Assert.Equal(electionPosition.Id, result.Value.ElectionPositionId);
            Assert.Equal(election.Name, result.Value.ElectionName);
            Assert.Equal(position.Name, result.Value.PositionName);
            Assert.Equal("Pending Payment", result.Value.ApplicationStatus);
            Assert.Equal(invoice.Id, result.Value.InvoiceId);
            Assert.Equal("Unpaid", result.Value.InvoiceStatus);
        }

        [Fact]
        public async Task GetPositionApplication_ReturnsSuccess_WhenPositionApplicationBelongsToCurrentUser()
        {
            using PositionApplicationServiceFactory factory = new PositionApplicationServiceFactory();

            string userId = Guid.NewGuid().ToString();

            User user = PositionApplicationTestData.CreateUser(userId);

            Student student = new()
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                User = user,
                Active = true
            };

            Role studentRole = PositionApplicationTestData.CreateRole(ApplicationConstants.RoleNames.Student);

            ApplicationUserRole studentUserRole =
                PositionApplicationTestData.CreateApplicationUserRole(user, studentRole);

            PositionApplicationStatus pendingPaymentStatus = new()
            {
                Id = 1,
                Code = ApplicationConstants.PositionApplicationStatuses.PendingPayment,
                Name = "Pending Payment",
                Active = true
            };

            InvoiceStatus unpaidInvoiceStatus = new()
            {
                Id = 1,
                Code = ApplicationConstants.InvoiceStatuses.Unpaid,
                Name = "Unpaid",
                Active = true
            };

            Election election = new()
            {
                Id = Guid.NewGuid().ToString(),
                Name = "2026 Engineering Election",
                ElectionTypeId = 1,
                YearId = 1
            };

            Position position = new()
            {
                Id = Guid.NewGuid().ToString(),
                Name = "President",
                Active = true
            };

            ElectionPosition electionPosition = new()
            {
                Id = Guid.NewGuid().ToString(),
                ElectionId = election.Id,
                PositionId = position.Id,
                ApplicationFee = 5000m,
                Currency = "NGN",
                Election = election,
                Position = position
            };

            PositionApplication positionApplication = new()
            {
                Id = Guid.NewGuid().ToString(),
                StudentId = student.Id,
                ElectionPositionId = electionPosition.Id,
                PositionApplicationStatusId = pendingPaymentStatus.Id,
                Student = student,
                ElectionPosition = electionPosition,
                PositionApplicationStatus = pendingPaymentStatus,
                CreatedAt = DateTime.UtcNow
            };

            Invoice invoice = new()
            {
                Id = Guid.NewGuid().ToString(),
                PositionApplicationId = positionApplication.Id,
                InvoiceNumber = $"INV-{Guid.NewGuid():N}",
                StudentId = student.Id,
                UserId = user.Id,
                PayerFirstName = "Test",
                PayerLastName = "Student",
                Amount = 5000m,
                Currency = "NGN",
                InvoiceStatusId = unpaidInvoiceStatus.Id,
                InvoiceStatus = unpaidInvoiceStatus,
                PositionApplication = positionApplication
            };

            positionApplication.Invoice = invoice;

            factory.DbContextFactory.Context.Users.Add(user);
            factory.DbContextFactory.Context.Students.Add(student);
            factory.DbContextFactory.Context.Roles.Add(studentRole);
            factory.DbContextFactory.Context.UserRoles.Add(studentUserRole);
            factory.DbContextFactory.Context.PositionApplicationStatuses.Add(pendingPaymentStatus);
            factory.DbContextFactory.Context.InvoiceStatuses.Add(unpaidInvoiceStatus);
            factory.DbContextFactory.Context.Elections.Add(election);
            factory.DbContextFactory.Context.Positions.Add(position);
            factory.DbContextFactory.Context.ElectionPositions.Add(electionPosition);
            factory.DbContextFactory.Context.PositionApplications.Add(positionApplication);
            factory.DbContextFactory.Context.Invoices.Add(invoice);

            await factory.DbContextFactory.Context.SaveChangesAsync();

            factory.CurrentUserContext.Setup(x => x.UserId)
                .Returns(user.Id);

            factory.Mapper.Setup(x => x.Map<PositionApplicationResponse>(It.IsAny<PositionApplication>()))
                .Returns((PositionApplication application) => new PositionApplicationResponse
                {
                    PositionApplicationId = application.Id,
                    StudentId = application.StudentId,
                    RegistrationNumber = application.Student.RegNumber,
                    StudentName = application.Student.User is not null
                        ? $"{application.Student.User.FirstName} {application.Student.User.LastName}".Trim()
                        : string.Empty,
                    StudentEmail = application.Student.User?.Email,
                    ElectionPositionId = application.ElectionPositionId,
                    ElectionName = application.ElectionPosition.Election.Name,
                    PositionName = application.ElectionPosition.Position.Name,
                    ApplicationStatus = application.PositionApplicationStatus.Name,
                    InvoiceId = application.Invoice?.Id,
                    InvoiceNumber = application.Invoice?.InvoiceNumber,
                    Amount = application.Invoice?.Amount,
                    Currency = application.Invoice?.Currency,
                    InvoiceStatus = application.Invoice?.InvoiceStatus.Name,
                    CreatedAt = application.CreatedAt
                });

            Result<PositionApplicationResponse> result =
                await factory.Service.GetPositionApplication(positionApplication.Id);

            Assert.Equal(ResultStatus.Success, result.Status);
            Assert.NotNull(result.Value);
            Assert.Equal(positionApplication.Id, result.Value.PositionApplicationId);
            Assert.Equal(student.Id, result.Value.StudentId);
            Assert.Equal(student.RegNumber, result.Value.RegistrationNumber);
            Assert.Equal($"{user.FirstName} {user.LastName}", result.Value.StudentName);
            Assert.Equal(user.Email, result.Value.StudentEmail);
            Assert.Equal(electionPosition.Id, result.Value.ElectionPositionId);
            Assert.Equal(election.Name, result.Value.ElectionName);
            Assert.Equal(position.Name, result.Value.PositionName);
            Assert.Equal("Pending Payment", result.Value.ApplicationStatus);
            Assert.Equal(invoice.Id, result.Value.InvoiceId);
            Assert.Equal(invoice.InvoiceNumber, result.Value.InvoiceNumber);
            Assert.Equal(invoice.Amount, result.Value.Amount);
            Assert.Equal(invoice.Currency, result.Value.Currency);
            Assert.Equal("Unpaid", result.Value.InvoiceStatus);
        }

        [Fact]
        public async Task GetPositionApplications_ReturnsApplicationsForAllStudents()
        {
            using PositionApplicationServiceFactory factory = new PositionApplicationServiceFactory();

            User firstUser = PositionApplicationTestData.CreateUser();
            User secondUser = PositionApplicationTestData.CreateUser();

            Student firstStudent = PositionApplicationTestData.CreateStudent(firstUser);
            Student secondStudent = PositionApplicationTestData.CreateStudent(secondUser);

            PositionApplicationStatus pendingPaymentStatus = new()
            {
                Id = 1,
                Code = ApplicationConstants.PositionApplicationStatuses.PendingPayment,
                Name = "Pending Payment",
                Active = true
            };

            Election firstElection = new()
            {
                Id = Guid.NewGuid().ToString(),
                Name = "2026 Engineering Election",
                ElectionTypeId = 1,
                YearId = 1
            };

            Election secondElection = new()
            {
                Id = Guid.NewGuid().ToString(),
                Name = "2026 Science Election",
                ElectionTypeId = 2,
                YearId = 1
            };

            Position firstPosition = new()
            {
                Id = Guid.NewGuid().ToString(),
                Name = "President",
                Active = true
            };

            Position secondPosition = new()
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Treasurer",
                Active = true
            };

            ElectionPosition firstElectionPosition = new()
            {
                Id = Guid.NewGuid().ToString(),
                ElectionId = firstElection.Id,
                PositionId = firstPosition.Id,
                Election = firstElection,
                Position = firstPosition
            };

            ElectionPosition secondElectionPosition = new()
            {
                Id = Guid.NewGuid().ToString(),
                ElectionId = secondElection.Id,
                PositionId = secondPosition.Id,
                Election = secondElection,
                Position = secondPosition
            };

            PositionApplication firstApplication = new()
            {
                Id = Guid.NewGuid().ToString(),
                StudentId = firstStudent.Id,
                ElectionPositionId = firstElectionPosition.Id,
                PositionApplicationStatusId = pendingPaymentStatus.Id,
                Student = firstStudent,
                ElectionPosition = firstElectionPosition,
                PositionApplicationStatus = pendingPaymentStatus,
                CreatedAt = DateTime.UtcNow
            };

            PositionApplication secondApplication = new()
            {
                Id = Guid.NewGuid().ToString(),
                StudentId = secondStudent.Id,
                ElectionPositionId = secondElectionPosition.Id,
                PositionApplicationStatusId = pendingPaymentStatus.Id,
                Student = secondStudent,
                ElectionPosition = secondElectionPosition,
                PositionApplicationStatus = pendingPaymentStatus,
                CreatedAt = DateTime.UtcNow.AddMinutes(-1)
            };

            factory.DbContextFactory.Context.Users.AddRange(firstUser, secondUser);
            factory.DbContextFactory.Context.Students.AddRange(firstStudent, secondStudent);
            factory.DbContextFactory.Context.PositionApplicationStatuses.Add(pendingPaymentStatus);
            factory.DbContextFactory.Context.Elections.AddRange(firstElection, secondElection);
            factory.DbContextFactory.Context.Positions.AddRange(firstPosition, secondPosition);
            factory.DbContextFactory.Context.ElectionPositions.AddRange(firstElectionPosition, secondElectionPosition);
            factory.DbContextFactory.Context.PositionApplications.AddRange(firstApplication, secondApplication);

            await factory.DbContextFactory.Context.SaveChangesAsync();

            factory.Mapper.Setup(x => x.Map<PagedResponse<PositionApplicationResponse>>(It.IsAny<PagedList<PositionApplication>>()))
                .Returns((PagedList<PositionApplication> positionApplications) => new PagedResponse<PositionApplicationResponse>
                {
                    Items = positionApplications.Select(positionApplication => new PositionApplicationResponse
                    {
                        PositionApplicationId = positionApplication.Id,
                        StudentId = positionApplication.StudentId,
                        RegistrationNumber = positionApplication.Student.RegNumber,
                        StudentName = $"{positionApplication.Student.User?.FirstName} {positionApplication.Student.User?.LastName}".Trim(),
                        StudentEmail = positionApplication.Student.User?.Email,
                        ElectionPositionId = positionApplication.ElectionPositionId,
                        ElectionName = positionApplication.ElectionPosition.Election.Name,
                        PositionName = positionApplication.ElectionPosition.Position.Name,
                        ApplicationStatus = positionApplication.PositionApplicationStatus.Name,
                        CreatedAt = positionApplication.CreatedAt
                    }).ToList(),
                    MetaData = positionApplications.MetaData
                });

            PositionApplicationRequest request = new()
            {
                PageNumber = 1,
                PageSize = 10
            };

            Result<PagedResponse<PositionApplicationResponse>> result = await factory.Service.GetPositionApplications(request);

            Assert.Equal(ResultStatus.Success, result.Status);
            Assert.NotNull(result.Value);
            Assert.NotNull(result.Value.Items);
            Assert.Equal(2, result.Value.Items.Count());
            Assert.Equal(2, result.Value.MetaData.TotalCount);

            Assert.Contains(result.Value.Items, x => x.PositionApplicationId == firstApplication.Id);
            Assert.Contains(result.Value.Items, x => x.PositionApplicationId == secondApplication.Id);
        }

        [Fact]
        public async Task GetPositionApplications_WithPositionApplicationStatusId_ShouldReturnFilteredApplications()
        {
            using PositionApplicationServiceFactory factory = new PositionApplicationServiceFactory();

            User user = PositionApplicationTestData.CreateUser();
            Student student = PositionApplicationTestData.CreateStudent(user);

            PositionApplicationStatus pendingPaymentStatus = new()
            {
                Id = 1,
                Code = ApplicationConstants.PositionApplicationStatuses.PendingPayment,
                Name = "Pending Payment",
                Active = true
            };

            PositionApplicationStatus pendingReviewStatus = new()
            {
                Id = 2,
                Code = ApplicationConstants.PositionApplicationStatuses.PendingReview,
                Name = "Pending Review",
                Active = true
            };

            Election election = new()
            {
                Id = Guid.NewGuid().ToString(),
                Name = "2026 Engineering Election",
                ElectionTypeId = 1,
                YearId = 1
            };

            Position firstPosition = new()
            {
                Id = Guid.NewGuid().ToString(),
                Name = "President",
                Active = true
            };

            Position secondPosition = new()
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Secretary",
                Active = true
            };

            ElectionPosition firstElectionPosition = new()
            {
                Id = Guid.NewGuid().ToString(),
                ElectionId = election.Id,
                PositionId = firstPosition.Id,
                Election = election,
                Position = firstPosition
            };

            ElectionPosition secondElectionPosition = new()
            {
                Id = Guid.NewGuid().ToString(),
                ElectionId = election.Id,
                PositionId = secondPosition.Id,
                Election = election,
                Position = secondPosition
            };

            PositionApplication pendingPaymentApplication = new()
            {
                Id = Guid.NewGuid().ToString(),
                StudentId = student.Id,
                ElectionPositionId = firstElectionPosition.Id,
                PositionApplicationStatusId = pendingPaymentStatus.Id,
                Student = student,
                ElectionPosition = firstElectionPosition,
                PositionApplicationStatus = pendingPaymentStatus,
                CreatedAt = DateTime.UtcNow
            };

            PositionApplication pendingReviewApplication = new()
            {
                Id = Guid.NewGuid().ToString(),
                StudentId = student.Id,
                ElectionPositionId = secondElectionPosition.Id,
                PositionApplicationStatusId = pendingReviewStatus.Id,
                Student = student,
                ElectionPosition = secondElectionPosition,
                PositionApplicationStatus = pendingReviewStatus,
                CreatedAt = DateTime.UtcNow.AddMinutes(-1)
            };

            factory.DbContextFactory.Context.Users.Add(user);
            factory.DbContextFactory.Context.Students.Add(student);
            factory.DbContextFactory.Context.PositionApplicationStatuses.AddRange(pendingPaymentStatus, pendingReviewStatus);
            factory.DbContextFactory.Context.Elections.Add(election);
            factory.DbContextFactory.Context.Positions.AddRange(firstPosition, secondPosition);
            factory.DbContextFactory.Context.ElectionPositions.AddRange(firstElectionPosition, secondElectionPosition);
            factory.DbContextFactory.Context.PositionApplications.AddRange(pendingPaymentApplication, pendingReviewApplication);

            await factory.DbContextFactory.Context.SaveChangesAsync();

            factory.Mapper.Setup(x => x.Map<PagedResponse<PositionApplicationResponse>>(It.IsAny<PagedList<PositionApplication>>()))
                .Returns((PagedList<PositionApplication> positionApplications) => new PagedResponse<PositionApplicationResponse>
                {
                    Items = positionApplications.Select(positionApplication => new PositionApplicationResponse
                    {
                        PositionApplicationId = positionApplication.Id,
                        ApplicationStatus = positionApplication.PositionApplicationStatus.Name
                    }).ToList(),
                    MetaData = positionApplications.MetaData
                });

            PositionApplicationRequest request = new()
            {
                PageNumber = 1,
                PageSize = 10,
                PositionApplicationStatusId = pendingReviewStatus.Id
            };

            Result<PagedResponse<PositionApplicationResponse>> result = await factory.Service.GetPositionApplications(request);

            Assert.Equal(ResultStatus.Success, result.Status);
            Assert.NotNull(result.Value);
            Assert.Single(result.Value.Items);
            Assert.Equal(pendingReviewApplication.Id, result.Value.Items.Single().PositionApplicationId);
            Assert.Equal("Pending Review", result.Value.Items.Single().ApplicationStatus);
            Assert.Equal(1, result.Value.MetaData.TotalCount);
        }

        [Fact]
        public async Task GetPositionApplications_WithSearchTerm_ShouldReturnMatchingStudentApplication()
        {
            using PositionApplicationServiceFactory factory = new PositionApplicationServiceFactory();

            User matchingUser = PositionApplicationTestData.CreateUser();
            matchingUser.FirstName = "Obinna";
            matchingUser.LastName = "Chike";
            matchingUser.Email = "obinna@example.com";

            User nonMatchingUser = PositionApplicationTestData.CreateUser();
            nonMatchingUser.FirstName = "John";
            nonMatchingUser.LastName = "Doe";
            nonMatchingUser.Email = "john@example.com";

            Student matchingStudent = PositionApplicationTestData.CreateStudent(matchingUser);
            matchingStudent.RegNumber = "REG-OBINNA-001";

            Student nonMatchingStudent = PositionApplicationTestData.CreateStudent(nonMatchingUser);
            nonMatchingStudent.RegNumber = "REG-JOHN-001";

            PositionApplicationStatus pendingPaymentStatus = new()
            {
                Id = 1,
                Code = ApplicationConstants.PositionApplicationStatuses.PendingPayment,
                Name = "Pending Payment",
                Active = true
            };

            Election firstElection = new()
            {
                Id = Guid.NewGuid().ToString(),
                Name = "2026 Engineering Election",
                ElectionTypeId = 1,
                YearId = 1
            };

            Election secondElection = new()
            {
                Id = Guid.NewGuid().ToString(),
                Name = "2026 Science Election",
                ElectionTypeId = 2,
                YearId = 1
            };

            Position firstPosition = new()
            {
                Id = Guid.NewGuid().ToString(),
                Name = "President",
                Active = true
            };

            Position secondPosition = new()
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Treasurer",
                Active = true
            };

            ElectionPosition firstElectionPosition = new()
            {
                Id = Guid.NewGuid().ToString(),
                ElectionId = firstElection.Id,
                PositionId = firstPosition.Id,
                Election = firstElection,
                Position = firstPosition
            };

            ElectionPosition secondElectionPosition = new()
            {
                Id = Guid.NewGuid().ToString(),
                ElectionId = secondElection.Id,
                PositionId = secondPosition.Id,
                Election = secondElection,
                Position = secondPosition
            };

            PositionApplication matchingApplication = new()
            {
                Id = Guid.NewGuid().ToString(),
                StudentId = matchingStudent.Id,
                ElectionPositionId = firstElectionPosition.Id,
                PositionApplicationStatusId = pendingPaymentStatus.Id,
                Student = matchingStudent,
                ElectionPosition = firstElectionPosition,
                PositionApplicationStatus = pendingPaymentStatus,
                CreatedAt = DateTime.UtcNow
            };

            PositionApplication nonMatchingApplication = new()
            {
                Id = Guid.NewGuid().ToString(),
                StudentId = nonMatchingStudent.Id,
                ElectionPositionId = secondElectionPosition.Id,
                PositionApplicationStatusId = pendingPaymentStatus.Id,
                Student = nonMatchingStudent,
                ElectionPosition = secondElectionPosition,
                PositionApplicationStatus = pendingPaymentStatus,
                CreatedAt = DateTime.UtcNow.AddMinutes(-1)
            };

            factory.DbContextFactory.Context.Users.AddRange(matchingUser, nonMatchingUser);
            factory.DbContextFactory.Context.Students.AddRange(matchingStudent, nonMatchingStudent);
            factory.DbContextFactory.Context.PositionApplicationStatuses.Add(pendingPaymentStatus);
            factory.DbContextFactory.Context.Elections.AddRange(firstElection, secondElection);
            factory.DbContextFactory.Context.Positions.AddRange(firstPosition, secondPosition);
            factory.DbContextFactory.Context.ElectionPositions.AddRange(firstElectionPosition, secondElectionPosition);
            factory.DbContextFactory.Context.PositionApplications.AddRange(matchingApplication, nonMatchingApplication);

            await factory.DbContextFactory.Context.SaveChangesAsync();

            factory.Mapper.Setup(x => x.Map<PagedResponse<PositionApplicationResponse>>(It.IsAny<PagedList<PositionApplication>>()))
                .Returns((PagedList<PositionApplication> positionApplications) => new PagedResponse<PositionApplicationResponse>
                {
                    Items = positionApplications.Select(positionApplication => new PositionApplicationResponse
                    {
                        PositionApplicationId = positionApplication.Id,
                        StudentId = positionApplication.StudentId,
                        RegistrationNumber = positionApplication.Student.RegNumber,
                        StudentName = $"{positionApplication.Student.User?.FirstName} {positionApplication.Student.User?.LastName}".Trim(),
                        StudentEmail = positionApplication.Student.User?.Email
                    }).ToList(),
                    MetaData = positionApplications.MetaData
                });

            PositionApplicationRequest request = new()
            {
                PageNumber = 1,
                PageSize = 10,
                SearchTerm = "Obinna"
            };

            Result<PagedResponse<PositionApplicationResponse>> result = await factory.Service.GetPositionApplications(request);

            Assert.Equal(ResultStatus.Success, result.Status);
            Assert.NotNull(result.Value);
            Assert.Single(result.Value.Items);
            Assert.Equal(matchingApplication.Id, result.Value.Items.Single().PositionApplicationId);
            Assert.Equal(matchingStudent.Id, result.Value.Items.Single().StudentId);
            Assert.Equal("Obinna Chike", result.Value.Items.Single().StudentName);
            Assert.Equal(1, result.Value.MetaData.TotalCount);
        }

        [Fact]
        public async Task ApproveOrRejectPositionApplication_ApprovesApplicationAndCreatesContestant()
        {
            using PositionApplicationServiceFactory factory = new PositionApplicationServiceFactory();

            User adminUser = PositionApplicationTestData.CreateUser();

            Role adminRole = PositionApplicationTestData.CreateRole(ApplicationConstants.RoleNames.Admin);
            ApplicationUserRole adminUserRole =
                PositionApplicationTestData.CreateApplicationUserRole(adminUser, adminRole);

            User studentUser = PositionApplicationTestData.CreateUser();
            Student student = PositionApplicationTestData.CreateStudent(studentUser);

            PositionApplicationStatus pendingReviewStatus = new()
            {
                Id = 2,
                Code = ApplicationConstants.PositionApplicationStatuses.PendingReview,
                Name = "Pending Review",
                Active = true
            };

            PositionApplicationStatus approvedStatus = new()
            {
                Id = 3,
                Code = ApplicationConstants.PositionApplicationStatuses.Approved,
                Name = "Approved",
                Active = true
            };

            InvoiceStatus paidInvoiceStatus = new()
            {
                Id = 2,
                Code = ApplicationConstants.InvoiceStatuses.Paid,
                Name = "Paid",
                Active = true
            };

            Election election = new()
            {
                Id = Guid.NewGuid().ToString(),
                Name = "2026 Engineering Election",
                ElectionTypeId = 1,
                YearId = 1
            };

            Position position = new()
            {
                Id = Guid.NewGuid().ToString(),
                Name = "President",
                Active = true
            };

            ElectionPosition electionPosition = new()
            {
                Id = Guid.NewGuid().ToString(),
                ElectionId = election.Id,
                PositionId = position.Id,
                ApplicationFee = 5000m,
                Currency = "NGN",
                Election = election,
                Position = position
            };

            PositionApplication positionApplication = new()
            {
                Id = Guid.NewGuid().ToString(),
                StudentId = student.Id,
                ElectionPositionId = electionPosition.Id,
                PositionApplicationStatusId = pendingReviewStatus.Id,
                Student = student,
                ElectionPosition = electionPosition,
                PositionApplicationStatus = pendingReviewStatus,
                Active = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            Invoice invoice = new()
            {
                Id = Guid.NewGuid().ToString(),
                PositionApplicationId = positionApplication.Id,
                InvoiceNumber = $"INV-{Guid.NewGuid():N}",
                StudentId = student.Id,
                UserId = studentUser.Id,
                PayerFirstName = studentUser.FirstName,
                PayerLastName = studentUser.LastName,
                PayerEmail = studentUser.Email,
                Amount = 5000m,
                Currency = "NGN",
                InvoiceStatusId = paidInvoiceStatus.Id,
                InvoiceStatus = paidInvoiceStatus,
                PositionApplication = positionApplication,
                PaidAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            positionApplication.Invoice = invoice;

            factory.DbContextFactory.Context.Users.AddRange(adminUser, studentUser);
            factory.DbContextFactory.Context.Students.Add(student);
            factory.DbContextFactory.Context.Roles.Add(adminRole);
            factory.DbContextFactory.Context.UserRoles.Add(adminUserRole);
            factory.DbContextFactory.Context.PositionApplicationStatuses.AddRange(pendingReviewStatus, approvedStatus);
            factory.DbContextFactory.Context.InvoiceStatuses.Add(paidInvoiceStatus);
            factory.DbContextFactory.Context.Elections.Add(election);
            factory.DbContextFactory.Context.Positions.Add(position);
            factory.DbContextFactory.Context.ElectionPositions.Add(electionPosition);
            factory.DbContextFactory.Context.PositionApplications.Add(positionApplication);
            factory.DbContextFactory.Context.Invoices.Add(invoice);

            await factory.DbContextFactory.Context.SaveChangesAsync();

            factory.CurrentUserContext.Setup(x => x.UserId)
                .Returns(adminUser.Id);

            ApproveOrRejectPositionApplicationRequest request = new()
            {
                PositionApplicationId = positionApplication.Id,
                PositionApplicationStatusId = approvedStatus.Id
            };

            Result<string> result =
                await factory.Service.ApproveOrRejectPositionApplication(request);

            Assert.Equal(ResultStatus.Success, result.Status);
            Assert.Equal("Position application approved successfully.", result.Value);

            factory.PositionApplicationRepository.Verify(x => x.Update(It.IsAny<PositionApplication>()), Times.Once);

            factory.ContestantRepository.Verify(x => x.Add(It.IsAny<Contestant>()),
                Times.Once);

            factory.UnitOfWork.Verify(x => x.BeginTransactionAsync(), Times.Once);

            factory.UnitOfWork.Verify(x => x.SaveChangesAsync(), Times.Once);

            factory.UnitOfWork.Verify(x => x.CommitTransactionAsync(), Times.Once);

            factory.UnitOfWork.Verify(x => x.RollbackTransactionAsync(), Times.Never);

            factory.CacheService.Verify(x => x.RemoveByTag(CacheTags.PositionApplication, It.IsAny<CancellationToken>()), Times.Once);

            factory.DbContextFactory.Context.ChangeTracker.Clear();

            PositionApplication? updatedPositionApplication = await factory.DbContextFactory.Context.PositionApplications.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == positionApplication.Id);

            Contestant? createdContestant = await factory.DbContextFactory.Context.Contestants.AsNoTracking()
                .FirstOrDefaultAsync(x => x.PositionApplicationId == positionApplication.Id);

            Assert.NotNull(updatedPositionApplication);
            Assert.Equal(approvedStatus.Id, updatedPositionApplication.PositionApplicationStatusId);

            Assert.NotNull(createdContestant);
            Assert.Equal(positionApplication.Id, createdContestant.PositionApplicationId);
            Assert.True(createdContestant.Active);
        }

        [Fact]
        public async Task ApproveOrRejectPositionApplication_RejectsApplicationWithoutCreatingContestant()
        {
            using PositionApplicationServiceFactory factory = new PositionApplicationServiceFactory();

            User adminUser = PositionApplicationTestData.CreateUser();

            Role adminRole = PositionApplicationTestData.CreateRole(ApplicationConstants.RoleNames.Admin);
            ApplicationUserRole adminUserRole =
                PositionApplicationTestData.CreateApplicationUserRole(adminUser, adminRole);

            User studentUser = PositionApplicationTestData.CreateUser();
            Student student = PositionApplicationTestData.CreateStudent(studentUser);

            PositionApplicationStatus pendingReviewStatus = new()
            {
                Id = 2,
                Code = ApplicationConstants.PositionApplicationStatuses.PendingReview,
                Name = "Pending Review",
                Active = true
            };

            PositionApplicationStatus rejectedStatus = new()
            {
                Id = 4,
                Code = ApplicationConstants.PositionApplicationStatuses.Rejected,
                Name = "Rejected",
                Active = true
            };

            InvoiceStatus paidInvoiceStatus = new()
            {
                Id = 2,
                Code = ApplicationConstants.InvoiceStatuses.Paid,
                Name = "Paid",
                Active = true
            };

            Election election = new()
            {
                Id = Guid.NewGuid().ToString(),
                Name = "2026 Engineering Election",
                ElectionTypeId = 1,
                YearId = 1
            };

            Position position = new()
            {
                Id = Guid.NewGuid().ToString(),
                Name = "President",
                Active = true
            };

            ElectionPosition electionPosition = new()
            {
                Id = Guid.NewGuid().ToString(),
                ElectionId = election.Id,
                PositionId = position.Id,
                ApplicationFee = 5000m,
                Currency = "NGN",
                Election = election,
                Position = position
            };

            PositionApplication positionApplication = new()
            {
                Id = Guid.NewGuid().ToString(),
                StudentId = student.Id,
                ElectionPositionId = electionPosition.Id,
                PositionApplicationStatusId = pendingReviewStatus.Id,
                Student = student,
                ElectionPosition = electionPosition,
                PositionApplicationStatus = pendingReviewStatus,
                Active = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            Invoice invoice = new()
            {
                Id = Guid.NewGuid().ToString(),
                PositionApplicationId = positionApplication.Id,
                InvoiceNumber = $"INV-{Guid.NewGuid():N}",
                StudentId = student.Id,
                UserId = studentUser.Id,
                PayerFirstName = studentUser.FirstName,
                PayerLastName = studentUser.LastName,
                PayerEmail = studentUser.Email,
                Amount = 5000m,
                Currency = "NGN",
                InvoiceStatusId = paidInvoiceStatus.Id,
                InvoiceStatus = paidInvoiceStatus,
                PositionApplication = positionApplication,
                PaidAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            positionApplication.Invoice = invoice;

            factory.DbContextFactory.Context.Users.AddRange(adminUser, studentUser);
            factory.DbContextFactory.Context.Students.Add(student);
            factory.DbContextFactory.Context.Roles.Add(adminRole);
            factory.DbContextFactory.Context.UserRoles.Add(adminUserRole);
            factory.DbContextFactory.Context.PositionApplicationStatuses.AddRange(pendingReviewStatus, rejectedStatus);
            factory.DbContextFactory.Context.InvoiceStatuses.Add(paidInvoiceStatus);
            factory.DbContextFactory.Context.Elections.Add(election);
            factory.DbContextFactory.Context.Positions.Add(position);
            factory.DbContextFactory.Context.ElectionPositions.Add(electionPosition);
            factory.DbContextFactory.Context.PositionApplications.Add(positionApplication);
            factory.DbContextFactory.Context.Invoices.Add(invoice);

            await factory.DbContextFactory.Context.SaveChangesAsync();

            factory.CurrentUserContext.Setup(x => x.UserId)
                .Returns(adminUser.Id);

            ApproveOrRejectPositionApplicationRequest request = new()
            {
                PositionApplicationId = positionApplication.Id,
                PositionApplicationStatusId = rejectedStatus.Id
            };

            Result<string> result = await factory.Service.ApproveOrRejectPositionApplication(request);

            Assert.Equal(ResultStatus.Success, result.Status);
            Assert.Equal("Position application rejected successfully.", result.Value);

            factory.PositionApplicationRepository.Verify(x => x.Update(It.IsAny<PositionApplication>()), Times.Once);
            factory.ContestantRepository.Verify(x => x.Add(It.IsAny<Contestant>()), Times.Never);

            factory.UnitOfWork.Verify(x => x.BeginTransactionAsync(), Times.Once);
            factory.UnitOfWork.Verify(x => x.SaveChangesAsync(), Times.Once);
            factory.UnitOfWork.Verify(x => x.CommitTransactionAsync(), Times.Once);
            factory.UnitOfWork.Verify(x => x.RollbackTransactionAsync(), Times.Never);

            factory.CacheService.Verify(x => x.RemoveByTag(CacheTags.PositionApplication, It.IsAny<CancellationToken>()), Times.Once);

            factory.DbContextFactory.Context.ChangeTracker.Clear();

            PositionApplication? updatedPositionApplication = await factory.DbContextFactory.Context.PositionApplications.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == positionApplication.Id);

            Contestant? contestant = await factory.DbContextFactory.Context.Contestants.AsNoTracking()
                .FirstOrDefaultAsync(x => x.PositionApplicationId == positionApplication.Id);

            Assert.NotNull(updatedPositionApplication);
            Assert.Equal(rejectedStatus.Id, updatedPositionApplication.PositionApplicationStatusId);
            Assert.Null(contestant);
        }

        [Fact]
        public async Task ApproveOrRejectPositionApplication_ReturnsForbidden_WhenUserIsStudent()
        {
            using PositionApplicationServiceFactory factory = new PositionApplicationServiceFactory();

            User studentUser = PositionApplicationTestData.CreateUser();
            Role studentRole = PositionApplicationTestData.CreateRole(ApplicationConstants.RoleNames.Student);
            ApplicationUserRole studentUserRole = PositionApplicationTestData.CreateApplicationUserRole(studentUser, studentRole);

            factory.DbContextFactory.Context.Users.Add(studentUser);
            factory.DbContextFactory.Context.Roles.Add(studentRole);
            factory.DbContextFactory.Context.UserRoles.Add(studentUserRole);

            await factory.DbContextFactory.Context.SaveChangesAsync();

            factory.CurrentUserContext.Setup(x => x.UserId)
                .Returns(studentUser.Id);

            ApproveOrRejectPositionApplicationRequest request = new()
            {
                PositionApplicationId = Guid.NewGuid().ToString(),
                PositionApplicationStatusId = 3
            };

            Result<string> result = await factory.Service.ApproveOrRejectPositionApplication(request);

            Assert.Equal(ResultStatus.Forbidden, result.Status);

            factory.UnitOfWork.Verify(x => x.BeginTransactionAsync(), Times.Never);
            factory.UnitOfWork.Verify(x => x.CommitTransactionAsync(), Times.Never);
            factory.CacheService.Verify(x => x.RemoveByTag(CacheTags.PositionApplication, It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task ApproveOrRejectPositionApplication_ReturnsValidationError_WhenRequestedStatusIsInvalid()
        {
            using PositionApplicationServiceFactory factory = new PositionApplicationServiceFactory();

            User adminUser = PositionApplicationTestData.CreateUser();
            Role adminRole = PositionApplicationTestData.CreateRole(ApplicationConstants.RoleNames.Admin);
            ApplicationUserRole adminUserRole = PositionApplicationTestData.CreateApplicationUserRole(adminUser, adminRole);

            PositionApplicationStatus invalidStatus = new()
            {
                Id = 1,
                Code = ApplicationConstants.PositionApplicationStatuses.PendingPayment,
                Name = "Pending Payment",
                Active = true
            };

            factory.DbContextFactory.Context.Users.Add(adminUser);
            factory.DbContextFactory.Context.Roles.Add(adminRole);
            factory.DbContextFactory.Context.UserRoles.Add(adminUserRole);
            factory.DbContextFactory.Context.PositionApplicationStatuses.Add(invalidStatus);

            await factory.DbContextFactory.Context.SaveChangesAsync();

            factory.CurrentUserContext.Setup(x => x.UserId)
                .Returns(adminUser.Id);

            ApproveOrRejectPositionApplicationRequest request = new()
            {
                PositionApplicationId = Guid.NewGuid().ToString(),
                PositionApplicationStatusId = invalidStatus.Id
            };

            Result<string> result = await factory.Service.ApproveOrRejectPositionApplication(request);

            Assert.Equal(ResultStatus.ValidationError, result.Status);

            factory.UnitOfWork.Verify(x => x.BeginTransactionAsync(), Times.Once);
            factory.UnitOfWork.Verify(x => x.RollbackTransactionAsync(), Times.Once);
            factory.UnitOfWork.Verify(x => x.CommitTransactionAsync(), Times.Never);
            factory.CacheService.Verify(x => x.RemoveByTag(CacheTags.PositionApplication, It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task ApproveOrRejectPositionApplication_ReturnsNotFound_WhenPositionApplicationDoesNotExist()
        {
            using PositionApplicationServiceFactory factory = new PositionApplicationServiceFactory();

            User adminUser = PositionApplicationTestData.CreateUser();
            Role adminRole = PositionApplicationTestData.CreateRole(ApplicationConstants.RoleNames.Admin);
            ApplicationUserRole adminUserRole = PositionApplicationTestData.CreateApplicationUserRole(adminUser, adminRole);

            PositionApplicationStatus approvedStatus = new()
            {
                Id = 3,
                Code = ApplicationConstants.PositionApplicationStatuses.Approved,
                Name = "Approved",
                Active = true
            };

            factory.DbContextFactory.Context.Users.Add(adminUser);
            factory.DbContextFactory.Context.Roles.Add(adminRole);
            factory.DbContextFactory.Context.UserRoles.Add(adminUserRole);
            factory.DbContextFactory.Context.PositionApplicationStatuses.Add(approvedStatus);

            await factory.DbContextFactory.Context.SaveChangesAsync();

            factory.CurrentUserContext.Setup(x => x.UserId)
                .Returns(adminUser.Id);

            ApproveOrRejectPositionApplicationRequest request = new()
            {
                PositionApplicationId = Guid.NewGuid().ToString(),
                PositionApplicationStatusId = approvedStatus.Id
            };

            Result<string> result = await factory.Service.ApproveOrRejectPositionApplication(request);

            Assert.Equal(ResultStatus.NotFound, result.Status);

            factory.UnitOfWork.Verify(x => x.BeginTransactionAsync(), Times.Once);
            factory.UnitOfWork.Verify(x => x.RollbackTransactionAsync(), Times.Once);
            factory.UnitOfWork.Verify(x => x.CommitTransactionAsync(), Times.Never);
        }

        [Fact]
        public async Task ApproveOrRejectPositionApplication_ReturnsConflict_WhenApplicationIsNotPendingReview()
        {
            using PositionApplicationServiceFactory factory = new PositionApplicationServiceFactory();

            User adminUser = PositionApplicationTestData.CreateUser();
            Role adminRole = PositionApplicationTestData.CreateRole(ApplicationConstants.RoleNames.Admin);
            ApplicationUserRole adminUserRole = PositionApplicationTestData.CreateApplicationUserRole(adminUser, adminRole);

            User studentUser = PositionApplicationTestData.CreateUser();
            Student student = PositionApplicationTestData.CreateStudent(studentUser);

            PositionApplicationStatus pendingPaymentStatus = new()
            {
                Id = 1,
                Code = ApplicationConstants.PositionApplicationStatuses.PendingPayment,
                Name = "Pending Payment",
                Active = true
            };

            PositionApplicationStatus approvedStatus = new()
            {
                Id = 3,
                Code = ApplicationConstants.PositionApplicationStatuses.Approved,
                Name = "Approved",
                Active = true
            };

            Election election = PositionApplicationTestData.CreateElection();
            ElectionPosition electionPosition = PositionApplicationTestData.CreateElectionPosition(election);

            PositionApplication positionApplication = PositionApplicationTestData.CreatePositionApplication(student, electionPosition, pendingPaymentStatus);

            factory.DbContextFactory.Context.Users.AddRange(adminUser, studentUser);
            factory.DbContextFactory.Context.Students.Add(student);
            factory.DbContextFactory.Context.Roles.Add(adminRole);
            factory.DbContextFactory.Context.UserRoles.Add(adminUserRole);
            factory.DbContextFactory.Context.PositionApplicationStatuses.AddRange(pendingPaymentStatus, approvedStatus);
            factory.DbContextFactory.Context.Elections.Add(election);
            factory.DbContextFactory.Context.ElectionPositions.Add(electionPosition);
            factory.DbContextFactory.Context.PositionApplications.Add(positionApplication);

            await factory.DbContextFactory.Context.SaveChangesAsync();

            factory.CurrentUserContext.Setup(x => x.UserId)
                .Returns(adminUser.Id);

            ApproveOrRejectPositionApplicationRequest request = new()
            {
                PositionApplicationId = positionApplication.Id,
                PositionApplicationStatusId = approvedStatus.Id
            };

            Result<string> result = await factory.Service.ApproveOrRejectPositionApplication(request);

            Assert.Equal(ResultStatus.Conflict, result.Status);

            factory.UnitOfWork.Verify(x => x.RollbackTransactionAsync(), Times.Once);
            factory.UnitOfWork.Verify(x => x.CommitTransactionAsync(), Times.Never);
        }

        [Fact]
        public async Task ApproveOrRejectPositionApplication_ReturnsConflict_WhenInvoiceDoesNotExist()
        {
            using PositionApplicationServiceFactory factory = new PositionApplicationServiceFactory();

            User adminUser = PositionApplicationTestData.CreateUser();
            Role adminRole = PositionApplicationTestData.CreateRole(ApplicationConstants.RoleNames.Admin);
            ApplicationUserRole adminUserRole = PositionApplicationTestData.CreateApplicationUserRole(adminUser, adminRole);

            User studentUser = PositionApplicationTestData.CreateUser();
            Student student = PositionApplicationTestData.CreateStudent(studentUser);

            PositionApplicationStatus pendingReviewStatus = new()
            {
                Id = 2,
                Code = ApplicationConstants.PositionApplicationStatuses.PendingReview,
                Name = "Pending Review",
                Active = true
            };

            PositionApplicationStatus approvedStatus = new()
            {
                Id = 3,
                Code = ApplicationConstants.PositionApplicationStatuses.Approved,
                Name = "Approved",
                Active = true
            };

            Election election = PositionApplicationTestData.CreateElection();
            ElectionPosition electionPosition = PositionApplicationTestData.CreateElectionPosition(election);

            PositionApplication positionApplication = PositionApplicationTestData.CreatePositionApplication(student, electionPosition, pendingReviewStatus);

            factory.DbContextFactory.Context.Users.AddRange(adminUser, studentUser);
            factory.DbContextFactory.Context.Students.Add(student);
            factory.DbContextFactory.Context.Roles.Add(adminRole);
            factory.DbContextFactory.Context.UserRoles.Add(adminUserRole);
            factory.DbContextFactory.Context.PositionApplicationStatuses.AddRange(pendingReviewStatus, approvedStatus);
            factory.DbContextFactory.Context.Elections.Add(election);
            factory.DbContextFactory.Context.ElectionPositions.Add(electionPosition);
            factory.DbContextFactory.Context.PositionApplications.Add(positionApplication);

            await factory.DbContextFactory.Context.SaveChangesAsync();

            factory.CurrentUserContext.Setup(x => x.UserId)
                .Returns(adminUser.Id);

            ApproveOrRejectPositionApplicationRequest request = new()
            {
                PositionApplicationId = positionApplication.Id,
                PositionApplicationStatusId = approvedStatus.Id
            };

            Result<string> result = await factory.Service.ApproveOrRejectPositionApplication(request);

            Assert.Equal(ResultStatus.Conflict, result.Status);

            factory.UnitOfWork.Verify(x => x.RollbackTransactionAsync(), Times.Once);
            factory.UnitOfWork.Verify(x => x.CommitTransactionAsync(), Times.Never);
        }

        [Fact]
        public async Task ApproveOrRejectPositionApplication_ReturnsConflict_WhenInvoiceIsNotPaid()
        {
            using PositionApplicationServiceFactory factory = new PositionApplicationServiceFactory();

            User adminUser = PositionApplicationTestData.CreateUser();
            Role adminRole = PositionApplicationTestData.CreateRole(ApplicationConstants.RoleNames.Admin);
            ApplicationUserRole adminUserRole = PositionApplicationTestData.CreateApplicationUserRole(adminUser, adminRole);

            User studentUser = PositionApplicationTestData.CreateUser();
            Student student = PositionApplicationTestData.CreateStudent(studentUser);

            PositionApplicationStatus pendingReviewStatus = new()
            {
                Id = 2,
                Code = ApplicationConstants.PositionApplicationStatuses.PendingReview,
                Name = "Pending Review",
                Active = true
            };

            PositionApplicationStatus approvedStatus = new()
            {
                Id = 3,
                Code = ApplicationConstants.PositionApplicationStatuses.Approved,
                Name = "Approved",
                Active = true
            };

            InvoiceStatus unpaidInvoiceStatus = new()
            {
                Id = 1,
                Code = ApplicationConstants.InvoiceStatuses.Unpaid,
                Name = "Unpaid",
                Active = true
            };

            Election election = PositionApplicationTestData.CreateElection();
            ElectionPosition electionPosition = PositionApplicationTestData.CreateElectionPosition(election);

            PositionApplication positionApplication = PositionApplicationTestData.CreatePositionApplication(student, electionPosition, pendingReviewStatus);

            Invoice invoice = new()
            {
                Id = Guid.NewGuid().ToString(),
                PositionApplicationId = positionApplication.Id,
                InvoiceNumber = $"INV-{Guid.NewGuid():N}",
                StudentId = student.Id,
                UserId = studentUser.Id,
                PayerFirstName = studentUser.FirstName,
                PayerLastName = studentUser.LastName,
                PayerEmail = studentUser.Email,
                Amount = 5000m,
                Currency = "NGN",
                InvoiceStatusId = unpaidInvoiceStatus.Id,
                InvoiceStatus = unpaidInvoiceStatus,
                PositionApplication = positionApplication,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            positionApplication.Invoice = invoice;

            factory.DbContextFactory.Context.Users.AddRange(adminUser, studentUser);
            factory.DbContextFactory.Context.Students.Add(student);
            factory.DbContextFactory.Context.Roles.Add(adminRole);
            factory.DbContextFactory.Context.UserRoles.Add(adminUserRole);
            factory.DbContextFactory.Context.PositionApplicationStatuses.AddRange(pendingReviewStatus, approvedStatus);
            factory.DbContextFactory.Context.InvoiceStatuses.Add(unpaidInvoiceStatus);
            factory.DbContextFactory.Context.Elections.Add(election);
            factory.DbContextFactory.Context.ElectionPositions.Add(electionPosition);
            factory.DbContextFactory.Context.PositionApplications.Add(positionApplication);
            factory.DbContextFactory.Context.Invoices.Add(invoice);

            await factory.DbContextFactory.Context.SaveChangesAsync();

            factory.CurrentUserContext.Setup(x => x.UserId)
                .Returns(adminUser.Id);

            ApproveOrRejectPositionApplicationRequest request = new()
            {
                PositionApplicationId = positionApplication.Id,
                PositionApplicationStatusId = approvedStatus.Id
            };

            Result<string> result = await factory.Service.ApproveOrRejectPositionApplication(request);

            Assert.Equal(ResultStatus.Conflict, result.Status);

            factory.UnitOfWork.Verify(x => x.RollbackTransactionAsync(), Times.Once);
            factory.UnitOfWork.Verify(x => x.CommitTransactionAsync(), Times.Never);
        }

        [Fact]
        public async Task GetPositionApplicationsWithContestants_ReturnsOnlyApplicationsWithContestants()
        {
            using PositionApplicationServiceFactory factory = new PositionApplicationServiceFactory();

            User firstUser = PositionApplicationTestData.CreateUser();
            User secondUser = PositionApplicationTestData.CreateUser();

            Student firstStudent = PositionApplicationTestData.CreateStudent(firstUser);
            Student secondStudent = PositionApplicationTestData.CreateStudent(secondUser);

            PositionApplicationStatus approvedStatus = new()
            {
                Id = 3,
                Code = ApplicationConstants.PositionApplicationStatuses.Approved,
                Name = "Approved",
                Active = true
            };

            Election election = new()
            {
                Id = Guid.NewGuid().ToString(),
                Name = "2026 Engineering Election",
                ElectionTypeId = 1,
                YearId = 1
            };

            Position firstPosition = new()
            {
                Id = Guid.NewGuid().ToString(),
                Name = "President",
                Active = true
            };

            Position secondPosition = new()
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Secretary",
                Active = true
            };

            ElectionPosition firstElectionPosition = new()
            {
                Id = Guid.NewGuid().ToString(),
                ElectionId = election.Id,
                PositionId = firstPosition.Id,
                Election = election,
                Position = firstPosition
            };

            ElectionPosition secondElectionPosition = new()
            {
                Id = Guid.NewGuid().ToString(),
                ElectionId = election.Id,
                PositionId = secondPosition.Id,
                Election = election,
                Position = secondPosition
            };

            PositionApplication firstApplication = new()
            {
                Id = Guid.NewGuid().ToString(),
                StudentId = firstStudent.Id,
                ElectionPositionId = firstElectionPosition.Id,
                PositionApplicationStatusId = approvedStatus.Id,
                Student = firstStudent,
                ElectionPosition = firstElectionPosition,
                PositionApplicationStatus = approvedStatus,
                CreatedAt = DateTime.UtcNow
            };

            PositionApplication secondApplication = new()
            {
                Id = Guid.NewGuid().ToString(),
                StudentId = secondStudent.Id,
                ElectionPositionId = secondElectionPosition.Id,
                PositionApplicationStatusId = approvedStatus.Id,
                Student = secondStudent,
                ElectionPosition = secondElectionPosition,
                PositionApplicationStatus = approvedStatus,
                CreatedAt = DateTime.UtcNow.AddMinutes(-1)
            };

            Contestant contestant = new()
            {
                Id = Guid.NewGuid().ToString(),
                PositionApplicationId = firstApplication.Id,
                PositionApplication = firstApplication,
                Active = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            firstApplication.Contestant = contestant;

            factory.DbContextFactory.Context.Users.AddRange(firstUser, secondUser);
            factory.DbContextFactory.Context.Students.AddRange(firstStudent, secondStudent);
            factory.DbContextFactory.Context.PositionApplicationStatuses.Add(approvedStatus);
            factory.DbContextFactory.Context.Elections.Add(election);
            factory.DbContextFactory.Context.Positions.AddRange(firstPosition, secondPosition);
            factory.DbContextFactory.Context.ElectionPositions.AddRange(firstElectionPosition, secondElectionPosition);
            factory.DbContextFactory.Context.PositionApplications.AddRange(firstApplication, secondApplication);
            factory.DbContextFactory.Context.Contestants.Add(contestant);

            await factory.DbContextFactory.Context.SaveChangesAsync();

            factory.Mapper.Setup(x => x.Map<PagedResponse<PositionApplicationResponse>>(It.IsAny<PagedList<PositionApplication>>()))
                .Returns((PagedList<PositionApplication> positionApplications) => new PagedResponse<PositionApplicationResponse>
                {
                    Items = positionApplications.Select(application => new PositionApplicationResponse
                    {
                        PositionApplicationId = application.Id,
                        StudentId = application.StudentId,
                        RegistrationNumber = application.Student.RegNumber,
                        StudentName = application.Student.User is not null
                            ? $"{application.Student.User.FirstName} {application.Student.User.LastName}".Trim()
                            : string.Empty,
                        ElectionPositionId = application.ElectionPositionId,
                        ElectionName = application.ElectionPosition.Election.Name,
                        PositionName = application.ElectionPosition.Position.Name,
                        ApplicationStatus = application.PositionApplicationStatus.Name,
                        Contestant = application.Contestant is null ? null : new ContestantResponse
                        {
                            ContestantId = application.Contestant.Id,
                            PositionApplicationId = application.Contestant.PositionApplicationId,
                            ContestantName = application.Student.User is not null
                                ? $"{application.Student.User.FirstName} {application.Student.User.LastName}".Trim()
                                : string.Empty,
                            RegistrationNumber = application.Student.RegNumber,
                            PositionName = application.ElectionPosition.Position.Name,
                            Active = application.Contestant.Active,
                            CreatedAt = application.Contestant.CreatedAt
                        }
                    }).ToList(),
                    MetaData = positionApplications.MetaData
                });

            PositionApplicationWithContestantRequest request = new()
            {
                PageNumber = 1,
                PageSize = 10
            };

            Result<PagedResponse<PositionApplicationResponse>> result = await factory.Service.GetPositionApplicationsWithContestants(request);

            Assert.Equal(ResultStatus.Success, result.Status);
            Assert.NotNull(result.Value);
            Assert.NotNull(result.Value.Items);
            Assert.Single(result.Value.Items);

            PositionApplicationResponse response = result.Value.Items.Single();

            Assert.Equal(firstApplication.Id, response.PositionApplicationId);
            Assert.NotNull(response.Contestant);
            Assert.Equal(contestant.Id, response.Contestant.ContestantId);
            Assert.Equal(firstPosition.Name, response.Contestant.PositionName);
        }

        private static async Task<(User User, Student Student, Election Election, ElectionPosition ElectionPosition, PositionApplicationStatus PendingPaymentStatus)> CreateValidApplicationData(
            PositionApplicationServiceFactory factory)
        {
            User user = PositionApplicationTestData.CreateUser();
            Student student = PositionApplicationTestData.CreateStudent(user);
            Election election = PositionApplicationTestData.CreateElection();
            ElectionPosition electionPosition = PositionApplicationTestData.CreateElectionPosition(election);
            PositionApplicationStatus pendingPaymentStatus = PositionApplicationTestData.CreatePendingPaymentStatus();

            factory.DbContextFactory.Context.Users.Add(user);
            factory.DbContextFactory.Context.Students.Add(student);
            factory.DbContextFactory.Context.Elections.Add(election);
            factory.DbContextFactory.Context.ElectionPositions.Add(electionPosition);
            factory.DbContextFactory.Context.PositionApplicationStatuses.Add(pendingPaymentStatus);

            await factory.DbContextFactory.Context.SaveChangesAsync();

            return (user, student, election, electionPosition, pendingPaymentStatus);
        }

        [Fact]
        public async Task GetPositionApplicationsWithContestants_WithElectionId_ShouldReturnFilteredApplications()
        {
            using PositionApplicationServiceFactory factory = new PositionApplicationServiceFactory();

            User firstUser = PositionApplicationTestData.CreateUser();
            User secondUser = PositionApplicationTestData.CreateUser();

            Student firstStudent = PositionApplicationTestData.CreateStudent(firstUser);
            Student secondStudent = PositionApplicationTestData.CreateStudent(secondUser);

            PositionApplicationStatus approvedStatus = new()
            {
                Id = 3,
                Code = ApplicationConstants.PositionApplicationStatuses.Approved,
                Name = "Approved",
                Active = true
            };

            Election firstElection = new()
            {
                Id = Guid.NewGuid().ToString(),
                Name = "2026 Engineering Election",
                ElectionTypeId = 1,
                YearId = 1
            };

            Election secondElection = new()
            {
                Id = Guid.NewGuid().ToString(),
                Name = "2026 Science Election",
                ElectionTypeId = 2,
                YearId = 1
            };

            Position firstPosition = new()
            {
                Id = Guid.NewGuid().ToString(),
                Name = "President",
                Active = true
            };

            Position secondPosition = new()
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Secretary",
                Active = true
            };

            ElectionPosition firstElectionPosition = new()
            {
                Id = Guid.NewGuid().ToString(),
                ElectionId = firstElection.Id,
                PositionId = firstPosition.Id,
                Election = firstElection,
                Position = firstPosition
            };

            ElectionPosition secondElectionPosition = new()
            {
                Id = Guid.NewGuid().ToString(),
                ElectionId = secondElection.Id,
                PositionId = secondPosition.Id,
                Election = secondElection,
                Position = secondPosition
            };

            PositionApplication firstApplication = new()
            {
                Id = Guid.NewGuid().ToString(),
                StudentId = firstStudent.Id,
                ElectionPositionId = firstElectionPosition.Id,
                PositionApplicationStatusId = approvedStatus.Id,
                Student = firstStudent,
                ElectionPosition = firstElectionPosition,
                PositionApplicationStatus = approvedStatus,
                CreatedAt = DateTime.UtcNow
            };

            PositionApplication secondApplication = new()
            {
                Id = Guid.NewGuid().ToString(),
                StudentId = secondStudent.Id,
                ElectionPositionId = secondElectionPosition.Id,
                PositionApplicationStatusId = approvedStatus.Id,
                Student = secondStudent,
                ElectionPosition = secondElectionPosition,
                PositionApplicationStatus = approvedStatus,
                CreatedAt = DateTime.UtcNow.AddMinutes(-1)
            };

            Contestant firstContestant = new()
            {
                Id = Guid.NewGuid().ToString(),
                PositionApplicationId = firstApplication.Id,
                PositionApplication = firstApplication,
                Active = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            Contestant secondContestant = new()
            {
                Id = Guid.NewGuid().ToString(),
                PositionApplicationId = secondApplication.Id,
                PositionApplication = secondApplication,
                Active = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            firstApplication.Contestant = firstContestant;
            secondApplication.Contestant = secondContestant;

            factory.DbContextFactory.Context.Users.AddRange(firstUser, secondUser);
            factory.DbContextFactory.Context.Students.AddRange(firstStudent, secondStudent);
            factory.DbContextFactory.Context.PositionApplicationStatuses.Add(approvedStatus);
            factory.DbContextFactory.Context.Elections.AddRange(firstElection, secondElection);
            factory.DbContextFactory.Context.Positions.AddRange(firstPosition, secondPosition);
            factory.DbContextFactory.Context.ElectionPositions.AddRange(firstElectionPosition, secondElectionPosition);
            factory.DbContextFactory.Context.PositionApplications.AddRange(firstApplication, secondApplication);
            factory.DbContextFactory.Context.Contestants.AddRange(firstContestant, secondContestant);

            await factory.DbContextFactory.Context.SaveChangesAsync();

            factory.Mapper.Setup(x => x.Map<PagedResponse<PositionApplicationResponse>>(It.IsAny<PagedList<PositionApplication>>()))
                .Returns((PagedList<PositionApplication> positionApplications) => new PagedResponse<PositionApplicationResponse>
                {
                    Items = positionApplications.Select(application => new PositionApplicationResponse
                    {
                        PositionApplicationId = application.Id,
                        ElectionPositionId = application.ElectionPositionId,
                        ElectionName = application.ElectionPosition.Election.Name,
                        PositionName = application.ElectionPosition.Position.Name
                    }).ToList(),
                    MetaData = positionApplications.MetaData
                });

            PositionApplicationWithContestantRequest request = new()
            {
                PageNumber = 1,
                PageSize = 10,
                ElectionId = firstElection.Id
            };

            Result<PagedResponse<PositionApplicationResponse>> result = await factory.Service.GetPositionApplicationsWithContestants(request);

            Assert.Equal(ResultStatus.Success, result.Status);
            Assert.NotNull(result.Value);
            Assert.NotNull(result.Value.Items);
            Assert.Single(result.Value.Items);
            Assert.Equal(firstApplication.Id, result.Value.Items.Single().PositionApplicationId);
            Assert.Equal(firstElection.Name, result.Value.Items.Single().ElectionName);
        }

        [Fact]
        public async Task GetPositionApplicationsWithContestants_WithPositionId_ShouldReturnFilteredApplications()
        {
            using PositionApplicationServiceFactory factory = new PositionApplicationServiceFactory();

            User firstUser = PositionApplicationTestData.CreateUser();
            User secondUser = PositionApplicationTestData.CreateUser();

            Student firstStudent = PositionApplicationTestData.CreateStudent(firstUser);
            Student secondStudent = PositionApplicationTestData.CreateStudent(secondUser);

            PositionApplicationStatus approvedStatus = new()
            {
                Id = 3,
                Code = ApplicationConstants.PositionApplicationStatuses.Approved,
                Name = "Approved",
                Active = true
            };

            Election election = new()
            {
                Id = Guid.NewGuid().ToString(),
                Name = "2026 Engineering Election",
                ElectionTypeId = 1,
                YearId = 1
            };

            Position presidentPosition = new()
            {
                Id = Guid.NewGuid().ToString(),
                Name = "President",
                Active = true
            };

            Position secretaryPosition = new()
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Secretary",
                Active = true
            };

            ElectionPosition presidentElectionPosition = new()
            {
                Id = Guid.NewGuid().ToString(),
                ElectionId = election.Id,
                PositionId = presidentPosition.Id,
                Election = election,
                Position = presidentPosition
            };

            ElectionPosition secretaryElectionPosition = new()
            {
                Id = Guid.NewGuid().ToString(),
                ElectionId = election.Id,
                PositionId = secretaryPosition.Id,
                Election = election,
                Position = secretaryPosition
            };

            PositionApplication presidentApplication = PositionApplicationTestData.CreatePositionApplication(firstStudent, presidentElectionPosition, approvedStatus);
            PositionApplication secretaryApplication = PositionApplicationTestData.CreatePositionApplication(secondStudent, secretaryElectionPosition, approvedStatus);

            Contestant presidentContestant = new()
            {
                Id = Guid.NewGuid().ToString(),
                PositionApplicationId = presidentApplication.Id,
                PositionApplication = presidentApplication,
                Active = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            Contestant secretaryContestant = new()
            {
                Id = Guid.NewGuid().ToString(),
                PositionApplicationId = secretaryApplication.Id,
                PositionApplication = secretaryApplication,
                Active = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            presidentApplication.Contestant = presidentContestant;
            secretaryApplication.Contestant = secretaryContestant;

            factory.DbContextFactory.Context.Users.AddRange(firstUser, secondUser);
            factory.DbContextFactory.Context.Students.AddRange(firstStudent, secondStudent);
            factory.DbContextFactory.Context.PositionApplicationStatuses.Add(approvedStatus);
            factory.DbContextFactory.Context.Elections.Add(election);
            factory.DbContextFactory.Context.Positions.AddRange(presidentPosition, secretaryPosition);
            factory.DbContextFactory.Context.ElectionPositions.AddRange(presidentElectionPosition, secretaryElectionPosition);
            factory.DbContextFactory.Context.PositionApplications.AddRange(presidentApplication, secretaryApplication);
            factory.DbContextFactory.Context.Contestants.AddRange(presidentContestant, secretaryContestant);

            await factory.DbContextFactory.Context.SaveChangesAsync();

            factory.Mapper.Setup(x => x.Map<PagedResponse<PositionApplicationResponse>>(It.IsAny<PagedList<PositionApplication>>()))
                .Returns((PagedList<PositionApplication> positionApplications) => new PagedResponse<PositionApplicationResponse>
                {
                    Items = positionApplications.Select(application => new PositionApplicationResponse
                    {
                        PositionApplicationId = application.Id,
                        PositionName = application.ElectionPosition.Position.Name
                    }).ToList(),
                    MetaData = positionApplications.MetaData
                });

            PositionApplicationWithContestantRequest request = new()
            {
                PageNumber = 1,
                PageSize = 10,
                PositionId = presidentPosition.Id
            };

            Result<PagedResponse<PositionApplicationResponse>> result = await factory.Service.GetPositionApplicationsWithContestants(request);

            Assert.Equal(ResultStatus.Success, result.Status);
            Assert.NotNull(result.Value);
            Assert.NotNull(result.Value.Items);
            Assert.Single(result.Value.Items);
            Assert.Equal(presidentApplication.Id, result.Value.Items.Single().PositionApplicationId);
            Assert.Equal("President", result.Value.Items.Single().PositionName);
        }

        [Fact]
        public async Task GetPositionApplicationsWithContestants_WithContestantActive_ShouldReturnFilteredApplications()
        {
            using PositionApplicationServiceFactory factory = new PositionApplicationServiceFactory();

            User firstUser = PositionApplicationTestData.CreateUser();
            User secondUser = PositionApplicationTestData.CreateUser();

            Student firstStudent = PositionApplicationTestData.CreateStudent(firstUser);
            Student secondStudent = PositionApplicationTestData.CreateStudent(secondUser);

            PositionApplicationStatus approvedStatus = new()
            {
                Id = 3,
                Code = ApplicationConstants.PositionApplicationStatuses.Approved,
                Name = "Approved",
                Active = true
            };

            Election election = new()
            {
                Id = Guid.NewGuid().ToString(),
                Name = "2026 Engineering Election",
                ElectionTypeId = 1,
                YearId = 1
            };

            Position firstPosition = new()
            {
                Id = Guid.NewGuid().ToString(),
                Name = "President",
                Active = true
            };

            Position secondPosition = new()
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Secretary",
                Active = true
            };

            ElectionPosition firstElectionPosition = new()
            {
                Id = Guid.NewGuid().ToString(),
                ElectionId = election.Id,
                PositionId = firstPosition.Id,
                Election = election,
                Position = firstPosition
            };

            ElectionPosition secondElectionPosition = new()
            {
                Id = Guid.NewGuid().ToString(),
                ElectionId = election.Id,
                PositionId = secondPosition.Id,
                Election = election,
                Position = secondPosition
            };

            PositionApplication activeApplication = PositionApplicationTestData.CreatePositionApplication(firstStudent, firstElectionPosition, approvedStatus);
            PositionApplication inactiveApplication = PositionApplicationTestData.CreatePositionApplication(secondStudent, secondElectionPosition, approvedStatus);

            Contestant activeContestant = new()
            {
                Id = Guid.NewGuid().ToString(),
                PositionApplicationId = activeApplication.Id,
                PositionApplication = activeApplication,
                Active = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            Contestant inactiveContestant = new()
            {
                Id = Guid.NewGuid().ToString(),
                PositionApplicationId = inactiveApplication.Id,
                PositionApplication = inactiveApplication,
                Active = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            activeApplication.Contestant = activeContestant;
            inactiveApplication.Contestant = inactiveContestant;

            factory.DbContextFactory.Context.Users.AddRange(firstUser, secondUser);
            factory.DbContextFactory.Context.Students.AddRange(firstStudent, secondStudent);
            factory.DbContextFactory.Context.PositionApplicationStatuses.Add(approvedStatus);
            factory.DbContextFactory.Context.Elections.Add(election);
            factory.DbContextFactory.Context.Positions.AddRange(firstPosition, secondPosition);
            factory.DbContextFactory.Context.ElectionPositions.AddRange(firstElectionPosition, secondElectionPosition);
            factory.DbContextFactory.Context.PositionApplications.AddRange(activeApplication, inactiveApplication);
            factory.DbContextFactory.Context.Contestants.AddRange(activeContestant, inactiveContestant);

            await factory.DbContextFactory.Context.SaveChangesAsync();

            factory.Mapper.Setup(x => x.Map<PagedResponse<PositionApplicationResponse>>(It.IsAny<PagedList<PositionApplication>>()))
                .Returns((PagedList<PositionApplication> positionApplications) => new PagedResponse<PositionApplicationResponse>
                {
                    Items = positionApplications.Select(application => new PositionApplicationResponse
                    {
                        PositionApplicationId = application.Id,
                        Contestant = application.Contestant is null ? null : new ContestantResponse
                        {
                            ContestantId = application.Contestant.Id,
                            PositionApplicationId = application.Contestant.PositionApplicationId,
                            Active = application.Contestant.Active
                        }
                    }).ToList(),
                    MetaData = positionApplications.MetaData
                });

            PositionApplicationWithContestantRequest request = new()
            {
                PageNumber = 1,
                PageSize = 10,
                ContestantActive = true
            };

            Result<PagedResponse<PositionApplicationResponse>> result = await factory.Service.GetPositionApplicationsWithContestants(request);

            Assert.Equal(ResultStatus.Success, result.Status);
            Assert.NotNull(result.Value);
            Assert.NotNull(result.Value.Items);
            Assert.Single(result.Value.Items);

            PositionApplicationResponse response = result.Value.Items.Single();

            Assert.Equal(activeApplication.Id, response.PositionApplicationId);
            Assert.NotNull(response.Contestant);
            Assert.True(response.Contestant.Active);
        }

        [Fact]
        public async Task GetPositionApplicationsWithContestants_WithSearchTerm_ShouldReturnMatchingApplications()
        {
            using PositionApplicationServiceFactory factory = new PositionApplicationServiceFactory();

            User matchingUser = PositionApplicationTestData.CreateUser();
            matchingUser.FirstName = "Obinna";
            matchingUser.LastName = "Achara";

            User nonMatchingUser = PositionApplicationTestData.CreateUser();
            nonMatchingUser.FirstName = "John";
            nonMatchingUser.LastName = "Doe";

            Student matchingStudent = PositionApplicationTestData.CreateStudent(matchingUser);
            Student nonMatchingStudent = PositionApplicationTestData.CreateStudent(nonMatchingUser);

            PositionApplicationStatus approvedStatus = new()
            {
                Id = 3,
                Code = ApplicationConstants.PositionApplicationStatuses.Approved,
                Name = "Approved",
                Active = true
            };

            Election election = new()
            {
                Id = Guid.NewGuid().ToString(),
                Name = "2026 Engineering Election",
                ElectionTypeId = 1,
                YearId = 1
            };

            Position presidentPosition = new()
            {
                Id = Guid.NewGuid().ToString(),
                Name = "President",
                Active = true
            };

            Position secretaryPosition = new()
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Secretary",
                Active = true
            };

            ElectionPosition presidentElectionPosition = new()
            {
                Id = Guid.NewGuid().ToString(),
                ElectionId = election.Id,
                PositionId = presidentPosition.Id,
                Election = election,
                Position = presidentPosition
            };

            ElectionPosition secretaryElectionPosition = new()
            {
                Id = Guid.NewGuid().ToString(),
                ElectionId = election.Id,
                PositionId = secretaryPosition.Id,
                Election = election,
                Position = secretaryPosition
            };

            PositionApplication matchingApplication = PositionApplicationTestData.CreatePositionApplication(
                matchingStudent, presidentElectionPosition, approvedStatus);

            PositionApplication nonMatchingApplication = PositionApplicationTestData.CreatePositionApplication(
                nonMatchingStudent, secretaryElectionPosition, approvedStatus);

            Contestant matchingContestant = new()
            {
                Id = Guid.NewGuid().ToString(),
                PositionApplicationId = matchingApplication.Id,
                PositionApplication = matchingApplication,
                Active = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            Contestant nonMatchingContestant = new()
            {
                Id = Guid.NewGuid().ToString(),
                PositionApplicationId = nonMatchingApplication.Id,
                PositionApplication = nonMatchingApplication,
                Active = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            matchingApplication.Contestant = matchingContestant;
            nonMatchingApplication.Contestant = nonMatchingContestant;

            factory.DbContextFactory.Context.Users.AddRange(matchingUser, nonMatchingUser);
            factory.DbContextFactory.Context.Students.AddRange(matchingStudent, nonMatchingStudent);
            factory.DbContextFactory.Context.PositionApplicationStatuses.Add(approvedStatus);
            factory.DbContextFactory.Context.Elections.Add(election);
            factory.DbContextFactory.Context.Positions.AddRange(presidentPosition, secretaryPosition);
            factory.DbContextFactory.Context.ElectionPositions.AddRange(presidentElectionPosition, secretaryElectionPosition);
            factory.DbContextFactory.Context.PositionApplications.AddRange(matchingApplication, nonMatchingApplication);
            factory.DbContextFactory.Context.Contestants.AddRange(matchingContestant, nonMatchingContestant);

            await factory.DbContextFactory.Context.SaveChangesAsync();

            factory.Mapper.Setup(x => x.Map<PagedResponse<PositionApplicationResponse>>(It.IsAny<PagedList<PositionApplication>>()))
                .Returns((PagedList<PositionApplication> positionApplications) => new PagedResponse<PositionApplicationResponse>
                {
                    Items = positionApplications.Select(application => new PositionApplicationResponse
                    {
                        PositionApplicationId = application.Id,
                        StudentName = application.Student.User is not null
                            ? $"{application.Student.User.FirstName} {application.Student.User.LastName}".Trim()
                            : string.Empty,
                        ElectionName = application.ElectionPosition.Election.Name,
                        PositionName = application.ElectionPosition.Position.Name,
                        Contestant = application.Contestant is null ? null : new ContestantResponse
                        {
                            ContestantId = application.Contestant.Id,
                            PositionApplicationId = application.Contestant.PositionApplicationId,
                            ContestantName = application.Student.User is not null
                                ? $"{application.Student.User.FirstName} {application.Student.User.LastName}".Trim()
                                : string.Empty,
                            PositionName = application.ElectionPosition.Position.Name,
                            Active = application.Contestant.Active
                        }
                    }).ToList(),
                    MetaData = positionApplications.MetaData
                });

            PositionApplicationWithContestantRequest request = new()
            {
                PageNumber = 1,
                PageSize = 10,
                SearchTerm = "Obinna"
            };

            Result<PagedResponse<PositionApplicationResponse>> result =
                await factory.Service.GetPositionApplicationsWithContestants(request);

            Assert.Equal(ResultStatus.Success, result.Status);
            Assert.NotNull(result.Value);
            Assert.NotNull(result.Value.Items);
            Assert.Single(result.Value.Items);

            PositionApplicationResponse response = result.Value.Items.Single();

            Assert.Equal(matchingApplication.Id, response.PositionApplicationId);
            Assert.Equal("Obinna Achara", response.StudentName);
            Assert.Equal("President", response.PositionName);
            Assert.NotNull(response.Contestant);
            Assert.Equal("Obinna Achara", response.Contestant.ContestantName);
        }

        private static void SetupInvoiceRequestMapping(PositionApplicationServiceFactory factory, User user, Student student, ElectionPosition electionPosition)
        {
            factory.Mapper.Setup(x => x.Map<CreateInvoiceRequest>(It.IsAny<Student>()))
                .Returns(new CreateInvoiceRequest
                {
                    StudentId = student.Id,
                    UserId = user.Id,
                    PayerFirstName = user.FirstName ?? string.Empty,
                    PayerLastName = user.LastName ?? string.Empty,
                    PayerEmail = user.Email,
                    RegistrationNumber = student.RegNumber,
                    Amount = electionPosition.ApplicationFee,
                    Currency = electionPosition.Currency
                });
        }

        private static void SetupPositionApplicationMapping(PositionApplicationServiceFactory factory, PositionApplicationStatus pendingPaymentStatus)
        {
            factory.Mapper.Setup(x => x.Map<PositionApplication>(It.IsAny<CreatePositionApplicationRequest>()))
                .Returns(() => new PositionApplication
                {
                    Id = Guid.NewGuid().ToString(),
                    PositionApplicationStatus = pendingPaymentStatus
                });

            factory.Mapper.Setup(x => x.Map<CreatePositionApplicationResponse>(It.IsAny<PositionApplication>()))
                .Returns((PositionApplication application) => new CreatePositionApplicationResponse
                {
                    PositionApplicationId = application.Id,
                    ApplicationStatus = application.PositionApplicationStatus.Name
                });
        }

        private static string CreateRequestHash(string electionPositionId)
        {
            string requestValue = electionPositionId.Trim().ToLowerInvariant();
            byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(requestValue));

            return Convert.ToHexString(hash);
        }
    }
}