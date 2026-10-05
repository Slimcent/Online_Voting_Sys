using Moq;
using OnlineVoting.Models.Constants;
using OnlineVoting.Services.Implementation.Payments;
using OnlineVoting.Services.Interfaces.Payments;

namespace OnlineVoting.Tests.UnitTests.Services
{
    public class PaymentGatewayResolverTests
    {
        [Fact]
        public void Resolve_WithPaystackCode_ShouldReturnPaystackGateway()
        {
            Mock<IPaymentGateway> paystackGateway = new Mock<IPaymentGateway>();
            paystackGateway.SetupGet(x => x.Code).Returns(ApplicationConstants.PaymentGateways.Paystack);

            Mock<IPaymentGateway> otherGateway = new Mock<IPaymentGateway>();
            otherGateway.SetupGet(x => x.Code).Returns("OTHER");

            List<IPaymentGateway> paymentGateways = new List<IPaymentGateway>
            {
                paystackGateway.Object,
                otherGateway.Object
            };

            PaymentGatewayResolver resolver = new PaymentGatewayResolver(paymentGateways);

            IPaymentGateway? result = resolver.Resolve(ApplicationConstants.PaymentGateways.Paystack);

            Assert.NotNull(result);
            Assert.Same(paystackGateway.Object, result);
        }

        [Fact]
        public void Resolve_WithMatchingCodeIgnoringCase_ShouldReturnGateway()
        {
            Mock<IPaymentGateway> paystackGateway = new Mock<IPaymentGateway>();
            paystackGateway.SetupGet(x => x.Code).Returns(ApplicationConstants.PaymentGateways.Paystack);

            List<IPaymentGateway> paymentGateways = new List<IPaymentGateway>
            {
                paystackGateway.Object
            };

            PaymentGatewayResolver resolver = new PaymentGatewayResolver(paymentGateways);

            IPaymentGateway? result = resolver.Resolve(ApplicationConstants.PaymentGateways.Paystack.ToLowerInvariant());

            Assert.NotNull(result);
            Assert.Same(paystackGateway.Object, result);
        }

        [Fact]
        public void Resolve_WithUnknownCode_ShouldReturnNull()
        {
            Mock<IPaymentGateway> paystackGateway = new Mock<IPaymentGateway>();
            paystackGateway.SetupGet(x => x.Code).Returns(ApplicationConstants.PaymentGateways.Paystack);

            List<IPaymentGateway> paymentGateways = new List<IPaymentGateway>
            {
                paystackGateway.Object
            };

            PaymentGatewayResolver resolver = new PaymentGatewayResolver(paymentGateways);

            IPaymentGateway? result = resolver.Resolve("UNKNOWN");

            Assert.Null(result);
        }

        [Fact]
        public void Resolve_WithEmptyGateways_ShouldReturnNull()
        {
            List<IPaymentGateway> paymentGateways = new List<IPaymentGateway>();

            PaymentGatewayResolver resolver = new PaymentGatewayResolver(paymentGateways);

            IPaymentGateway? result = resolver.Resolve(ApplicationConstants.PaymentGateways.Paystack);

            Assert.Null(result);
        }
    }
}