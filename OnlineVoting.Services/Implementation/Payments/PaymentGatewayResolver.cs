using OnlineVoting.Services.Interfaces.Payments;

namespace OnlineVoting.Services.Implementation.Payments
{
    public class PaymentGatewayResolver : IPaymentGatewayResolver
    {
        private readonly IEnumerable<IPaymentGateway> _paymentGateways;

        public PaymentGatewayResolver(IEnumerable<IPaymentGateway> paymentGateways)
        {
            _paymentGateways = paymentGateways;
        }

        public IPaymentGateway? Resolve(string code)
        {
            return _paymentGateways.FirstOrDefault(x => x.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
        }
    }
}