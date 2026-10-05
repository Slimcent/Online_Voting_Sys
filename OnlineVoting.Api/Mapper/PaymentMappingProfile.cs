using AutoMapper;
using OnlineVoting.Models.Dtos.Request;
using OnlineVoting.Models.Dtos.Request.Payments;
using OnlineVoting.Models.Dtos.Response;
using OnlineVoting.Models.Dtos.Response.Payments;
using OnlineVoting.Models.Entities;

namespace OnlineVoting.Api.Mapper
{
    public class PaymentMappingProfile : Profile
    {
        public PaymentMappingProfile()
        {
            CreateMap<Student, CreateInvoiceRequest>()
                .ForMember(destination => destination.PositionApplicationId, option => option.Ignore())
                .ForMember(destination => destination.StudentId, option => option.MapFrom(source => source.Id))
                .ForMember(destination => destination.UserId, option => option.MapFrom(source => source.User!.Id))
                .ForMember(destination => destination.PayerFirstName, option => option.MapFrom(source => source.User!.FirstName ?? string.Empty))
                .ForMember(destination => destination.PayerLastName, option => option.MapFrom(source => source.User!.LastName ?? string.Empty))
                .ForMember(destination => destination.PayerEmail, option => option.MapFrom(source => source.User!.Email))
                .ForMember(destination => destination.RegistrationNumber, option => option.MapFrom(source => source.RegNumber))
                .ForMember(destination => destination.Amount, option => option.Ignore())
                .ForMember(destination => destination.Currency, option => option.Ignore());

            CreateMap<CreateInvoiceRequest, Invoice>()
                .ForMember(destination => destination.Id, option => option.Ignore())
                .ForMember(destination => destination.InvoiceNumber, option => option.Ignore())
                .ForMember(destination => destination.InvoiceStatusId, option => option.Ignore())
                .ForMember(destination => destination.PaidAt, option => option.Ignore())
                .ForMember(destination => destination.Currency, option => option.MapFrom(source => source.Currency.Trim()));

            CreateMap<Invoice, InvoiceResponse>()
                .ForMember(destination => destination.Status, option => option.MapFrom(source => source.InvoiceStatus.Name));

            CreateMap<Invoice, PaymentTransaction>()
                .ForMember(destination => destination.Id, option => option.Ignore())
                .ForMember(destination => destination.InvoiceId, option => option.MapFrom(source => source.Id))
                .ForMember(destination => destination.PaymentGatewayId, option => option.Ignore())
                .ForMember(destination => destination.PaymentStatusId, option => option.Ignore())
                .ForMember(destination => destination.PaymentReference, option => option.Ignore())
                .ForMember(destination => destination.ProviderReference, option => option.Ignore())
                .ForMember(destination => destination.CheckoutUrl, option => option.Ignore())
                .ForMember(destination => destination.FailureReason, option => option.Ignore())
                .ForMember(destination => destination.PaidAt, option => option.Ignore())
                .ForMember(destination => destination.CreatedAt, option => option.Ignore())
                .ForMember(destination => destination.UpdatedAt, option => option.Ignore())
                .ForMember(destination => destination.CreatedBy, option => option.Ignore())
                .ForMember(destination => destination.UpdatedBy, option => option.Ignore())
                .ForMember(destination => destination.Invoice, option => option.Ignore())
                .ForMember(destination => destination.PaymentGateway, option => option.Ignore())
                .ForMember(destination => destination.PaymentStatus, option => option.Ignore());

            CreateMap<PaymentTransaction, GatewayPaymentInitializationRequest>()
                .ForMember(destination => destination.Email, option => option.MapFrom(source => source.PayerEmail));

            CreateMap<PaymentTransaction, InitiatePaymentResponse>()
                .ForMember(destination => destination.PaymentTransactionId, option => option.MapFrom(source => source.Id))
                .ForMember(destination => destination.PaymentGatewayCode, option => option.Ignore())
                .ForMember(destination => destination.PaymentStatus, option => option.Ignore());

            CreateMap<PaymentTransaction, PaymentVerificationResponse>()
                .ForMember(destination => destination.PaymentTransactionId, option => option.MapFrom(source => source.Id))
                .ForMember(destination => destination.PaymentReference, option => option.MapFrom(source => source.PaymentReference))
                .ForMember(destination => destination.ProviderReference, option => option.MapFrom(source => source.ProviderReference))
                .ForMember(destination => destination.PaymentStatus, option => option.MapFrom(source => source.PaymentStatus.Name))
                .ForMember(destination => destination.InvoiceStatus, option => option.MapFrom(source => source.Invoice.InvoiceStatus.Name))
                .ForMember(destination => destination.Amount, option => option.MapFrom(source => source.Amount))
                .ForMember(destination => destination.Currency, option => option.MapFrom(source => source.Currency))
                .ForMember(destination => destination.PaidAt, option => option.MapFrom(source => source.PaidAt));

            CreateMap<GatewayPaymentVerificationResponse, PaymentVerificationResponse>()
                .ForMember(destination => destination.ProviderReference, option =>
                {
                    option.Condition(source => !string.IsNullOrWhiteSpace(source.ProviderReference));
                    option.MapFrom(source => source.ProviderReference);
                })
                .ForMember(destination => destination.PaymentStatus, option => option.MapFrom(source => source.Status))
                .ForMember(destination => destination.PaymentTransactionId, option => option.Ignore())
                .ForMember(destination => destination.PaymentReference, option => option.Ignore())
                .ForMember(destination => destination.InvoiceStatus, option => option.Ignore())
                .ForMember(destination => destination.Amount, option => option.Ignore())
                .ForMember(destination => destination.Currency, option => option.Ignore())
                .ForMember(destination => destination.PaidAt, option => option.Ignore());

            CreateMap<PaymentTransaction, PaymentTransactionResponse>()
                .ForMember(destination => destination.PaymentTransactionId, option => option.MapFrom(source => source.Id))
                .ForMember(destination => destination.PaymentGateway, option => option.MapFrom(source => source.PaymentGateway.Name))
                .ForMember(destination => destination.PaymentStatus, option => option.MapFrom(source => source.PaymentStatus.Name));
        }
    }
}
