namespace OnlineVoting.Services.Interfaces.Payments
{
    public interface IPaymentGatewayResolver
    {
        IPaymentGateway? Resolve(string code);
    }
}
