using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using OpenAlprWebhookProcessor.Data;
using OpenAlprWebhookProcessor.Data.Repositories;
using OpenAlprWebhookProcessor.Features.Users.Data;
using OpenAlprWebhookProcessor.Features.Users.Data.Repositories;
using Mediator;
using NUnit.Framework;

namespace Tests.TestHelpers
{
    [Parallelizable(ParallelScope.Self)]
    [TestFixture]
    public abstract class TestBase
    {
        protected EfContextCreator ContextCreator { get; private set; }

        protected ProcessorContext Context { get; private set; }

        protected UsersContext UsersContext { get; private set; }

        protected IUnitOfWork UnitOfWork { get; private set; }

        protected IUsersUnitOfWork UsersUnitOfWork { get; private set; }

        protected IMediator Mediator { get; private set; }

        [SetUp]
        public virtual void SetUp()
        {
            ContextCreator = new EfContextCreator();
            Context = ContextCreator.CreateContext();
            UsersContext = ContextCreator.CreateUsersContext();
            
            UnitOfWork = new UnitOfWork(Context);
            UsersUnitOfWork = new UsersUnitOfWork(UsersContext);

            Mediator = Substitute.For<IMediator>();
        }

        [TearDown]
        public virtual void TearDown()
        {
            UnitOfWork?.Dispose();
            UsersUnitOfWork?.Dispose();
            Context?.Dispose();
            UsersContext?.Dispose();
            ContextCreator?.Dispose();
        }

        protected static T GetControllerResult<T>(IActionResult result)
        {
            return result switch
            {
                OkObjectResult okResult => (T)okResult.Value,
                ObjectResult objectResult => (T)objectResult.Value,
                _ => throw new InvalidOperationException($"Unexpected result type: {result.GetType()}")
            };
        }

        protected static T GetControllerActionResult<T>(ActionResult<T> result)
        {
            return result.Value ?? GetControllerResult<T>(result.Result);
        }

        protected static void AssertOkResult(IActionResult result)
        {
            Assert.That(result, Is.InstanceOf<OkResult>().Or.InstanceOf<OkObjectResult>());
        }

        protected static void AssertOkResult<T>(ActionResult<T> result)
        {
            if (result.Value != null)
            {
                Assert.That(result.Value, Is.Not.Null);
            }
            else
            {
                AssertOkResult(result.Result);
            }
        }

        protected static void AssertBadRequestResult(IActionResult result)
        {
            Assert.That(result, Is.InstanceOf<BadRequestResult>().Or.InstanceOf<BadRequestObjectResult>());
        }

        protected static void AssertNotFoundResult(IActionResult result)
        {
            Assert.That(result, Is.InstanceOf<NotFoundResult>().Or.InstanceOf<NotFoundObjectResult>());
        }

        protected CancellationToken GetCancellationToken()
        {
            return CancellationToken.None;
        }
    }
} 